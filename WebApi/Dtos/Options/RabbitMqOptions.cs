namespace WebApi.Dtos.Options;

public record RabbitMqOptions
{
    public required string HostName { get; set; }
    public int Port { get; set; } = 5672;
    public required string UserName { get; set; }
    public required string Password { get; set; }
};