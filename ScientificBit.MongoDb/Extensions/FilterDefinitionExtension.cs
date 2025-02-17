using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ScientificBit.MongoDb.Extensions;

/// <summary>
/// Extensions for FilterDefinition
/// </summary>
public static class FilterDefinitionExtension
{
    /// <summary>
    /// Backward compatibility for `FilterDefinition.Render` method
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="serializer"></param>
    /// <param name="serializerRegistry"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static BsonDocument Render<TEntity>(this FilterDefinition<TEntity> filter,
        IBsonSerializer<TEntity> serializer, IBsonSerializerRegistry serializerRegistry)
    {
        var args = new RenderArgs<TEntity>(serializer, serializerRegistry);
        return filter.Render(args);
    }
}