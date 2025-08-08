using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Dtos.Options;
using WebApi.Entities;

namespace WebApi.Persistence;

public class AppDbContext
{
    private IMongoDatabase Context { get; }
    public IMongoCollection<User> Users => Context.GetCollection<User>("Users");
    public IMongoCollection<Url> Urls => Context.GetCollection<Url>("Urls");

    public AppDbContext(IOptions<ConnectionStrings> options)
    {   
        var connection = new MongoClient(options.Value.MongoDb);
        var database = connection.GetDatabase("UrlShortener");
        Context = database;
    }
}