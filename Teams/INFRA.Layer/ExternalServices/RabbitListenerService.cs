using System.Text;
using System.Text.RegularExpressions;
using CustomVaultPackage.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
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

    // Pattern: | GUID | TeamName
    // Project affected | GUID | TeamName (eg. Project affected | b14db1e2-026e-4ac9-9739-378720de6f5b | Pentester)
    // Member to add | GUID | TeamName (eg. Member to add | 12345678-90ab-cdef-1234-567890abcdef | Pentester)
    // Member to delete | GUID | TeamName (eg. Member to delete | 12345678-90ab-cdef-1234-567890abcdef | Pentester)
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

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
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

            // Keep service alive while not cancelled
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
                var TeamManagerId = new Guid(match.Groups[1].Value);
                var teamName = match.Groups[3].Value.Trim();
                if (message.Contains("Member to add", StringComparison.OrdinalIgnoreCase))
                    backgroundJob.ScheduleAddTeamMemberAsync(identifier);
                else if (message.Contains("Member to delete", StringComparison.OrdinalIgnoreCase))
                    backgroundJob.ScheduleDeleteTeamMemberAsync(identifier, teamName);
                else if (message.Contains("Project affected", StringComparison.OrdinalIgnoreCase))
                    backgroundJob.ScheduleAddProjectToTeamAsync(TeamManagerId, teamName);
                else if (message.Contains("Project suspended", StringComparison.OrdinalIgnoreCase))
                    backgroundJob.ScheduleAddProjectToTeamAsync(TeamManagerId, teamName);
                else
                    throw new InvalidOperationException("Unrecognized message type");

                _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                LogHelper.Info(
                    $"🔔 Message processed for member or manager {identifier} in team {teamName}",
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
        var vault = new HashicorpVaultService(configuration);
        var connectionString = await vault.GetRabbitConnectionStringFromVault();
        return new ConnectionFactory
        {
            Uri = new Uri("amqp://" + connectionString),
            DispatchConsumersAsync = false,
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
