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

    internal async Task<long> GetNextSequenceId(string sequenceName, long startValue)
    {
        if (startValue < 0) startValue = 0;

        var filter = Builders<AutoIncSequence>.Filter.Eq(s => s.Name, sequenceName);
        var update = Builders<AutoIncSequence>.Update.Inc(s => s.Value, 1);
        var opts = new FindOneAndUpdateOptions<AutoIncSequence>
        {
            ReturnDocument = ReturnDocument.After
        };

        var collection = _dbContext.GetCollection<AutoIncSequence>("auto_inc_sequences_internal");
        var session = _dbContext.StartSession();
        try
        {
            var transactionOpts = await _dbContext.IsReplicaSetAsync()
                ? new TransactionOptions(readPreference: ReadPreference.Primary)
                : null;
            session.StartTransaction(transactionOpts);
            var sequence = await collection.FindOneAndUpdateAsync(session, filter, update, opts);
            if (sequence is null)
            {
                // Create new sequence. It may cause sync issues though
                sequence = new AutoIncSequence
                {
                    Name = sequenceName,
                    Value = startValue + 1
                };
                await collection.InsertOneAsync(session, sequence);
            }

            await session.CommitTransactionAsync();

            return sequence.Value;
        }
        catch (Exception)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Derived classes must implement it to define indexes on a collection
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<CreateIndexModel<TEntity>> BuildIndexModels();
}