using System.Linq.Expressions;
using MongoDB.Driver;
using ScientificBit.MongoDb.Entities;
using ScientificBit.MongoDb.Mongo;
using ScientificBit.MongoDb.Updates;
using ScientificBit.MongoDb.Views;

namespace ScientificBit.MongoDb.Abstractions.Repositories;

/// <summary>
/// Interface for Base repository, which defines basic CRUD operations on MongoDB
/// Users can define their own Repository classes derived from BaseRepository.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Get a document by ID
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    Task<TEntity?> GetAsync(string documentId);

    /// <summary>
    /// Get a document by ID in a given session
    /// </summary>
    /// <param name="session"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    Task<TEntity?> GetAsync(IClientSessionHandle? session, string documentId);

    /// <summary>
    /// Find one document with a given sort key or default sort key, i-e `_id`
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr, string? orderBy = null);

    /// <summary>
    /// Find one document with a given sort key or default sort key, i-e `_id`
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        string? orderBy = null);

    /// <summary>
    /// Find one document with a given sort definition
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort);

    /// <summary>
    /// Find one document with a given sort definition
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort);

    /// <summary>
    /// Find one document with a given sort key or default sort key, i-e `_id`
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter, string? orderBy = null);

    /// <summary>
    /// Find one document with a given sort key or default sort key, i-e `_id`
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        string? orderBy = null);

    /// <summary>
    /// Find one document with a given sort definition
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort);

    /// <summary>
    /// Find one document with a given sort definition
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filter"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        SortDefinition<TEntity> sort);

    /// <summary>
    /// Find all documents matching the given expression
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset, int? limit,
        string? orderBy = null);

    /// <summary>
    /// Find all documents matching the given expression
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        int? offset, int? limit, string? orderBy = null);

    /// <summary>
    /// Find all documents matching the given expression
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, int? offset, int? limit,
        SortDefinition<TEntity> sort);

    /// <summary>
    /// Find all documents matching the given expression
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        int? offset, int? limit, SortDefinition<TEntity> sort);

    /// <summary>
    /// Find all documents matching the given list of filter definitions.
    /// All filters are combined using AND logic
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters, int? offset, int? limit,
        string? orderBy = null);

    /// <summary>
    /// Find all documents matching the given list of filter definitions.
    /// All filters are combined using AND logic
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filters"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        IList<FilterDefinition<TEntity>> filters,
        int? offset, int? limit, string? orderBy = null);

    /// <summary>
    /// Find all documents matching the given list of filter definitions.
    /// All filters are combined using AND logic
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters, int? offset, int? limit,
        SortDefinition<TEntity> sort);

    /// <summary>
    /// Find all documents matching the given list of filter definitions.
    /// All filters are combined using AND logic
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filters"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session,
        IList<FilterDefinition<TEntity>> filters,
        int? offset, int? limit, SortDefinition<TEntity> sort);

    /// <summary>
    /// Find all documents matching the given filter definition
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, int? offset, int? limit,
        string? orderBy = null);

    /// <summary>
    /// Find all documents matching the given filter definition
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filter"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        int? offset, int? limit, string? orderBy = null);

    /// <summary>
    /// Find all documents matching the given filter definition
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, int? offset, int? limit,
        SortDefinition<TEntity> sort);

    /// <summary>
    /// Find all documents matching the given filter definition
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filter"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        int? offset, int? limit, SortDefinition<TEntity> sort);

    /// <summary>
    /// Find all documents matching the given filters
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IList<FilterDefinition<TEntity>> filters, FindOptions<TEntity> options);

    /// <summary>
    /// Find all documents matching the given filters
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filters"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, IList<FilterDefinition<TEntity>> filters,
        FindOptions<TEntity> options);

    /// <summary>
    /// Find all documents matching the given expression
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expr, FindOptions<TEntity> options);

    /// <summary>
    /// Find all documents matching the given expression
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        FindOptions<TEntity> options);

    /// <summary>
    /// Find all documents matching the given filter definition
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(FilterDefinition<TEntity> filter, FindOptions<TEntity> options);

    /// <summary>
    /// Find all documents matching the given filter definition
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filter"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    Task<IListViewModel<TEntity>> FindAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        FindOptions<TEntity> options);

    /// <summary>
    /// Count all documents matching this given filter expression
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    Task<long> CountDocumentsAsync(Expression<Func<TEntity, bool>> expr);

    /// <summary>
    /// Count all documents matching this given filter expression
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <returns></returns>
    Task<long> CountDocumentsAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr);

    /// <summary>
    /// Count all documents matching this given filter definition
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<long> CountDocumentsAsync(FilterDefinition<TEntity> filter);

    /// <summary>
    /// Count all documents matching this given filter definition
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<long> CountDocumentsAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter);

    /// <summary>
    /// Equivalent to collection.findOneAndUpdate method. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="payload"></param>
    /// <param name="returnUpdated"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(string documentId, TUpdateModel payload,
        bool returnUpdated = true) where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Equivalent to collection.findOneAndUpdate method. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="session"></param>
    /// <param name="documentId"></param>
    /// <param name="payload"></param>
    /// <param name="returnUpdated"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(IClientSessionHandle? session, string documentId,
        TUpdateModel payload, bool returnUpdated = true) where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Equivalent to collection.findOneAndUpdate method. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="payload"></param>
    /// <param name="isUpsert"></param>
    /// <param name="returnUpdated"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(Expression<Func<TEntity, bool>> expr, TUpdateModel payload,
        bool isUpsert, bool returnUpdated = true)  where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Equivalent to collection.findOneAndUpdate method. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="payload"></param>
    /// <param name="isUpsert"></param>
    /// <param name="returnUpdated"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<TEntity?> FindOneAndUpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr,
        TUpdateModel payload,
        bool isUpsert, bool returnUpdated = true) where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Create a single document
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    Task CreateAsync(TEntity document);

    /// <summary>
    /// Create a single document
    /// </summary>
    /// <param name="session"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    Task CreateAsync(IClientSessionHandle? session, TEntity document);

    /// <summary>
    /// Creates the given list of documents.
    /// </summary>
    /// <param name="documents"></param>
    /// <returns></returns>
    Task CreateManyAsync(IList<TEntity> documents);

    /// <summary>
    /// Creates the given list of documents.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="documents"></param>
    /// <returns></returns>
    Task CreateManyAsync(IClientSessionHandle? session, IList<TEntity> documents);

    /// <summary>
    /// Updates a single document. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="payload"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<DbResult> UpdateAsync<TUpdateModel>(TUpdateModel payload) where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Updates a single document. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="session"></param>
    /// <param name="payload"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session, TUpdateModel payload)
        where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Updates a single document. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="payload"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<DbResult> UpdateAsync<TUpdateModel>(string documentId, TUpdateModel payload) where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Updates a single document. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="session"></param>
    /// <param name="documentId"></param>
    /// <param name="payload"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session, string documentId, TUpdateModel payload)
        where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Updated documents matching the filter expression. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="payload"></param>
    /// <param name="updateMany"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<DbResult> UpdateAsync<TUpdateModel>(Expression<Func<TEntity, bool>> expr, TUpdateModel payload,
        bool updateMany) where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Updated documents matching the filter expression. The update definition is generated
    /// automatically based on TUpdateModel fields.
    /// All `null` values are ignored in TUpdateModel instance
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="payload"></param>
    /// <param name="updateMany"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<DbResult> UpdateAsync<TUpdateModel>(IClientSessionHandle? session,
        Expression<Func<TEntity, bool>> expr, TUpdateModel payload,
        bool updateMany) where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Update documents in bulk. `IUpdateModel.Id` filed is used to match documents.
    /// </summary>
    /// <param name="updates"></param>
    /// <typeparam name="TUpdateModel"></typeparam>
    /// <returns></returns>
    Task<BulkWriteResult<TEntity>> BulkUpdateAsync<TUpdateModel>(IList<TUpdateModel> updates)
        where TUpdateModel : IUpdateModel;

    /// <summary>
    /// Deletes a document with a given I D.
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAndDeleteAsync(string documentId, bool permanent = false);

    /// <summary>
    /// Deletes a document with a given I D.
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="session"></param>
    /// <param name="documentId"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session, string documentId, bool permanent = false);

    /// <summary>
    /// Deletes a document matching the filter expression
    /// Deleting the document just updates `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAndDeleteAsync(Expression<Func<TEntity, bool>> expr, bool permanent = false);

    /// <summary>
    /// Deletes a document matching the filter expression
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        bool permanent = false);

    /// <summary>
    /// Deletes a document matching the filter expression.
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAndDeleteAsync(FilterDefinition<TEntity> filter, bool permanent = false);

    /// <summary>
    /// Deletes a document matching the filter expression.
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filter"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAndDeleteAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        bool permanent = false);

    /// <summary>
    /// Deletes a document with given ID.
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<DbResult> DeleteOneAsync(string documentId, bool permanent = false);

    /// <summary>
    /// Deletes a document with given ID.
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="session"></param>
    /// <param name="documentId"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<DbResult> DeleteOneAsync(IClientSessionHandle? session, string documentId, bool permanent = false);

    /// <summary>
    /// Deletes all documents matching the given filter expression
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<DbResult> DeleteAsync(Expression<Func<TEntity, bool>> expr, bool permanent = false);

    /// <summary>
    /// Deletes all documents matching the given filter expression
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expr"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<DbResult> DeleteAsync(IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        bool permanent = false);

    /// <summary>
    /// Deletes all documents matching the given filter definition
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<DbResult> DeleteAsync(FilterDefinition<TEntity> filter, bool permanent = false);

    /// <summary>
    /// Deletes all documents matching the given filter definition
    /// If soft delete is supported, it just updates the `DeletedAt` property. To delete documents permanently, set
    /// pass `permanent = true` to this function
    /// </summary>
    /// <param name="session"></param>
    /// <param name="filter"></param>
    /// <param name="permanent"></param>
    /// <returns></returns>
    Task<DbResult> DeleteAsync(IClientSessionHandle? session, FilterDefinition<TEntity> filter, bool permanent = false);

    /// <summary>
    /// Generate an auto-increment ID for underlying entity
    /// </summary>
    /// <param name="startValue"></param>
    /// <returns></returns>
    Task<long> GenerateAutoIncrementIdAsync(long startValue = 0);

    /// <summary>
    /// Generate an auto-increment ID for given sequence name
    /// </summary>
    /// <param name="sequenceName"></param>
    /// <param name="startValue"></param>
    /// <returns></returns>
    Task<long> GenerateAutoIncrementIdAsync(string sequenceName, long startValue = 0);
}