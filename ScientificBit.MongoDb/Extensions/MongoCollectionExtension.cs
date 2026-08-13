using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ScientificBit.MongoDb.Extensions;

public static class MongoCollectionExtension
{
    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection, string documentId,
        CancellationToken cancellationToken = default)
    {
        return collection.FindOneAsync(null, documentId, cancellationToken);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        Expression<Func<TEntity, bool>> expr, CancellationToken cancellationToken = default)
    {
        return collection.FindOneAsync(null, expr, cancellationToken);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort,
        CancellationToken cancellationToken = default)
    {
        return collection.FindOneAsync(null, expr, sort, cancellationToken);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
    {
        return collection.FindOneAsync(null, filter, sort: null, cancellationToken);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort,
        CancellationToken cancellationToken = default)
    {
        return collection.FindOneAsync(null, filter, sort, cancellationToken);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, string documentId, CancellationToken cancellationToken = default)
    {
        var filter = new BsonDocumentFilterDefinition<TEntity>(new BsonDocument("_id", ObjectId.Parse(documentId)));
        return collection.FindOneAsync(session, filter, sort: null, cancellationToken);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr,
        CancellationToken cancellationToken = default)
    {
        return collection.FindOneAsync(session, (FilterDefinition<TEntity>)expr, cancellationToken);
    }

    public static async Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort,
        CancellationToken cancellationToken = default)
    {
        var opts = new FindOptions<TEntity>
        {
            Limit = 1,
            Sort = sort
        };

        var cursor = session != null
            ? await collection.FindAsync(session, expr, opts, cancellationToken)
            : await collection.FindAsync(expr, opts, cancellationToken);

        return await cursor.FirstOrDefaultAsync(cancellationToken);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, FilterDefinition<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        return collection.FindOneAsync(session, filter, sort: null, cancellationToken: cancellationToken);
    }

    public static async Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, FilterDefinition<TEntity> filter, SortDefinition<TEntity>? sort,
        CancellationToken cancellationToken = default)
    {
        var opts = new FindOptions<TEntity> {Limit = 1};
        if (sort != null) opts.Sort = sort;

        var cursor = session != null
            ? await collection.FindAsync(session, filter, opts, cancellationToken)
            : await collection.FindAsync(filter, opts, cancellationToken);
        return await cursor.FirstOrDefaultAsync(cancellationToken);
    }
}