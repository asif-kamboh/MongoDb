using MongoDB.Driver;
using ScientificBit.MongoDb.Abstractions.Db;
using ScientificBit.MongoDb.Entities;

namespace ScientificBit.MongoDb.Repositories;

public abstract class BaseRepositoryInternal<TEntity> where TEntity : BaseEntity
{
    private readonly IMongoDbContext _dbContext;
    private readonly IMongoCollection<TEntity> _collection;

    protected IMongoCollection<TEntity> Collection
    {
        get
        {
            EnsureIndexes();
            return _collection;
        }
    }

    protected BaseRepositoryInternal(IMongoDbContext dbContext)
    {
        _dbContext = dbContext;
        _collection = dbContext.GetCollection<TEntity>();
    }

    private void EnsureIndexes()
    {
        if (!_dbContext.IndexesCreated(_collection.CollectionNamespace.CollectionName))
        {
            _dbContext.CreateIndexes(_collection, BuildIndexModels());
        }
    }

    /// <summary>
    /// Derived classes must implement it to define indexes on a collection
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<CreateIndexModel<TEntity>> BuildIndexModels();
}