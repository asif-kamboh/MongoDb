using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ScientificBit.MongoDb.Entities;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
    }
    
    /// <summary>
    /// Gets or sets the Id of the Entity.
    /// </summary>
    /// <value>Id of the Entity.</value>        
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; }

    public virtual void Validate()
    {
        if (!ObjectId.TryParse(Id, out _))
        {
            Id = ObjectId.GenerateNewId().ToString();
        }
        // TODO: Add more generic validations
    }
}