using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace WebApi.Services.MsgBroker;

public class PublisherService(RabbitMqService rabbitMqOptions)
{
    public async Task PublishAsync<T>(string queueName, T message)
    {
        await using var channel = await rabbitMqOptions.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            null
            );
        
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));


        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            body: body
        );

    }
}