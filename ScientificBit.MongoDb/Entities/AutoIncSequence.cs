using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ScientificBit.MongoDb.Entities;

internal class AutoIncSequence
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;

    public long Value { get; set; } = 0;
}