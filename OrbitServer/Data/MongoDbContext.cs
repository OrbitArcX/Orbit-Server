using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        // Use the connection string from appsettings.json
        var mongoClient = new MongoClient(configuration["MongoDbSettings:ConnectionString"]);
        
        // Get the database name from appsettings.json
        _database = mongoClient.GetDatabase(configuration["MongoDbSettings:DatabaseName"]);
    }

    // Generic method to get MongoDB collections
    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}
