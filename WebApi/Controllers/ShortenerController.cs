using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos.shortener;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShortenerController(
    IHttpContextAccessor ctx,
    UrlShortenerService urlShortenerService
    ) : ControllerBase
{

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ShortenerUrl([FromBody] ShortenerDto body)
    {
        var id = ctx.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(id, out var ownerId)) throw new UnauthorizedAccessException();
        
        var result = await urlShortenerService.ShortenUrl(ownerId, body.OriginalUrl);
        return Ok(result);
    }
}