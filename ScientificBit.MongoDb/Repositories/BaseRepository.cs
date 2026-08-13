using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using ScientificBit.MongoDb.Abstractions.Db;
using ScientificBit.MongoDb.Abstractions.Repositories;
using ScientificBit.MongoDb.Builders;
using ScientificBit.MongoDb.Entities;
using ScientificBit.MongoDb.Mongo;
using ScientificBit.MongoDb.Updates;
using ScientificBit.MongoDb.Views;
using ScientificBit.MongoDb.Extensions;
using ScientificBit.MongoDb.Utils;

namespace ScientificBit.MongoDb.Repositories;

/// <summary>
/// Defines Generic repository for MongoDB document manipulation operations
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class BaseRepository<TEntity> : BaseRepositoryInternal<TEntity>, IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly ILogger? _logger;

    private const int DefaultLimit = 50;
    private const string SoftDeleteFieldName = "DeletedAt";

    protected virtual bool SupportsSoftDelete => typeof(TEntity).GetProperties()
        .Any(p => p.Name.Equals(SoftDeleteFieldName, StringComparison.OrdinalIgnoreCase));

    protected BaseRepository(IMongoDbContext dbContext) : this(null, dbContext)
    {
    }

    /// <summary>
    /// Override this constructor if you want to pass ILogger
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbContext"></param>
    protected BaseRepository(ILogger? logger, IMongoDbContext dbContext) : base(logger, dbContext)
    {
        _logger = logger;
    }

    protected override IEnumerable<CreateIndexModel<TEntity>> BuildIndexModels()
    {
        var builder = Builders<TEntity>.IndexKeys;
        var indexModels = new List<CreateIndexModel<TEntity>>
        {
            new(builder.Descending(doc => doc.UpdatedAt), new CreateIndexOptions { Background = true }),
            new(builder.Descending(doc => doc.CreatedAt), new CreateIndexOptions { Background = true })
        };
        if (SupportsSoftDelete)
        {
            indexModels.Add(new CreateIndexModel<TEntity>(builder.Ascending(SoftDeleteFieldName),
                new CreateIndexOptions { Background = true }));
        }

        return indexModels;
    }

    public virtual async Task<TEntity?> GetAsync(string documentId, CancellationToken cancellationToken = default)
    {
        return await GetAsync(null, documentId, cancellationToken);
    }

    public virtual async Task<TEntity?> GetAsync(IClientSessionHandle? session, string documentId,
        CancellationToken cancellationToken = default)
    {
        AssertObjectId(documentId);
        return await FindOneAsync(session, FiltersBuilder<TEntity>.GetIdFilter(documentId),
            cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(null, expr, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(session, expr, orderBy: null, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr, string? orderBy,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(null, expr, orderBy, cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        string? orderBy, CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(session, (FilterDefinition<TEntity>) expr, orderBy, cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(null, expr, sort, cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(session, (FilterDefinition<TEntity>) expr, sort, cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(session: null, filter, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(session, filter, orderBy: null, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter, string? orderBy,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(null, filter, orderBy, cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        string? orderBy, CancellationToken cancellationToken = default)
    {
        var result = await FindAsyncImpl(session, filter, 0, 1, orderBy, cancellationToken);
        return result.Documents.FirstOrDefault();
    }

    public async Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(null, filter, sort, cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
    {
        var result = await FindAsyncImpl(session, filter, 0, 1, sort, cancellationToken);
        return result.Documents.FirstOrDefault();
    }

    public async Task<long> CountDocumentsAsync(Expression<Func<TEntity, bool>> expr,
        CancellationToken cancellationToken = default)
    {
        return await CountDocumentsAsync(null, expr, cancellationToken);
    }

    public async Task<long> CountDocumentsAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        CancellationToken cancellationToken = default)
    {
        return await CountDocumentsAsync(session, (FilterDefinition<TEntity>) expr, cancellationToken);
    }

    public async Task<long> CountDocumentsAsync(FilterDefinition<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        return await CountDocumentsAsync(null, filter, cancellationToken);
    }

    public async Task<long> CountDocumentsAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        var result = session != null
            ? await Collection.CountDocumentsAsync(session, filter, cancellationToken: cancellationToken)
            : await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return result;
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset,
        int? limit, CancellationToken cancellationToken = default)
    {
        return await FindAsync(expr, offset, limit, orderBy: null, cancellationToken: cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset, int? limit,
        string? orderBy, CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, expr, offset, limit, orderBy, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        int? offset, int? limit, string? orderBy, CancellationToken cancellationToken = default)
    {
        return await FindAsync(session, (FilterDefinition<TEntity>) expr, offset, limit, orderBy, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset, int? limit,
        SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, expr, offset, limit, sort, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        int? offset, int? limit,
        SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
    {
        return await FindAsync(session, (FilterDefinition<TEntity>) expr, offset, limit, sort, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters, int? offset,
        int? limit, CancellationToken cancellationToken = default)
    {
        return await FindAsync(filters, offset, limit, orderBy: null, cancellationToken: cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters,
        int? offset, int? limit, string? orderBy, CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, filters, offset, limit, orderBy, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        IList<FilterDefinition<TEntity>> filters,
        int? offset, int? limit,
        string? orderBy, CancellationToken cancellationToken = default)
    {
        return await FindAsync(session, FiltersBuilder<TEntity>.And(filters), offset, limit, orderBy, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters,
        int? offset, int? limit, SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, filters, offset, limit, sort, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        IList<FilterDefinition<TEntity>> filters, int? offset, int? limit,
        SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
    {
        return await FindAsync(session, FiltersBuilder<TEntity>.And(filters), offset, limit, sort, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, int? offset, int? limit,
        CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, filter, offset, limit, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter, int? offset, int? limit, CancellationToken cancellationToken = default)
    {
        return await FindAsync(session, filter, offset, limit, orderBy: null, cancellationToken: cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, int? offset, int? limit, string? orderBy,
        CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, filter, offset, limit, orderBy, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter, int? offset, int? limit, string? orderBy,
        CancellationToken cancellationToken = default)
    {
        var pageSize = limit ?? DefaultLimit;
        if (pageSize == -1) pageSize = int.MaxValue; // Removes the limit if -1 is passed

        var pageOffset = offset ?? 0;

        return await FindAsyncImpl(session, filter, pageOffset, pageSize, orderBy, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter,
        int? offset, int? limit,
        SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, filter, offset, limit, sort, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter,
        int? offset, int? limit,
        SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
    {
        var pageSize = limit ?? DefaultLimit;
        if(pageSize == -1) pageSize = int.MaxValue; // Removes the limit if -1 is passed
        var pageOffset = offset ?? 0;
        return await FindAsyncImpl(session, filter, pageOffset, pageSize, sort, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters,
        FindOptions<TEntity> options, CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, filters, options, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        IList<FilterDefinition<TEntity>> filters,
        FindOptions<TEntity> options, CancellationToken cancellationToken = default)
    {
        return await FindAsync(session, FiltersBuilder<TEntity>.And(filters), options, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr,
        FindOptions<TEntity> options, CancellationToken cancellationToken = default)
    {
        return await FindAsync((FilterDefinition<TEntity>) expr, options, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        FindOptions<TEntity> options, CancellationToken cancellationToken = default)
    {
        return await FindAsync(session, (FilterDefinition<TEntity>) expr, options, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, FindOptions<TEntity> options,
        CancellationToken cancellationToken = default)
    {
        return await FindAsync(null, filter, options, cancellationToken);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter,
        FindOptions<TEntity> options, CancellationToken cancellationToken = default)
    {
        return await FindAsyncImpl(session, filter, options, cancellationToken);
    }

    public virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(string documentId, TUpdateModel payload,
        bool returnUpdated, CancellationToken cancellationToken = default) where TUpdateModel : IUpdateModel
    {
        return await FindOneAndUpdateAsync(null, documentId, payload, returnUpdated, cancellationToken);
    }

    public virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        string documentId, TUpdateModel payload,
        bool returnUpdated, CancellationToken cancellationToken = default) where TUpdateModel : IUpdateModel
    {
        var opts = new FindOneAndUpdateOptions<TEntity>
        {
            ReturnDocument = returnUpdated ? ReturnDocument.After : ReturnDocument.Before,
            IsUpsert = false
        };

        var filter = FiltersBuilder<TEntity>.GetIdFilter(documentId);
        return await FindOneAndUpdateAsync(session, filter, payload, opts, cancellationToken);
    }

    public virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(Expression<Func<TEntity, bool>> expr,
        TUpdateModel payload, bool isUpsert, bool returnUpdated, CancellationToken cancellationToken = default)
        where TUpdateModel : IUpdateModel
    {
        return await FindOneAndUpdateAsync(null, expr, payload, isUpsert, returnUpdated, cancellationToken);
    }

    public virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        TUpdateModel payload,
        bool isUpsert, bool returnUpdated, CancellationToken cancellationToken = default) where TUpdateModel : IUpdateModel
    {
        var opts = new FindOneAndUpdateOptions<TEntity>
        {
            ReturnDocument = returnUpdated ? ReturnDocument.After : ReturnDocument.Before,
            IsUpsert = isUpsert
        };

        return await FindOneAndUpdateAsync(session, (FilterDefinition<TEntity>)expr, payload, opts, cancellationToken);
    }

    public virtual async Task CreateAsync(TEntity document, CancellationToken cancellationToken = default)
    {
        await CreateAsync(session: null, document, cancellationToken);
    }

    public virtual async Task CreateAsync(IClientSessionHandle? session, TEntity document,
        CancellationToken cancellationToken = default)
    {
        // Validate Input model
        document.Validate();

        if (session != null)
        {
            await Collection.InsertOneAsync(session, document, cancellationToken: cancellationToken);
        }
        else
        {
            await Collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
    }

    public virtual async Task CreateManyAsync(IList<TEntity> documents, CancellationToken cancellationToken = default)
    {
        await CreateManyAsync(null, documents, cancellationToken);
    }

    public virtual async Task CreateManyAsync(IClientSessionHandle? session, IList<TEntity> documents,
        CancellationToken cancellationToken = default)
    {
        if (!documents.Any()) return;
        documents.ToList().ForEach(doc => doc.Validate());

        if (session != null)
        {
            await Collection.InsertManyAsync(session, documents, cancellationToken: cancellationToken);
        }
        else
        {
            await Collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        }
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(TUpdateModel payload,
        CancellationToken cancellationToken = default)
        where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(session: null, payload, cancellationToken);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        TUpdateModel payload, CancellationToken cancellationToken = default) where TUpdateModel : IUpdateModel
    {
        AssertObjectId(payload.Id);
        return await UpdateAsync(session, payload.Id!, payload, cancellationToken);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(string documentId, TUpdateModel payload,
        CancellationToken cancellationToken = default)
        where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(null, documentId, payload, cancellationToken);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session, string documentId,
        TUpdateModel payload, CancellationToken cancellationToken = default) where TUpdateModel : IUpdateModel
    {
        AssertObjectId(documentId);
        return await UpdateAsync(session, FiltersBuilder<TEntity>.GetIdFilter(documentId), payload, false,
            cancellationToken);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(Expression<Func<TEntity, bool>> expr, TUpdateModel payload,
        bool updateMany, CancellationToken cancellationToken = default) where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(null, expr, payload, updateMany, cancellationToken);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        TUpdateModel payload,
        bool updateMany, CancellationToken cancellationToken = default) where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(session, (FilterDefinition<TEntity>) expr, payload, updateMany, cancellationToken);
    }

    public virtual async Task<BulkWriteResult<TEntity>> BulkUpdateAsync<TUpdateModel>(IList<TUpdateModel> updates,
        CancellationToken cancellationToken = default)
        where TUpdateModel : IUpdateModel
    {
        var bulkOps = new List<WriteModel<TEntity>>();

        if (!updates.Any())
        {
            return new BulkWriteResult<TEntity>.Acknowledged(0, 0L, 0L, 0L, 0L, bulkOps, new List<BulkWriteUpsert>());
        }

        foreach (var update in updates)
        {
            AssertObjectId(update.Id);

            var filter = Builders<TEntity>.Filter.Eq(p => p.Id, update.Id);
            var updateDef = GenerateUpdateDefinition(update);
            bulkOps.Add(new UpdateOneModel<TEntity>(filter, updateDef));
        }

        var result = await Collection.BulkWriteAsync(bulkOps, cancellationToken: cancellationToken);
        return result;
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(string documentId, bool permanent,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAndDeleteAsync(null, documentId, permanent, cancellationToken);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session, string documentId, bool permanent,
        CancellationToken cancellationToken = default)
    {
        AssertObjectId(documentId);
        return await FindOneAndDeleteAsync(session, FiltersBuilder<TEntity>.GetIdFilter(documentId), permanent,
            cancellationToken);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(Expression<Func<TEntity, bool>> expr, bool permanent,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAndDeleteAsync(null, expr, permanent, cancellationToken);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr, bool permanent, CancellationToken cancellationToken = default)
    {
        return await FindOneAndDeleteAsync(session, (FilterDefinition<TEntity>) expr, permanent, cancellationToken);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(FilterDefinition<TEntity> filter, bool permanent,
        CancellationToken cancellationToken = default)
    {
        return await FindOneAndDeleteAsync(null, filter, permanent, cancellationToken);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        bool permanent, CancellationToken cancellationToken = default)
    {
        if (permanent || !SupportsSoftDelete)
        {
            return await Collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
        }

        var updateDef = Builders<TEntity>.Update.Set(SoftDeleteFieldName, DateTime.UtcNow);
        var opts = new FindOneAndUpdateOptions<TEntity>
        {
            IsUpsert = false,
            ReturnDocument = ReturnDocument.After
        };

        return session != null
            ? await Collection.FindOneAndUpdateAsync(session, filter, updateDef, opts, cancellationToken)
            : await Collection.FindOneAndUpdateAsync(filter, updateDef, opts, cancellationToken);
    }

    public async Task<DbResult> DeleteOneAsync(string documentId, bool permanent,
        CancellationToken cancellationToken = default)
    {
        return await DeleteOneAsync(null, documentId, permanent, cancellationToken);
    }

    public async Task<DbResult> DeleteOneAsync(IClientSessionHandle? session, string documentId, bool permanent,
        CancellationToken cancellationToken = default)
    {
        AssertObjectId(documentId);
        return await DeleteAsync(session, FiltersBuilder<TEntity>.GetIdFilter(documentId), false, permanent,
            cancellationToken);
    }

    public async Task<DbResult> DeleteAsync(Expression<Func<TEntity, bool>> expr, bool permanent,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(null, expr, permanent, cancellationToken);
    }

    public async Task<DbResult> DeleteAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        bool permanent, CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(session, (FilterDefinition<TEntity>) expr, true, permanent, cancellationToken);
    }

    public async Task<DbResult> DeleteAsync(FilterDefinition<TEntity> filter, bool permanent,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(filter, true, permanent, cancellationToken);
    }

    public async Task<DbResult> DeleteAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter, bool permanent,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(session, filter, true, permanent, cancellationToken);
    }

    public async Task<long> GenerateAutoIncrementIdAsync(long startValue, CancellationToken cancellationToken = default)
    {
        return await GenerateAutoIncrementIdAsync(Collection.CollectionNamespace.CollectionName, startValue,
            cancellationToken);
    }

    public async Task<long> GenerateAutoIncrementIdAsync(IClientSessionHandle? session, long startValue,
        CancellationToken cancellationToken = default)
    {
        return await GenerateAutoIncrementIdAsync(session, Collection.CollectionNamespace.CollectionName, startValue,
            cancellationToken);
    }

    public async Task<long> GenerateAutoIncrementIdAsync(string sequenceName, long startValue,
        CancellationToken cancellationToken = default)
    {
        return await GenerateAutoIncrementIdAsync(null, sequenceName, startValue, cancellationToken);
    }

    public async Task<long> GenerateAutoIncrementIdAsync(IClientSessionHandle? session, string sequenceName,
        long startValue, CancellationToken cancellationToken = default)
    {
        return await GetNextSequenceId(sequenceName, startValue, session, cancellationToken);
    }

    #region Protected Methods

    protected virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(FilterDefinition<TEntity> filter,
        TUpdateModel payload, FindOneAndUpdateOptions<TEntity> opts, CancellationToken cancellationToken = default)
    {
        return await FindOneAndUpdateAsync(null, filter, payload, opts, cancellationToken);
    }

    protected virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter,
        TUpdateModel payload, FindOneAndUpdateOptions<TEntity> opts, CancellationToken cancellationToken = default)
    {
        var update = GenerateUpdateDefinition(payload, opts.IsUpsert);

        return session != null
            ? await Collection.FindOneAndUpdateAsync(session, filter, update, opts, cancellationToken)
            : await Collection.FindOneAndUpdateAsync(filter, update, opts, cancellationToken);
    }

    protected async Task<DbResult> DeleteAsync(FilterDefinition<TEntity> filter, bool deleteMany, bool permanent,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(null, filter, deleteMany, permanent, cancellationToken);
    }

    protected async Task<DbResult> DeleteAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        bool deleteMany, bool permanent, CancellationToken cancellationToken = default)
    {
        if (permanent || !SupportsSoftDelete)
        {
            DeleteResult result;
            if (session != null)
            {
                result = deleteMany
                    ? await Collection.DeleteManyAsync(session, filter, cancellationToken: cancellationToken)
                    : await Collection.DeleteOneAsync(session, filter, cancellationToken: cancellationToken);
            }
            else
            {
                result = deleteMany
                    ? await Collection.DeleteManyAsync(filter, cancellationToken: cancellationToken)
                    : await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken);
            }

            return result.IsAcknowledged ? new DbResult(result.DeletedCount) : new DbResult(0);
        }

        var update = Builders<TEntity>.Update.Set(SoftDeleteFieldName, DateTime.UtcNow);
        return await UpdateAsync(session, filter, update, deleteMany, cancellationToken);
    }

    protected virtual async Task<DbResult> UpdateAsync<TUpdateModel>(FilterDefinition<TEntity> filter,
        TUpdateModel payload, bool updateMany, CancellationToken cancellationToken = default)
        where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(null, filter, payload, updateMany, cancellationToken);
    }

    protected virtual async Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter,
        TUpdateModel payload,
        bool updateMany, CancellationToken cancellationToken = default) where TUpdateModel : IUpdateModel
    {
        var update = GenerateUpdateDefinition(payload);
        return await UpdateAsync(session, filter, update, updateMany, cancellationToken);
    }

    protected virtual async Task<DbResult> UpdateAsync(FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity>? update, bool updateMany, CancellationToken cancellationToken = default)
    {
        return await UpdateAsync(null, filter, update, updateMany, cancellationToken);
    }

    protected virtual async Task<DbResult> UpdateAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity>? update, bool updateMany, CancellationToken cancellationToken = default)
    {
        return updateMany
            ? await UpdateManyImplAsync(session, filter, update, cancellationToken)
            : await UpdateOneImplAsync(session, filter, update, cancellationToken);
    }

    /// <summary>
    /// Please override this method to customize TModel specific functionality
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="isUpsert"></param>
    /// <returns></returns>
    protected virtual UpdateDefinition<TEntity>? GenerateUpdateDefinition(object? payload, bool isUpsert = false)
    {
        if (payload is UpdateDefinition<TEntity> update) return update;

        var builder = Builders<TEntity>.Update;
        var updates = new List<UpdateDefinition<TEntity>>();
        var modelType = typeof(TEntity);

        var properties = payload?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? [];

        // Skip Updates on Id, UpdatedAt, CreatedAt, and DeletedAt fields
        var fieldsToSkip = GetReadonlyFields();
        var createdAtUpdated = false;

        foreach (var prop in properties)
        {
            // Skip updates specific fields
            if (fieldsToSkip.Any(field => prop.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase))) continue;

            // Ignore Invalid properties
            if (modelType.GetProperty(prop.Name) is null) continue;

            if (!ShouldUpdateField(prop.Name)) continue;

            var propertyValue = prop.GetValue(payload);

            var updateDef = GenerateFieldUpdateDefinition(prop.Name, prop.PropertyType, propertyValue);
            if (updateDef != null)
            {
                if (prop.Name == nameof(BaseEntity.CreatedAt))
                {
                    createdAtUpdated = true;
                }
                updates.Add(updateDef);
            }
        }

        if (updates.Count > 0)
        {
            updates.Add(builder.Set(doc => doc.UpdatedAt, DateTime.UtcNow));
            if (isUpsert && !createdAtUpdated)
            {
                updates.Add(builder.SetOnInsert(nameof(BaseEntity.CreatedAt), DateTime.UtcNow));
            }
            return builder.Combine(updates);
        }

        return null;
    }

    /// <summary>
    /// Defines fields which should not be updated by UpdateAsync methods
    /// </summary>
    /// <returns></returns>
    protected virtual string[] GetReadonlyFields()
    {
        var readonlyFields = new List<string>
        {
            nameof(BaseEntity.Id),
            nameof(BaseEntity.UpdatedAt),
            nameof(BaseEntity.CreatedAt),
        };

        if (SupportsSoftDelete)
        {
            readonlyFields.Add(SoftDeleteFieldName);
        }

        return readonlyFields.ToArray();
    }

    /// <summary>
    /// Please override this method to customize field's UpdateDefinition
    /// </summary>
    /// <param name="field"></param>
    /// <param name="fieldType"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    protected virtual UpdateDefinition<TEntity>? GenerateFieldUpdateDefinition(string field, Type fieldType, object? val)
    {
        // Values can't be set to null.
        return val != null ? Builders<TEntity>.Update.Set(field, val) : null;
    }

    /// <summary>
    /// If the method returns true, the field will be included in UpdateDefinition
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    protected virtual bool ShouldUpdateField(string field) => true;

    #endregion

    #region Validations

    protected void AssertObjectId(string? id)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("Invalid document ID");

        if (!ObjectId.TryParse(id, out _))
        {
            throw new ArgumentException($"Invalid document ID format. Id={id}");
        }
    }

    #endregion

    #region Private methods

    private Task<IListViewModel<TEntity>> FindAsyncImpl(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        int offset, int limit, string? orderBy, CancellationToken cancellationToken = default)
    {
        var sortDef = SortDefinitionHelper.GetSortDefinition<TEntity>(orderBy);
        return FindAsyncImpl(session, filter, offset, limit, sortDef, cancellationToken);
    }

    private async Task<IListViewModel<TEntity>> FindAsyncImpl(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter, int offset, int limit, SortDefinition<TEntity>? sort,
        CancellationToken cancellationToken = default)
    {
        var opts = new FindOptions<TEntity> { Skip  = offset, Limit = limit };
        // Apply sorting
        if (sort != null) opts.Sort = sort;

        return await FindAsyncImpl(session, filter, opts, cancellationToken);
    }

    private async Task<IListViewModel<TEntity>> FindAsyncImpl(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter, FindOptions<TEntity> opts, CancellationToken cancellationToken = default)
    {
        if (SupportsSoftDelete)
        {
            var fieldName = PropertyNamingStylesHelper.ToCurrentNamingStyle(SoftDeleteFieldName);
            filter = Builders<TEntity>.Filter.And(
                filter,
                Builders<TEntity>.Filter.Eq(fieldName, BsonNull.Value)
            );
        }

        if (_logger != null)
        {
            // Render filter for logging
            var rendered = filter.Render(Collection.DocumentSerializer, Collection.Settings.SerializerRegistry);
            _logger.LogInformation("FindAsyncImpl: {FilterDef}", rendered);
        }

        var pageTask = session != null
            ? Collection.FindAsync(session, filter, opts, cancellationToken)
            : Collection.FindAsync(filter, opts, cancellationToken);

        var tasks = new List<Task<IAsyncCursor<TEntity>>> { pageTask };

        var offset = opts.Skip ?? 0;
        if (opts.Limit > 1)
        {
            var nextPageOpts = new FindOptions<TEntity> { Skip  = offset + opts.Limit.Value, Limit = 1 };
            if (opts.Sort != null) nextPageOpts.Sort = opts.Sort;

            var nextPageTask = session != null
                ? Collection.FindAsync(session, filter, nextPageOpts, cancellationToken)
                : Collection.FindAsync(filter, nextPageOpts, cancellationToken);

            tasks.Add(nextPageTask);
        }

        await Task.WhenAll(tasks);

        var result = new ListViewModel<TEntity>
        {
            Documents = await pageTask.Result.ToListAsync(cancellationToken),
            Offset = offset,
            Limit = opts.Limit ?? 0, // It should not be ZERO
            HasNextPage = tasks.Count > 1 && await tasks[1].Result.AnyAsync(cancellationToken)
        };

        return result;
    }

    private async Task<DbResult> UpdateOneImplAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity>? update, CancellationToken cancellationToken = default)
    {
        try
        {
            if (update is null) throw new ArgumentNullException(nameof(update));

            var result = session != null
                ? await Collection.UpdateOneAsync(session, filter, update, cancellationToken: cancellationToken)
                : await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

            return new DbResult(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UpdateOneAsync failed. Error={Message}", ex.Message);
            throw;
        }
    }

    private async Task<DbResult> UpdateManyImplAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity>? update, CancellationToken cancellationToken = default)
    {
        try
        {
            if (update is null) throw new ArgumentNullException(nameof(update));

            var result = session != null
                ? await Collection.UpdateManyAsync(session, filter, update, cancellationToken: cancellationToken)
                : await Collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);

            return new DbResult(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UpdateManyAsync failed. Error={Message}", ex.Message);
            throw;
        }
    }

    #endregion
}
