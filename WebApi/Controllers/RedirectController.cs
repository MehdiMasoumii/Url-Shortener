using Microsoft.AspNetCore.Mvc;
using WebApi.Events;
using WebApi.Services;
using WebApi.Services.MsgBroker;

namespace WebApi.Controllers;

[ApiController]
public class RedirectController(
    IHttpContextAccessor ctx,
    RedirectService redirectService,
    PublisherService publisherService
    ): ControllerBase
{
    [HttpGet]
    [Route("{shortUrl}")]
    public async Task<IActionResult> OpenShortLink([FromRoute] string shortUrl)
    {
        var url = await redirectService.GetOriginalUrl(shortUrl);
        if (url == null) return NotFound();
        
        var ip = ctx.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = ctx.HttpContext?.Request.Headers.UserAgent.ToString();
        
        if (ip != null && !string.IsNullOrEmpty(userAgent))
        {
            var clickMessage = new ClickEvent
            {
                Ip = ip,
                ShortUrl = url.ShortUrl,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow,
            };
            await publisherService.PublishAsync("Click_Event", clickMessage);
            return RedirectPermanent(url.OriginalUrl);
        }

        return BadRequest();
    }
    
}