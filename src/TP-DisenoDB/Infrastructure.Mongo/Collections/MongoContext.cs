using MongoDB.Driver;
using TP_DisenoDB.Domain.Entities;

namespace TP_DisenoDB.Infrastructure.Mongo.Collections;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Bank> Banks => _database.GetCollection<Bank>("Banks");
    public IMongoCollection<CardHolder> CardHolders => _database.GetCollection<CardHolder>("CardHolders");
    public IMongoCollection<Card> Cards => _database.GetCollection<Card>("Cards");
    public IMongoCollection<Purchase> Purchases => _database.GetCollection<Purchase>("Purchases");
    public IMongoCollection<Promotion> Promotions => _database.GetCollection<Promotion>("Promotions");
    public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>("Payments");
}
