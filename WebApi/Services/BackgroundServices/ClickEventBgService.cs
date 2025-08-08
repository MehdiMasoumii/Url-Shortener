using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApi.Events;
using WebApi.Persistence.Repository;
using WebApi.Services.MsgBroker;

namespace WebApi.Services.BackgroundServices;

public class ClickEventBgService(
    RabbitMqService rabbitMqService,
    IServiceProvider serviceProvider
    ): BackgroundService
{
    private IChannel channel;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        channel = await rabbitMqService.CreateChannelAsync();
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var clickEvent = JsonSerializer.Deserialize<ClickEvent>(message);
                if (clickEvent == null) throw new NullReferenceException();

                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider.GetRequiredService<ClickRepository>();
                
                await ProcessClickEvent(clickEvent, scopedServices);
    
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }

        };
        
        await channel.BasicConsumeAsync("Click_Event", false, consumer, cancellationToken: stoppingToken);
    }
    
    private async Task ProcessClickEvent(ClickEvent clickEvent, ClickRepository clickRepository)
    {
        await clickRepository.SaveClickAsync(clickEvent);
    }
}