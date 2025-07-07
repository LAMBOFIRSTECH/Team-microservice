using System.Text;
using System.Text.RegularExpressions;
using CustomVaultPackage.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Teams.APP.Layer.Interfaces;

namespace Teams.INFRA.Layer.ExternalServices;

public partial class RabbitListenerService(
    ILogger<RabbitListenerService> log,
    IModel? channel,
    IConfiguration configuration,
    IConnection? connection,
    IBackgroundJobService backgroundJob
) : BackgroundService
{
    /** Le message de rabbitMq doit etre du genre
     - Member to add    | 12345678-90ab-cdef-1234-567890abcdef | Equipe dev
     - Member to delete | 12345678-90ab-cdef-1234-567890abcdef | Equipe dev
     - Project Affected
    **/
    [GeneratedRegex(
        @"\|\s*([\da-f]{8}(-[\da-f]{4}){3}-[\da-f]{12})\s*\|\s*(.+)",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex GuidAndTeamRegex();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Run(() => StartListening("team_management"), cancellationToken);
    }

    public async Task<ConnectionFactory> EstablishConnection()
    {
        var vault = new HashicorpVaultService(configuration);
        var connectionString = await vault.GetRabbitConnectionStringFromVault();
        var rabbitUri = new Uri("amqp://" + connectionString);
        await Task.Delay(500);
        return new ConnectionFactory { Uri = rabbitUri };
    }

    private async void StartListening(string queueName)
    {
        try
        {
            var factory = await EstablishConnection();
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                log.LogInformation("🌐 Message reçu: {Message}", message);

                var match = GuidAndTeamRegex().Match(message);

                if (match.Success && Guid.TryParse(match.Groups[1].Value, out Guid memberId))
                {
                    var teamName = match.Groups[2].Value;
                    try
                    {
                        if (message.Contains("Member to add"))
                            backgroundJob.ScheduleAddTeamMemberAsync(memberId);
                        else if (message.Contains("Member to delete"))
                            backgroundJob.ScheduleDeleteTeamMemberAsync(memberId, teamName);
                        else if (message.Contains("Project Affected"))
                            backgroundJob.ScheduleProjectAssociationAsync();
                        else
                            throw new InvalidOperationException();
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        log.LogError("❌ Error during message processing: {Exception}", ex);
                        channel.BasicNack(
                            deliveryTag: ea.DeliveryTag,
                            multiple: false,
                            requeue: true
                        );
                    }
                }
            };
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            log.LogInformation("🔄 Listening to messages on queue: {QueueName}", queueName);
            while (!connection.IsOpen)
            {
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            log.LogError("❌ Eroor during messages listenning: {Exception}", ex);
        }
    }

    public override void Dispose()
    {
        channel?.Close();
        connection?.Close();
        base.Dispose();
    }
}
