using System.Text;
using System.Text.RegularExpressions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;

namespace Teams.INFRA.Layer.ExternalServices;

public partial class RabbitListenerService(
    ILogger<RabbitListenerService> log,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    private const string QueueName = "team_management";
    private IConnection? _connection;
    private IModel? _channel;

    [GeneratedRegex(
        @"\|\s*([\da-f]{8}(-[\da-f]{4}){3}-[\da-f]{12})\s*\|\s*(.+)",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex GuidAndTeamRegex();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogHelper.Info("🟢 RabbitListenerService started", log);

        try
        {
            await InitializeRabbitMqConnectionAsync();
            using var scope = scopeFactory.CreateScope();
            var backgroundJob = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                if (string.IsNullOrWhiteSpace(message))
                {
                    LogHelper.Warning("Received empty message, skipping processing.", log);
                    _channel?.BasicNack(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: false
                    );
                    return;
                }
                LogHelper.Info($"📩 Message received: {message}", log);

                ProcessMessage(message, ea, backgroundJob);
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            LogHelper.Info(
                $"📡 RabbitMQ consumer started successfully for queue: {QueueName}",
                log
            );
            while (!stoppingToken.IsCancellationRequested && _connection?.IsOpen == true)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            HandleServiceError(ex);
        }
    }
    private void ProcessMessage(
        string message,
        BasicDeliverEventArgs ea,
        IBackgroundJobService backgroundJob
    )
    {
        var match = GuidAndTeamRegex().Match(message);
        try
        {
            if (match.Success && Guid.TryParse(match.Groups[1].Value, out Guid identifier))
            {
                var projectOperationId = new Guid(match.Groups[1].Value);
                var projectOperationName = match.Groups[3].Value.Trim();
                if (message.Contains("Member to add", StringComparison.OrdinalIgnoreCase))
                    backgroundJob.ScheduleAddTeamMemberAsync(identifier);
                else if (message.Contains("Member to delete", StringComparison.OrdinalIgnoreCase))
                    backgroundJob.ScheduleDeleteTeamMemberAsync(identifier, projectOperationName);
                else if (message.Contains("Project affected", StringComparison.OrdinalIgnoreCase))
                    backgroundJob.ScheduleAddProjectToTeamAsync(projectOperationId, projectOperationName);
                else if (message.Contains("Project suspended", StringComparison.OrdinalIgnoreCase))
                    backgroundJob.ScheduleRemoveProjectToTeamAsync(projectOperationId, projectOperationName);
                else
                    throw new InvalidOperationException("Unrecognized message type");

                _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                LogHelper.Info(
                    $"🔔 Message processed for member or manager {identifier} in team {projectOperationName}",
                    log
                );
            }
            else
            {
                LogHelper.Warning($"⚠️ Unrecognized message: {message}", log);
                _channel?.BasicReject(ea.DeliveryTag, requeue: false);
            }
        }
        catch (Exception ex)
        {
            HandleMessageError(ex, ea, match.Groups[1].Value);
        }
    }
    private async Task InitializeRabbitMqConnectionAsync()
    {
        var factory = await EstablishConnection();
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
    }
    private async Task<ConnectionFactory> EstablishConnection()
    {
        var rabbitSection = configuration.GetSection("RabbitMQ");
        string connectionString = $"{rabbitSection["UserName"]}:{rabbitSection["Password"]}@{rabbitSection["ConnectionString"]}";
        await Task.Delay(500);
        return new ConnectionFactory
        {
            Uri = new Uri("amqp://" + connectionString),
            DispatchConsumersAsync = true,
            Ssl = new SslOption
            {
                Enabled = false
            }
        };
    }
    private void HandleMessageError(Exception ex, BasicDeliverEventArgs ea, string guidValue)
    {
        LogHelper.Error(
            $"❌ Error processing message. GUID: {guidValue}. Exception: {ex.Message}",
            log
        );
        LogHelper.CriticalFailure(
            log,
            "RabbitListenerService",
            "Error processing RabbitMQ message",
            ex
        );
        _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
    }

    private void HandleServiceError(Exception ex)
    {
        LogHelper.Error($"❌ Error in RabbitListenerService: {ex.Message}", log);
        _channel?.Close();
        _connection?.Close();
        LogHelper.CriticalFailure(
            log,
            "RabbitListenerService",
            "Exception in RabbitListenerService",
            ex
        );
    }
    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
