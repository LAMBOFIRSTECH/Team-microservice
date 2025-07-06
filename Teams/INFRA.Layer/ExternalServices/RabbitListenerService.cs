using System.Text;
using CustomVaultPackage.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Teams.INFRA.Layer.ExternalServices;

public class RabbitListenerService(
    ILogger<RabbitListenerService> log,
    IModel? channel,
    IConfiguration configuration,
    IConnection? connection
) : BackgroundService
{
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
                var message = Encoding.UTF8.GetString(body);
                log.LogInformation("🌐 Message reçu: {Message}", message);
                try
                {
                    if (
                        message.Contains("Member to add")
                        || message.Contains("Project Affected")
                        || message.Contains("Member to delete")
                    )
                        hangFire.RetrieveDataFromOpenLdap(true, message); //
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    log.LogError("❌ Error during message processing: {Exception}", ex);
                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
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
