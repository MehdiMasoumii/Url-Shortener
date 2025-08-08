namespace WebApi.Events;

public record ClickEvent
{ 
    public string ShortUrl { get; set; } 
    public string Ip { get; set; } 
    public string UserAgent { get; set; } 
    public DateTime Timestamp { get; set; }
};