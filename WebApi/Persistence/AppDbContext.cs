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
    public IMongoCollection<Click> Clicks => Context.GetCollection<Click>("Clicks");

    public AppDbContext(IOptions<ConnectionStrings> options)
    {   
        var connection = new MongoClient(options.Value.MongoDb);
        var database = connection.GetDatabase("UrlShortener");
        Context = database;

        var userEmailIndex = Builders<User>.IndexKeys.Ascending(user => user.Email);
        var userEmailIndexModel = new CreateIndexModel<User>(
            userEmailIndex,
                new CreateIndexOptions { Unique = true, Name = "Email_Unique_Index" }
            );
        
        var shortUrlIndex = Builders<Url>.IndexKeys.Ascending(url => url.ShortUrl);
        var shortUrlIndexModel = new CreateIndexModel<Url>(
                shortUrlIndex,
                new CreateIndexOptions { Unique = true, Name = "ShortUrl_Unique_Index" }
            );
        
        Urls.Indexes.CreateOne(shortUrlIndexModel);
        Users.Indexes.CreateOne(userEmailIndexModel);
    }
}