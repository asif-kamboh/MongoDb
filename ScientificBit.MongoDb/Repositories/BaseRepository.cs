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

    protected BaseRepository(IMongoDbContext dbContext) : this(null, dbContext)
    {
    }

    /// <summary>
    /// Override this constructor, if you want to pass ILogger
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbContext"></param>
    protected BaseRepository(ILogger? logger, IMongoDbContext dbContext) : base(dbContext)
    {
        _logger = logger;
    }

    protected override IEnumerable<CreateIndexModel<TEntity>> BuildIndexModels()
    {
        var builder = Builders<TEntity>.IndexKeys;
        var indexModels = new List<CreateIndexModel<TEntity>>
        {
            new (builder.Descending(doc => doc.UpdatedAt), new CreateIndexOptions { Background = true }),
            new (builder.Descending(doc => doc.CreatedAt), new CreateIndexOptions { Background = true }),
            new (builder.Ascending(doc => doc.DeletedAt), new CreateIndexOptions { Background = true })
        };
        return indexModels;
    }

    public virtual Task<TEntity?> GetAsync(string documentId)
    {
        AssertObjectId(documentId);
        return FindOneAsync(FiltersBuilder<TEntity>.GetIdFilter(documentId));
    }

    public Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr, string? orderBy = null)
    {
        return FindOneAsync((FilterDefinition<TEntity>) expr, orderBy);
    }

    public Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort)
    {
        return FindOneAsync((FilterDefinition<TEntity>) expr, sort);
    }

    public async Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter, string? orderBy = null)
    {
        var result = await FindAsyncImpl(filter, 0, 1, orderBy);
        return result.Documents.FirstOrDefault();
    }

    public async Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort)
    {
        var result = await FindAsyncImpl(filter, 0, 1, sort);
        return result.Documents.FirstOrDefault();
    }

    public Task<long> CountDocumentsAsync(Expression<Func<TEntity, bool>> expr)
    {
        return CountDocumentsAsync((FilterDefinition<TEntity>) expr);
    }

    public async Task<long> CountDocumentsAsync(FilterDefinition<TEntity> filter)
    {
        var result = await Collection.CountDocumentsAsync(filter);
        return result;
    }

    public Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset,  int? limit, string? orderBy = null)
    {
        return FindAsync((FilterDefinition<TEntity>) expr, offset, limit, orderBy);
    }

    public Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset,  int? limit, SortDefinition<TEntity> sort)
    {
        return FindAsync((FilterDefinition<TEntity>) expr, offset, limit, sort);
    }

    public Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters, int? offset, int? limit, string? orderBy = null)
    {
        // Exclude deleted docs
        filters.Add(FiltersBuilder<TEntity>.GetDeleteAtFilter());
        return FindAsync(FiltersBuilder<TEntity>.And(filters), offset, limit, orderBy);
    }

    public Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters, int? offset, int? limit, SortDefinition<TEntity> sort)
    {
        // Exclude deleted docs
        filters.Add(FiltersBuilder<TEntity>.GetDeleteAtFilter());
        return FindAsync(FiltersBuilder<TEntity>.And(filters), offset, limit, sort);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, int? offset, int? limit, string? orderBy = null)
    {
        var pageSize = limit ?? DefaultLimit;
        if(pageSize == -1) pageSize = int.MaxValue; // Removes the limit if -1 is passed
        var pageOffset = offset ?? 0;
        return await FindAsyncImpl(filter, pageOffset, pageSize, orderBy);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, int? offset, int? limit, SortDefinition<TEntity> sort)
    {
        var pageSize = limit ?? DefaultLimit;
        if(pageSize == -1) pageSize = int.MaxValue; // Removes the limit if -1 is passed
        var pageOffset = offset ?? 0;
        return await FindAsyncImpl(filter, pageOffset, pageSize, sort);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters,
        FindOptions<TEntity> options)
    {
        // Exclude deleted docs
        filters.Add(FiltersBuilder<TEntity>.GetDeleteAtFilter());
        return await FindAsync(FiltersBuilder<TEntity>.And(filters), options);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr,
        FindOptions<TEntity> options)
    {
        return await FindAsync((FilterDefinition<TEntity>) expr, options);
    }

    public async Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, FindOptions<TEntity> options)
    {
        return await FindAsyncImpl(filter, options);
    }

    public Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(string documentId, TUpdateModel payload,
        bool returnUpdated = true) where TUpdateModel : IUpdateModel
    {
        var opts = new FindOneAndUpdateOptions<TEntity>
        {
            ReturnDocument = returnUpdated ? ReturnDocument.After : ReturnDocument.Before,
            IsUpsert = false
        };

        var filter = FiltersBuilder<TEntity>.GetIdFilter(documentId);
        return FindOneAndUpdateAsync(filter, payload, opts);
    }

    public Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(Expression<Func<TEntity, bool>> expr,
        TUpdateModel payload, bool isUpsert, bool returnUpdated = false) where TUpdateModel : IUpdateModel
    {
        var opts = new FindOneAndUpdateOptions<TEntity>
        {
            ReturnDocument = returnUpdated ? ReturnDocument.After : ReturnDocument.Before,
            IsUpsert = isUpsert
        };

        return FindOneAndUpdateAsync((FilterDefinition<TEntity>)expr, payload, opts);
    }

    public virtual Task CreateAsync(TEntity document)
    {
        // Validate Input model
        document.Validate();

        return Collection.InsertOneAsync(document);
    }

    public virtual async Task CreateManyAsync(IList<TEntity> documents)
    {
        if (!documents.Any()) return;
        documents.ToList().ForEach(doc => doc.Validate());

        await Collection.InsertManyAsync(documents);
    }

    public virtual Task<DbResult> UpdateAsync<TUpdateModel>(TUpdateModel payload) where TUpdateModel : IUpdateModel
    {
        AssertObjectId(payload.Id);
        return UpdateAsync(payload.Id!, payload);
    }

    public virtual Task<DbResult> UpdateAsync<TUpdateModel>(string documentId, TUpdateModel payload)
        where TUpdateModel : IUpdateModel
    {
        AssertObjectId(documentId);
        return UpdateAsync(FiltersBuilder<TEntity>.GetIdFilter(documentId), payload, false);
    }

    public virtual Task<DbResult> UpdateAsync<TUpdateModel>(Expression<Func<TEntity, bool>> expr, TUpdateModel payload,
        bool updateMany) where TUpdateModel : IUpdateModel
    {
        return UpdateAsync((FilterDefinition<TEntity>) expr, payload, updateMany);
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

    public Task<TEntity?> FindOneAndDeleteAsync(string documentId, bool permanent = false)
    {
        AssertObjectId(documentId);
        return FindOneAndDeleteAsync(FiltersBuilder<TEntity>.GetIdFilter(documentId), permanent);
    }

    public Task<TEntity?> FindOneAndDeleteAsync(Expression<Func<TEntity, bool>> expr, bool permanent = false)
    {
        return FindOneAndDeleteAsync((FilterDefinition<TEntity>) expr);
    }

    public async Task<TEntity?> FindOneAndDeleteAsync(FilterDefinition<TEntity> filter, bool permanent = false)
    {
        // var entity = await Collection.FindOneAndDeleteAsync(filter);
        if (permanent)
        {
            return await Collection.FindOneAndDeleteAsync(filter);
        }

        var updateDef = Builders<TEntity>.Update.Set(nameof(BaseEntity.DeletedAt), DateTime.UtcNow);
        return await Collection.FindOneAndUpdateAsync(filter, updateDef, new FindOneAndUpdateOptions<TEntity>
        {
            IsUpsert = false,
            ReturnDocument = ReturnDocument.After
        });
    }

    public Task<DbResult> DeleteOneAsync(string documentId, bool permanent = false)
    {
        AssertObjectId(documentId);
        return DeleteAsync(FiltersBuilder<TEntity>.GetIdFilter(documentId), false, permanent);
    }

    public Task<DbResult> DeleteAsync(Expression<Func<TEntity, bool>> expr, bool permanent = false)
    {
        return DeleteAsync((FilterDefinition<TEntity>) expr, true, permanent);
    }

    public Task<DbResult> DeleteAsync(FilterDefinition<TEntity> filter, bool permanent = false)
    {
        return DeleteAsync(filter, true, permanent);
    }

    #region Protected Methods

    protected async Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(FilterDefinition<TEntity> filter, TUpdateModel payload, FindOneAndUpdateOptions<TEntity> opts)
    {
        var update = GenerateUpdateDefinition(payload, opts.IsUpsert);
        return await Collection.FindOneAndUpdateAsync(filter, update, opts);
    }

    /// <summary>
    /// Deletes one or more documents
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="deleteMany"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    protected async Task<DbResult> DeleteAsync(FilterDefinition<TEntity> filter, bool deleteMany, bool permanent)
    {
        if (permanent)
        {
            var result = deleteMany
                ? await Collection.DeleteManyAsync(filter)
                : await Collection.DeleteOneAsync(filter);
            return result.IsAcknowledged ? new DbResult(result.DeletedCount) : new DbResult(0);
        }

        var update = Builders<TEntity>.Update.Set(doc => doc.DeletedAt, DateTime.UtcNow);
        return await UpdateAsync(filter, update, deleteMany);
    }

    /// <summary>
    /// Automatically generates UpdateDefinition and updates documents
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="payload"></param>
    /// <param name="updateMany"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    protected Task<DbResult> UpdateAsync<TUpdateModel>(FilterDefinition<TEntity> filter, TUpdateModel payload,
        bool updateMany) where TUpdateModel : IUpdateModel
    {
        var update = GenerateUpdateDefinition(payload);
        return UpdateAsync(filter, update, updateMany);
    }

    /// <summary>
    /// Update documents
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="update"></param>
    /// <param name="updateMany"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected async Task<DbResult> UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity>? update, bool updateMany)
    {
        try
        {
            if (update is null) throw new ArgumentNullException(nameof(update));

            var result = updateMany
                ? await Collection.UpdateManyAsync(filter, update)
                : await Collection.UpdateOneAsync(filter, update);

            return new DbResult(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UpdateAsync failed. Error={Message}", ex.Message);
            throw;
        }
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
        return new []
        {
            nameof(BaseEntity.Id),
            nameof(BaseEntity.UpdatedAt),
            nameof(BaseEntity.CreatedAt),
            nameof(BaseEntity.DeletedAt),
        };
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
        if (!ObjectId.TryParse(id ?? string.Empty, out var _))
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Invalid document ID");
            throw new ArgumentException($"Invalid document ID format. Id={id}");
        }
    }

    #endregion

    #region Private methods

    private Task<IListViewModel<TEntity>> FindAsyncImpl(FilterDefinition<TEntity> filter, int offset, int limit, string? orderBy)
    {
        var sortDef = SortDefinitionHelper.GetSortDefinition<TEntity>(orderBy);
        return FindAsyncImpl(filter, offset, limit, sortDef);
    }

    private async Task<IListViewModel<TEntity>> FindAsyncImpl(FilterDefinition<TEntity> filter, int offset, int limit, SortDefinition<TEntity>? sort)
    {
        var opts = new FindOptions<TEntity> { Skip  = offset, Limit = limit };
        // Apply sorting
        if (sort != null) opts.Sort = sort;

        return await FindAsyncImpl(filter, opts);
    }

    private async Task<IListViewModel<TEntity>> FindAsyncImpl(FilterDefinition<TEntity> filter, FindOptions<TEntity> opts)
    {
        if (_logger != null)
        {
            // Render filter for logging
            var rendered = filter.Render(Collection.DocumentSerializer, Collection.Settings.SerializerRegistry);
            _logger.LogInformation("FindAsyncImpl: {FilterDef}", rendered);
        }

        var pageTask = Collection.FindAsync(filter, opts);

        var tasks = new List<Task<IAsyncCursor<TEntity>>> { pageTask };

        var offset = opts.Skip ?? 0;
        if (opts.Limit > 1)
        {
            var nextPageOpts = new FindOptions<TEntity> { Skip  = offset + opts.Limit.Value, Limit = 1 };
            if (opts.Sort != null) nextPageOpts.Sort = opts.Sort;

            var nextPageTask = Collection.FindAsync(filter, nextPageOpts);
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

    #endregion
}