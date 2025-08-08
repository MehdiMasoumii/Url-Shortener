using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using WebApi.Dtos.Options;

namespace WebApi.Services.MsgBroker;

public class RabbitMqService(IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    private readonly ConnectionFactory _factory = new()
    {
        HostName = options.Value.HostName,
        Port = options.Value.Port,
        UserName = options.Value.UserName,
        Password = options.Value.Password,
    };
    private IConnection? _connection;

    public async Task InitializeAsync()
    {
        _connection = await _factory.CreateConnectionAsync();
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        if (_connection is null)
            throw new InvalidOperationException("RabbitMQ connection not initialized.");
        
        return await _connection.CreateChannelAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}