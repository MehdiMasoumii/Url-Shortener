using System.Text;
using IdGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebApi.Dtos.Options;
using WebApi.Persistence;
using WebApi.Persistence.Repository;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOptions<ConnectionStrings>()
    .Bind(builder.Configuration.GetSection("ConnectionStrings"))
    .ValidateOnStart();
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateOnStart();

builder.Services.AddSingleton<IdGenerator>(_ => new IdGenerator(0));

builder.Services.AddSingleton<AppDbContext>();

builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<UrlRepository>();
builder.Services.AddSingleton<UrlShortenerService>();
builder.Services.AddSingleton<IdGeneratorService>();
builder.Services.AddSingleton<PasswordHasherService>();




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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();