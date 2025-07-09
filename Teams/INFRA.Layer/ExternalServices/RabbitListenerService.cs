using System.Text;
using System.Text.RegularExpressions;
using CustomVaultPackage.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Teams.APP.Layer.Interfaces;

namespace Teams.INFRA.Layer.ExternalServices;

public partial class RabbitListenerService(
    ILogger<RabbitListenerService> log,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    /** Member to Add    | 12345678-90ab-cdef-1234-567890abcdef | Pentester
        Member to delete | 12345678-90ab-cdef-1234-567890abcdef | Pentester
        Project Affected
    **/
    private IConnection? _connection;
    private IModel? _channel;

    [GeneratedRegex(
        @"\|\s*([\da-f]{8}(-[\da-f]{4}){3}-[\da-f]{12})\s*\|\s*(.+)",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex GuidAndTeamRegex();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("🟢 RabbitListenerService démarré");

        try
        {
            var factory = await EstablishConnection();
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            const string queueName = "team_management";

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                log.LogInformation("📨 Message reçu : {Message}", message);

                var match = GuidAndTeamRegex().Match(message);

                if (match.Success && Guid.TryParse(match.Groups[1].Value, out Guid memberId))
                {
                    var teamName = match.Groups[2].Value;
                    try
                    {
                        // 👇 Création d'un scope local
                        using var scope = scopeFactory.CreateScope();
                        var backgroundJob =
                            scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

                        if (message.Contains("Member to add", StringComparison.OrdinalIgnoreCase))
                            backgroundJob.ScheduleAddTeamMemberAsync(memberId);
                        else if (
                            message.Contains("Member to delete", StringComparison.OrdinalIgnoreCase)
                        )
                            backgroundJob.ScheduleDeleteTeamMemberAsync(memberId, teamName);
                        else if (
                            message.Contains("Project Affected", StringComparison.OrdinalIgnoreCase)
                        )
                            backgroundJob.ScheduleProjectAssociationAsync();
                        else
                            throw new InvalidOperationException("Message non reconnu");

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        log.LogInformation("✅ Message traité et ack envoyé");
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "❌ Erreur lors du traitement du message RabbitMQ");
                        _channel.BasicNack(
                            deliveryTag: ea.DeliveryTag,
                            multiple: false,
                            requeue: true
                        );
                    }
                }
                else
                {
                    log.LogWarning("⚠️ Message mal formé ou GUID invalide : {Message}", message);
                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            log.LogInformation("📡 En écoute sur la queue RabbitMQ : {Queue}", queueName);

            while (!stoppingToken.IsCancellationRequested && _connection?.IsOpen == true)
            {
                await Task.Delay(1000, stoppingToken); // stop apres avoir consommer
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "❌ Exception dans RabbitListenerService");
        }
    }

    private async Task<ConnectionFactory> EstablishConnection()
    {
        var vault = new HashicorpVaultService(configuration);
        var connectionString = await vault.GetRabbitConnectionStringFromVault();
        var uri = new Uri("amqp://" + connectionString);
        return new ConnectionFactory { Uri = uri, DispatchConsumersAsync = false };
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
