using System.Collections.Concurrent;
using MongoDB.Bson;
using MongoDB.Driver;
using ScientificBit.MongoDb.Abstractions.Db;
using ScientificBit.MongoDb.Attributes;

namespace ScientificBit.MongoDb.Configuration;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _db;

    private readonly IMongoClient _client;

    private readonly ConcurrentDictionary<string, bool> _indexesCreated = new();

    public string DatabaseName { get; }

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _client = client;
        _db = _client.GetDatabase(databaseName);
        DatabaseName = databaseName;
    }

    public async Task<bool> IsReplicaSetAsync()
    {
        var isMasterCommand = new BsonDocument { { "isMaster", 1 } };
        var result = await _client.GetDatabase("admin").RunCommandAsync<BsonDocument>(isMasterCommand);
        return result.Contains("setName");
    }

    public IClientSessionHandle StartSession()
    {
        return _client.StartSession();
    }

    public bool IndexesCreated(string collectionName)
    {
        return _indexesCreated.ContainsKey(collectionName);
    }

    public void CreateIndexes<TEntity>(IMongoCollection<TEntity> collection, IEnumerable<CreateIndexModel<TEntity>> indexModels)
    {
        var models = indexModels.ToList();
        if (models.Count > 0)
        {
            collection.Indexes.CreateManyAsync(models).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        _indexesCreated.TryAdd(collection.CollectionNamespace.CollectionName, true);
    }

    public IMongoCollection<TEntity> GetCollection<TEntity>()
    {
        var collectionName = GetCollectionName(typeof(TEntity));
        return GetCollection<TEntity>(collectionName);
    }

    public IMongoCollection<TEntity> GetCollection<TEntity>(string name)
    {
        return _db.GetCollection<TEntity>(name);
    }

    private string GetCollectionName(Type entityType)
    {
        var attr = Attribute.GetCustomAttributes(entityType).FirstOrDefault(a => a is CollectionNameAttribute);
        if (attr is CollectionNameAttribute nameAttr)
        {
            if (!string.IsNullOrEmpty(nameAttr.Name)) return nameAttr.Name;
        }

        var name = entityType.Name.ToLower();
        if (name.EndsWith("y"))
        {
            name = name.Substring(0, name.Length - 1) + "ies";
        }
        else if (name.EndsWith("o"))
        {
            name += "es";
        }
        else
        {
            name += "s";
        }
        return name;
    }
}