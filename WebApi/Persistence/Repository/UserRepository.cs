using MongoDB.Driver;
using WebApi.Entities;
using WebApi.Exceptions;

namespace WebApi.Persistence.Repository;

public class UserRepository(AppDbContext dbContext)
{
    public async Task CreateUser(User user)
    {
        var isAny = await dbContext.Users.Find(u => u.Email == user.Email).AnyAsync();
        if (isAny) throw new EmailAlreadyExistsException(user.Email);
        
        await dbContext.Users.InsertOneAsync(user);
    }

    public async Task<User?> FindByEmail(string email)
    {
        return await dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }
}