using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ScientificBit.MongoDb.Abstractions.Db;
using ScientificBit.MongoDb.Entities;

namespace ScientificBit.MongoDb.Repositories;

public abstract class BaseRepositoryInternal<TEntity> where TEntity : BaseEntity
{
    private readonly ILogger? _logger;
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

    protected BaseRepositoryInternal(ILogger? logger, IMongoDbContext dbContext)
    {
        _logger = logger;
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
    /// Generates the next sequence ID for the given sequence name.
    /// </summary>
    /// <param name="sequenceName"></param>
    /// <param name="startValue"></param>
    /// <param name="externalSession">If an external session is given, the owner is responsible for closing it</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal async Task<long> GetNextSequenceId(string sequenceName, long startValue,
        IClientSessionHandle? externalSession, CancellationToken cancellationToken = default)
    {
        if (startValue < 0) startValue = 0;

        var filter = Builders<AutoIncSequence>.Filter.Eq(s => s.Name, sequenceName);
        var update = Builders<AutoIncSequence>.Update.Inc(s => s.Value, 1);
        var opts = new FindOneAndUpdateOptions<AutoIncSequence>
        {
            ReturnDocument = ReturnDocument.After
        };

        var collection = _dbContext.GetCollection<AutoIncSequence>("auto_inc_sequences_internal");
        var session = externalSession ?? await StartTransactionAsync(cancellationToken);
        try
        {
            var sequence = await collection.FindOneAndUpdateAsync(session, filter, update, opts, cancellationToken);
            if (sequence is null)
            {
                // Create a new sequence. It may cause sync issues though
                sequence = new AutoIncSequence
                {
                    Name = sequenceName,
                    Value = startValue + 1
                };
                await collection.InsertOneAsync(session, sequence, cancellationToken: cancellationToken);
            }

            if (externalSession is null) await session.CommitTransactionAsync(cancellationToken);

            return sequence.Value;
        }
        catch (Exception ex)
        {
            if (externalSession is null) await session.AbortTransactionAsync(cancellationToken);

            _logger?.LogError(ex,
                "Failed to get next sequence ID. SequenceName={SequenceName}, StartValue={StartValue}, Error={Message}",
                sequenceName, startValue, ex.Message);
            throw;
        }
    }

    protected async Task<IClientSessionHandle> StartTransactionAsync(CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var session = _dbContext.StartSession();
#pragma warning restore CS0618 // Type or member is obsolete
        try
        {
            var transactionOpts = await _dbContext.IsReplicaSetAsync(cancellationToken)
                ? new TransactionOptions(readPreference: ReadPreference.Primary)
                : null;
            session.StartTransaction(transactionOpts);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start transaction. Error={Message}", ex.Message);
        }

        return session;
    }

    /// <summary>
    /// Derived classes must implement it to define indexes on a collection
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<CreateIndexModel<TEntity>> BuildIndexModels();
}