using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ScientificBit.MongoDb.Extensions;

public static class MongoCollectionExtension
{
    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection, string documentId)
    {
        return collection.FindOneAsync(null, documentId);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        Expression<Func<TEntity, bool>> expr)
    {
        return collection.FindOneAsync(null, expr);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort)
    {
        return collection.FindOneAsync(null, expr, sort);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        FilterDefinition<TEntity> filter)
    {
        return collection.FindOneAsync(null, filter, sort: null);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort)
    {
        return collection.FindOneAsync(null, filter, sort);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, string documentId)
    {
        var filter = new BsonDocumentFilterDefinition<TEntity>(new BsonDocument("_id", ObjectId.Parse(documentId)));
        return collection.FindOneAsync(session, filter, sort: null);
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr)
    {
        return collection.FindOneAsync(session, (FilterDefinition<TEntity>)expr);
    }

    public static async Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, Expression<Func<TEntity, bool>> expr, SortDefinition<TEntity> sort)
    {
        var opts = new FindOptions<TEntity>
        {
            Limit = 1,
            Sort = sort
        };

        var cursor = session != null
            ? await collection.FindAsync(session, expr, opts)
            : await collection.FindAsync(expr, opts);

        return await cursor.FirstOrDefaultAsync();
    }

    public static Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, FilterDefinition<TEntity> filter)
    {
        return collection.FindOneAsync(session, filter, sort: null);
    }

    public static async Task<TEntity?> FindOneAsync<TEntity>(this IMongoCollection<TEntity> collection,
        IClientSessionHandle? session, FilterDefinition<TEntity> filter, SortDefinition<TEntity>? sort)
    {
        var opts = new FindOptions<TEntity> {Limit = 1};
        if (sort != null) opts.Sort = sort;

        var cursor = session != null
            ? await collection.FindAsync(session, filter, opts)
            : await collection.FindAsync(filter, opts);
        return await cursor.FirstOrDefaultAsync();
    }
}