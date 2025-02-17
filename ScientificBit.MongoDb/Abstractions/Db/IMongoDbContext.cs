using MongoDB.Driver;

namespace ScientificBit.MongoDb.Abstractions.Db;

/// <summary>
/// Current MongoDb context
/// </summary>
public interface IMongoDbContext
{
    /// <summary>
    /// Database name
    /// </summary>
    string DatabaseName { get; } 

    /// <summary>
    /// Starts MongoDB session
    /// </summary>
    /// <returns></returns>
    IClientSessionHandle StartSession();

    /// <summary>
    /// Get collection based on Entity type
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    IMongoCollection<TEntity> GetCollection<TEntity>();

    /// <summary>
    /// Get collection based on given name
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    IMongoCollection<TEntity> GetCollection<TEntity>(string name);

    /// <summary>
    /// Checks if indexes are already created on the collection
    /// </summary>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    bool IndexesCreated(string collectionName);

    /// <summary>
    /// Creates indexes based on given index models
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="indexModels"></param>
    /// <typeparam name="TEntity"></typeparam>
    void CreateIndexes<TEntity>(IMongoCollection<TEntity> collection, IEnumerable<CreateIndexModel<TEntity>> indexModels);
}