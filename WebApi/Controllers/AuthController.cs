using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos.auth;
using WebApi.Entities;
using WebApi.Exceptions;
using WebApi.Persistence.Repository;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IdGeneratorService idGenerator,
    UserRepository userRepository,
    PasswordHasherService passwordHasherService,
    JwtService jwtService): ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto body)
    {
        var user = await userRepository.FindByEmail(body.Email);
        if (user == null || !passwordHasherService.Compare(body.Password, user.PasswordHash)) throw new InvalidCredentialsException();
        
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);
        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            access_token = accessToken,
            refresh_token = refreshToken
        });
    }
    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto body)
    {
        var newUser = new User
        {
            Id = idGenerator.GenerateId(),
            Email = body.Email,
            Name = body.Name,
            PasswordHash = passwordHasherService.Hash(body.Password)
        };
        await userRepository.CreateUser(newUser);
        
        var accessToken = jwtService.GenerateAccessToken(newUser);
        var refreshToken = jwtService.GenerateRefreshToken(newUser);
        
        return Ok(new
        {
            newUser.Id,
            newUser.Name,
            newUser.Email,
            access_token = accessToken,
            refresh_token = refreshToken
        });
    }
}