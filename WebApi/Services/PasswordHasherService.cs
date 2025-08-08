namespace WebApi.Services;

public class PasswordHasherService
{
    public string Hash(string rawPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(rawPassword);
    }

    public bool Compare(string rawPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(rawPassword, hashedPassword);
    }
}