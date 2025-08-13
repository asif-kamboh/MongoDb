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

    public virtual async Task<TEntity?> GetAsync(string documentId)
    {
        return await GetAsync(null, documentId);
    }

    public virtual async Task<TEntity?> GetAsync(IClientSessionHandle? session, string documentId)
    {
        AssertObjectId(documentId);
        return await FindOneAsync(session, FiltersBuilder<TEntity>.GetIdFilter(documentId));
    }

    public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr)
    {
        return await FindOneAsync(null, expr);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr)
    {
        return await FindOneAsync(session, expr, orderBy: null);
    }

    public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr, string? orderBy)
    {
        return await FindOneAsync(null, expr, orderBy);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        string? orderBy)
    {
        return await FindOneAsync(session, (FilterDefinition<TEntity>) expr, orderBy);
    }

    public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort)
    {
        return await FindOneAsync(null, expr, sort);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        SortDefinition<TEntity> sort)
    {
        return await FindOneAsync(session, (FilterDefinition<TEntity>) expr, sort);
    }

    public async Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter)
    {
        return await FindOneAsync(session: null, filter);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter)
    {
        return await FindOneAsync(session, filter, orderBy: null);
    }

    public async Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter, string? orderBy)
    {
        return await FindOneAsync(null, filter, orderBy);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        string? orderBy)
    {
        var result = await FindAsyncImpl(session, filter, 0, 1, orderBy);
        return result.Documents.FirstOrDefault();
    }

    public async Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort)
    {
        return await FindOneAsync(null, filter, sort);
    }

    public async Task<TEntity?> FindOneAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        SortDefinition<TEntity> sort)
    {
        var result = await FindAsyncImpl(session, filter, 0, 1, sort);
        return result.Documents.FirstOrDefault();
    }

    public async Task<long> CountDocumentsAsync(Expression<Func<TEntity, bool>> expr)
    {
        return await CountDocumentsAsync(null, expr);
    }

    public async Task<long> CountDocumentsAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr)
    {
        return await CountDocumentsAsync(session, (FilterDefinition<TEntity>) expr);
    }

    public async Task<long> CountDocumentsAsync(FilterDefinition<TEntity> filter)
    {
        return await CountDocumentsAsync(null, filter);
    }

    public async Task<long> CountDocumentsAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter)
    {
        var result = await Collection.CountDocumentsAsync(session, filter);
        return result;
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset, int? limit)
    {
        return await FindAsync(expr, offset, limit, orderBy: null);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset, int? limit,
        string? orderBy)
    {
        return await FindAsync(null, expr, offset, limit, orderBy);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        int? offset, int? limit, string? orderBy)
    {
        return await FindAsync(session, (FilterDefinition<TEntity>) expr, offset, limit, orderBy);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset, int? limit,
        SortDefinition<TEntity> sort)
    {
        return await FindAsync(null, expr, offset, limit, sort);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        int? offset, int? limit,
        SortDefinition<TEntity> sort)
    {
        return await FindAsync(session, (FilterDefinition<TEntity>) expr, offset, limit, sort);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters, int? offset,
        int? limit)
    {
        return await FindAsync(filters, offset, limit, orderBy: null);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters,
        int? offset, int? limit, string? orderBy)
    {
        return await FindAsync(null, filters, offset, limit, orderBy);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        IList<FilterDefinition<TEntity>> filters,
        int? offset, int? limit,
        string? orderBy)
    {
        return await FindAsync(session,FiltersBuilder<TEntity>.And(filters), offset, limit, orderBy);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters,
        int? offset, int? limit, SortDefinition<TEntity> sort)
    {
        return await FindAsync(null, filters, offset, limit, sort);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        IList<FilterDefinition<TEntity>> filters, int? offset, int? limit,
        SortDefinition<TEntity> sort)
    {
        return await FindAsync(session, FiltersBuilder<TEntity>.And(filters), offset, limit, sort);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, int? offset, int? limit)
    {
        return await FindAsync(null, filter, offset, limit);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter, int? offset, int? limit)
    {
        return await FindAsync(session, filter, offset, limit, orderBy: null);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, int? offset, int? limit, string? orderBy)
    {
        return await FindAsync(null, filter, offset, limit, orderBy);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter, int? offset, int? limit, string? orderBy)
    {
        var pageSize = limit ?? DefaultLimit;
        if (pageSize == -1) pageSize = int.MaxValue; // Removes the limit if -1 is passed

        var pageOffset = offset ?? 0;

        return await FindAsyncImpl(session, filter, pageOffset, pageSize, orderBy);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter,
        int? offset, int? limit,
        SortDefinition<TEntity> sort)
    {
        return await FindAsync(null, filter, offset, limit, sort);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter,
        int? offset, int? limit,
        SortDefinition<TEntity> sort)
    {
        var pageSize = limit ?? DefaultLimit;
        if(pageSize == -1) pageSize = int.MaxValue; // Removes the limit if -1 is passed
        var pageOffset = offset ?? 0;
        return await FindAsyncImpl(session, filter, pageOffset, pageSize, sort);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters,
        FindOptions<TEntity> options)
    {
        return await FindAsync(null, filters, options);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        IList<FilterDefinition<TEntity>> filters,
        FindOptions<TEntity> options)
    {
        return await FindAsync(session, FiltersBuilder<TEntity>.And(filters), options);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr,
        FindOptions<TEntity> options)
    {
        return await FindAsync((FilterDefinition<TEntity>) expr, options);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        FindOptions<TEntity> options)
    {
        return await FindAsync(session, (FilterDefinition<TEntity>) expr, options);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, FindOptions<TEntity> options)
    {
        return await FindAsync(null, filter, options);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter,
        FindOptions<TEntity> options)
    {
        return await FindAsyncImpl(session, filter, options);
    }

    public virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(string documentId, TUpdateModel payload,
        bool returnUpdated) where TUpdateModel : IUpdateModel
    {
        return await FindOneAndUpdateAsync(null, documentId, payload, returnUpdated);
    }

    public virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        string documentId, TUpdateModel payload,
        bool returnUpdated) where TUpdateModel : IUpdateModel
    {
        var opts = new FindOneAndUpdateOptions<TEntity>
        {
            ReturnDocument = returnUpdated ? ReturnDocument.After : ReturnDocument.Before,
            IsUpsert = false
        };

        var filter = FiltersBuilder<TEntity>.GetIdFilter(documentId);
        return await FindOneAndUpdateAsync(session, filter, payload, opts);
    }

    public virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(Expression<Func<TEntity, bool>> expr,
        TUpdateModel payload, bool isUpsert, bool returnUpdated) where TUpdateModel : IUpdateModel
    {
        return await FindOneAndUpdateAsync(null, expr, payload, isUpsert, returnUpdated);
    }

    public virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        TUpdateModel payload,
        bool isUpsert, bool returnUpdated) where TUpdateModel : IUpdateModel
    {
        var opts = new FindOneAndUpdateOptions<TEntity>
        {
            ReturnDocument = returnUpdated ? ReturnDocument.After : ReturnDocument.Before,
            IsUpsert = isUpsert
        };

        return await FindOneAndUpdateAsync(session, (FilterDefinition<TEntity>)expr, payload, opts);
    }

    public virtual async Task CreateAsync(TEntity document)
    {
        await CreateAsync(session: null, document);
    }

    public virtual async Task CreateAsync(IClientSessionHandle? session, TEntity document)
    {
        // Validate Input model
        document.Validate();

        if (session != null)
        {
            await Collection.InsertOneAsync(session, document);
        }
        else
        {
            await Collection.InsertOneAsync(document);
        }
    }

    public virtual async Task CreateManyAsync(IList<TEntity> documents)
    {
        await CreateManyAsync(null, documents);
    }

    public virtual async Task CreateManyAsync(IClientSessionHandle? session, IList<TEntity> documents)
    {
        if (!documents.Any()) return;
        documents.ToList().ForEach(doc => doc.Validate());

        if (session != null)
        {
            await Collection.InsertManyAsync(session, documents);
        }
        else
        {
            await Collection.InsertManyAsync(documents);
        }
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(TUpdateModel payload)
        where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(session: null, payload);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        TUpdateModel payload) where TUpdateModel : IUpdateModel
    {
        AssertObjectId(payload.Id);
        return await UpdateAsync(session, payload.Id!, payload);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(string documentId, TUpdateModel payload)
        where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(null, documentId, payload);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session, string documentId,
        TUpdateModel payload) where TUpdateModel : IUpdateModel
    {
        AssertObjectId(documentId);
        return await UpdateAsync(session, FiltersBuilder<TEntity>.GetIdFilter(documentId), payload, false);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(Expression<Func<TEntity, bool>> expr, TUpdateModel payload,
        bool updateMany) where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(null, expr, payload, updateMany);
    }

    public virtual async Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        TUpdateModel payload,
        bool updateMany) where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(session, (FilterDefinition<TEntity>) expr, payload, updateMany);
    }

    public virtual async Task<BulkWriteResult<TEntity>> BulkUpdateAsync<TUpdateModel>(IList<TUpdateModel> updates)
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

        var result = await Collection.BulkWriteAsync(bulkOps);
        return result;
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(string documentId, bool permanent)
    {
        return await FindOneAndDeleteAsync(null, documentId, permanent);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session, string documentId, bool permanent)
    {
        AssertObjectId(documentId);
        return await FindOneAndDeleteAsync(session, FiltersBuilder<TEntity>.GetIdFilter(documentId), permanent);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(Expression<Func<TEntity, bool>> expr, bool permanent)
    {
        return await FindOneAndDeleteAsync(null, expr, permanent);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr, bool permanent)
    {
        return await FindOneAndDeleteAsync(session, (FilterDefinition<TEntity>) expr, permanent);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(FilterDefinition<TEntity> filter, bool permanent)
    {
        return await FindOneAndDeleteAsync(null, filter, permanent);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        bool permanent)
    {
        if (permanent || !SupportsSoftDelete)
        {
            return await Collection.FindOneAndDeleteAsync(filter);
        }

        var updateDef = Builders<TEntity>.Update.Set(SoftDeleteFieldName, DateTime.UtcNow);
        var opts = new FindOneAndUpdateOptions<TEntity>
        {
            IsUpsert = false,
            ReturnDocument = ReturnDocument.After
        };

        return session != null
            ? await Collection.FindOneAndUpdateAsync(session, filter, updateDef, opts)
            : await Collection.FindOneAndUpdateAsync(filter, updateDef, opts);
    }

    public async Task<DbResult> DeleteOneAsync(string documentId, bool permanent)
    {
        return await DeleteOneAsync(null, documentId, permanent);
    }

    public async Task<DbResult> DeleteOneAsync(IClientSessionHandle? session, string documentId, bool permanent)
    {
        AssertObjectId(documentId);
        return await DeleteAsync(session, FiltersBuilder<TEntity>.GetIdFilter(documentId), false, permanent);
    }

    public async Task<DbResult> DeleteAsync(Expression<Func<TEntity, bool>> expr, bool permanent)
    {
        return await DeleteAsync(null, expr, permanent);
    }

    public async Task<DbResult> DeleteAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        bool permanent)
    {
        return await DeleteAsync(session, (FilterDefinition<TEntity>) expr, true, permanent);
    }

    public async Task<DbResult> DeleteAsync(FilterDefinition<TEntity> filter, bool permanent)
    {
        return await DeleteAsync(filter, true, permanent);
    }

    public async Task<DbResult> DeleteAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter, bool permanent)
    {
        return await DeleteAsync(session, filter, true, permanent);
    }

    public async Task<long> GenerateAutoIncrementIdAsync(long startValue)
    {
        return await GetNextSequenceId(Collection.CollectionNamespace.CollectionName, startValue);
    }

    public async Task<long> GenerateAutoIncrementIdAsync(string sequenceName, long startValue)
    {
        return await GetNextSequenceId(sequenceName, startValue);
    }

    #region Protected Methods

    protected virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(FilterDefinition<TEntity> filter, TUpdateModel payload, FindOneAndUpdateOptions<TEntity> opts)
    {
        return await FindOneAndUpdateAsync(null, filter, payload, opts);
    }

    protected virtual async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter,
        TUpdateModel payload, FindOneAndUpdateOptions<TEntity> opts)
    {
        var update = GenerateUpdateDefinition(payload, opts.IsUpsert);

        return session != null
            ? await Collection.FindOneAndUpdateAsync(session, filter, update, opts)
            : await Collection.FindOneAndUpdateAsync(filter, update, opts);
    }

    protected async Task<DbResult> DeleteAsync(FilterDefinition<TEntity> filter, bool deleteMany, bool permanent)
    {
        return await DeleteAsync(null, filter, deleteMany, permanent);
    }

    protected async Task<DbResult> DeleteAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        bool deleteMany, bool permanent)
    {
        if (permanent || !SupportsSoftDelete)
        {
            DeleteResult result;
            if (session != null)
            {
                result = deleteMany
                    ? await Collection.DeleteManyAsync(session, filter)
                    : await Collection.DeleteOneAsync(session, filter);
            }
            else
            {
                result = deleteMany
                    ? await Collection.DeleteManyAsync(filter)
                    : await Collection.DeleteOneAsync(filter);
            }

            return result.IsAcknowledged ? new DbResult(result.DeletedCount) : new DbResult(0);
        }

        var update = Builders<TEntity>.Update.Set(SoftDeleteFieldName, DateTime.UtcNow);
        return await UpdateAsync(session, filter, update, deleteMany);
    }

    protected virtual async Task<DbResult> UpdateAsync<TUpdateModel>(FilterDefinition<TEntity> filter,
        TUpdateModel payload, bool updateMany) where TUpdateModel : IUpdateModel
    {
        return await UpdateAsync(null, filter, payload, updateMany);
    }

    protected virtual async Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter,
        TUpdateModel payload,
        bool updateMany) where TUpdateModel : IUpdateModel
    {
        var update = GenerateUpdateDefinition(payload);
        return await UpdateAsync(session, filter, update, updateMany);
    }

    protected virtual async Task<DbResult> UpdateAsync(FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity>? update, bool updateMany)
    {
        return await UpdateAsync(null, filter, update, updateMany);
    }

    protected virtual async Task<DbResult> UpdateAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity>? update, bool updateMany)
    {
        return updateMany
            ? await UpdateManyImplAsync(session, filter, update)
            : await UpdateOneImplAsync(session, filter, update);
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

        var properties = payload?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[] {};

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
        int offset, int limit, string? orderBy)
    {
        var sortDef = SortDefinitionHelper.GetSortDefinition<TEntity>(orderBy);
        return FindAsyncImpl(session, filter, offset, limit, sortDef);
    }

    private async Task<IListViewModel<TEntity>> FindAsyncImpl(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter, int offset, int limit, SortDefinition<TEntity>? sort)
    {
        var opts = new FindOptions<TEntity> { Skip  = offset, Limit = limit };
        // Apply sorting
        if (sort != null) opts.Sort = sort;

        return await FindAsyncImpl(session, filter, opts);
    }

    private async Task<IListViewModel<TEntity>> FindAsyncImpl(IClientSessionHandle? session,
        FilterDefinition<TEntity> filter, FindOptions<TEntity> opts)
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
            ? Collection.FindAsync(session, filter, opts)
            : Collection.FindAsync(filter, opts);

        var tasks = new List<Task<IAsyncCursor<TEntity>>> { pageTask };

        var offset = opts.Skip ?? 0;
        if (opts.Limit > 1)
        {
            var nextPageOpts = new FindOptions<TEntity> { Skip  = offset + opts.Limit.Value, Limit = 1 };
            if (opts.Sort != null) nextPageOpts.Sort = opts.Sort;

            var nextPageTask = session != null
                ? Collection.FindAsync(session, filter, nextPageOpts)
                : Collection.FindAsync(filter, nextPageOpts);

            tasks.Add(nextPageTask);
        }

        await Task.WhenAll(tasks);

        var result = new ListViewModel<TEntity>
        {
            Documents = await pageTask.Result.ToListAsync(),
            Offset = offset,
            Limit = opts.Limit ?? 0, // It should not be ZERO
            HasNextPage = tasks.Count > 1 && await tasks[1].Result.AnyAsync()
        };

        return result;
    }

    private async Task<DbResult> UpdateOneImplAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity>? update)
    {
        try
        {
            if (update is null) throw new ArgumentNullException(nameof(update));

            var result = session != null
                ? await Collection.UpdateOneAsync(session, filter, update)
                : await Collection.UpdateOneAsync(filter, update);

            return new DbResult(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UpdateOneAsync failed. Error={Message}", ex.Message);
            throw;
        }
    }

    private async Task<DbResult> UpdateManyImplAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity>? update)
    {
        try
        {
            if (update is null) throw new ArgumentNullException(nameof(update));

            var result = session != null
                ? await Collection.UpdateManyAsync(session, filter, update)
                : await Collection.UpdateManyAsync(filter, update);

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