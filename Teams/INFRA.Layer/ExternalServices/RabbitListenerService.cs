using System.Text;
using System.Text.RegularExpressions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;

namespace Teams.INFRA.Layer.ExternalServices;

/// <summary>
/// RabbitMQ listener service
/// 1. Connects to RabbitMQ and listens for messages on a specified queue.
/// 2. Processes messages to handle team member additions/removals and project associations.
/// 3. Uses regex to parse messages and extract relevant information.
/// 4. Interacts with a background job service to schedule tasks based on message content.
/// 5. Implements error handling and logging for monitoring and debugging.
/// 6. Designed to run as a hosted background service within the application.
///   Note: Ensure RabbitMQ server is accessible and the queue is properly configured.
///  Connection details are sourced from configuration settings.
/// </summary>
/// <param name="log"></param>
/// <param name="configuration"></param>
/// <param name="scopeFactory"></param>
public partial class RabbitListenerService(
    ILogger<RabbitListenerService> log,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    // Que se passe t-il quand un message n'est pas consommé correctement ?
    // -> Il est renvoyé dans la file d'attente (requeue)
    // -> On peut aussi le rejeter (reject) et le supprimer (nack) de la file d'attente
    // -> On peut aussi le rediriger vers une autre file d'attente (dead-letter queue)
    // Ici, on choisit de le supprimer (nack) pour éviter les boucles infinies de reprocessing
    // En production, il serait judicieux de mettre en place une dead-letter queue pour analyser
    // les messages qui n'ont pas pu être traités
    // et comprendre pourquoi (format incorrect, données manquantes, etc.)
    // On pourrait aussi implémenter un mécanisme de retry avec un délai avant de renvoyer le message
    // dans la file d'attente principale.
    // Pour l'instant, on a un nack et un retry coté hangfire (3 tentatives par défaut)
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
        LogHelper.Info("🚀 RabbitMq Listener Service starting up", log);

        try
        {
            await InitializeRabbitMqConnectionAsync();
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var exchange = ea.Exchange;
                var routingKey = ea.RoutingKey;
                var queue = QueueName;

                // Vérification de l'exchange et de la queue
                if (exchange != "team_exchange")
                {
                    LogHelper.Warning($"⚠️ Message reçu d’un exchange ({exchange}) ou routing key ({routingKey}) inattendu. Nack et skip.", log);
                    _channel?.BasicNack(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: false
                    );
                    return;
                }
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                if (string.IsNullOrWhiteSpace(message))
                {
                    LogHelper.Warning("📭 Received empty message, skipping processing.", log);
                    _channel?.BasicNack(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: false
                    );
                    return;
                }
                LogHelper.Info($"📩 Message received from queue '{queue}': and routing key {ea.RoutingKey}", log);
                try
                {
                    ProcessMessage(message, ea);
                    // Ack après traitement réussi
                    _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"❌ Erreur lors du traitement du message: {ex.Message}", log);
                    // Nack + requeue = false pour éviter boucle infinie sur message corrompu
                    _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            LogHelper.Info($"📡 RabbitMQ consumer started successfully for queue: {QueueName}", log);
            while (!stoppingToken.IsCancellationRequested && _connection?.IsOpen == true)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            LogHelper.Error($"❌ RabbitMQ listener failed: {ex.Message}", log);
            throw;
        }
    }

    private void ProcessMessage(string message, BasicDeliverEventArgs ea)
    {
        using var scope = scopeFactory.CreateScope();
        var teamProjectLife = scope.ServiceProvider.GetRequiredService<ITeamProjectLifeCycle>();
        var match = GuidAndTeamRegex().Match(message);

        try
        {
            switch (ea.RoutingKey)
            {
                case "Project.affected":
                    Console.WriteLine("depuis le listener");
                    teamProjectLife.AddProjectToTeamAsync(message);
                    break;

                case "Project.suspended":
                    teamProjectLife.SuspendProjectAsync(message);
                    break;

                // case "Member.add":
                //         backgroundJob.ScheduleAddTeamMemberAsync(memberToAddId);
                //     break;

                // case "Member.delete":
                //         backgroundJob.ScheduleDeleteTeamMemberAsync(memberToDeleteId, match.Groups[3].Value.Trim());
                //     break;

                default:
                    LogHelper.Warning($"⚠️ Routing key {ea.RoutingKey} non gérée.", log);
                    _channel?.BasicReject(ea.DeliveryTag, requeue: false);
                    return;
            }
            _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            LogHelper.Info($"🔔 Message processed successfully for routing key '{ea.RoutingKey}'", log);
        }
        catch (Exception ex)
        {
            HandleMessageError(ex, ea, match.Success ? match.Groups[1].Value : "N/A");
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
        LogHelper.Error($"❌ Error processing message. GUID: {guidValue}. Exception: {ex.Message}", log);
        LogHelper.CriticalFailure(log, "RabbitListenerService", "Error processing RabbitMQ message", ex);
        _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
    }

    private void HandleServiceError(Exception ex)
    {
        LogHelper.Error($"❌ Error in RabbitListenerService: {ex.Message}", log);
        _channel?.Close();
        _connection?.Close();
        LogHelper.CriticalFailure(log, "RabbitListenerService", "Exception in RabbitListenerService", ex);
    }
    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
