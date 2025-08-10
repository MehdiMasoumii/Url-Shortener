using System.Text;
using IdGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using WebApi.Dtos.Options;
using WebApi.Middlewares;
using WebApi.Persistence;
using WebApi.Persistence.Repository;
using WebApi.Services;
using WebApi.Services.BackgroundServices;
using WebApi.Services.MsgBroker;
using WebApi.Services.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<ClickEventBgService>();

#region RedisInjection

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Redis connection string not initialized.");
    return ConnectionMultiplexer.Connect(connectionString);
});
builder.Services.AddScoped<IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

#endregion

#region ConfigurationsInjection

builder.Services.AddOptions<ConnectionStrings>()
    .Bind(builder.Configuration.GetSection("ConnectionStrings"))
    .ValidateOnStart();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateOnStart();

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection("RabbitMq"))
    .ValidateOnStart();

builder.Services.AddOptions<RateLimitOptions>()
    .Bind(builder.Configuration.GetSection("RateLimit"))
    .ValidateOnStart();

#endregion

#region ServiceBusInjection

builder.Services.AddSingleton<IdGenerator>(_ => new IdGenerator(0));

builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IdGeneratorService>();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UrlCacheService>();
builder.Services.AddScoped<UrlShortenerService>();
builder.Services.AddScoped<RedirectService>();
builder.Services.AddScoped<PublisherService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UrlRepository>();
builder.Services.AddScoped<ClickRepository>();
builder.Services.AddScoped<PasswordHasherService>();

#endregion

#region AuthService

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
    if (jwtOptions == null) throw new Exception("JWT options not found");
    
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
        ClockSkew = TimeSpan.Zero
    };
    
});
builder.Services.AddAuthorization();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RateLimitMiddleware>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<RabbitMqService>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<UrlCacheService>().CacheTopClickUrlsAsync(50);
}

app.Run();