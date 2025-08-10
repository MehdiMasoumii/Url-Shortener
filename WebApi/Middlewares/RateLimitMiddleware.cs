using System.Security.Claims;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebApi.Dtos.Options;

namespace WebApi.Middlewares;

public class RateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer connectionMultiplexer, IOptions<RateLimitOptions> rateLimitOptions)
{
    private readonly List<string> _limitedRoutes = ["/api/shortener"];
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (_limitedRoutes.Contains(context.Request.Path))
        {
            var redisContext = connectionMultiplexer.GetDatabase();
            var id = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(id, out var userId))
            {
                var bucketSize = rateLimitOptions.Value.BucketSize;
                var refillTime = rateLimitOptions.Value.RefillTime;
                var dateOffsetNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var ttl = bucketSize * refillTime;
                
                var key = $"rate_limit:{userId}";
                var script = @"
                        local data = redis.call('HGETALL', KEYS[1]);
                        
                        if #data == 0 then
                            redis.call('HSETEX', KEYS[1], 'EX', ARGV[1], 'FIELDS', 2, ARGV[2], ARGV[3], ARGV[4], ARGV[5]);
                            return 1;
                        else
                            local offset = tonumber(ARGV[5]) - tonumber(data[2]);
                            local newTokenCount = math.min(math.floor(offset / tonumber(ARGV[6])), tonumber(ARGV[3])) + tonumber(data[4]) - 1;

                            if newTokenCount >= 0 then
                                redis.call('HSETEX', KEYS[1], 'EX', ARGV[1], 'FIELDS', 2, ARGV[2], math.min(newTokenCount, tonumber(ARGV[3])), ARGV[4], ARGV[5]);
                                return 1;
                            else
                                return 0;
                            end
                        end
                ";
                
                var result = await redisContext
                    .ScriptEvaluateAsync(script, [key], 
                        [ ttl,
                            // fields:
                            "tokens", bucketSize - 1 , "last_refill", dateOffsetNow,
                            refillTime
                        ]);
                
                if ((int)result != 1)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return;
                }
            }
        }
        await next(context);
    }

    
}