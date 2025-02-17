using MongoDB.Driver;

namespace ScientificBit.MongoDb.Mongo;

public class DbResult
{
    public DbResult(UpdateResult result)
        : this(result.IsAcknowledged ? result.ModifiedCount : 0, result.IsAcknowledged ? result.MatchedCount : 0)
    {
    }

    public DbResult(long modifiedCount) : this(modifiedCount, modifiedCount)
    {
    }

    public DbResult(long modifiedCount, long matchedCount)
    {
        ModifiedCount = modifiedCount;
        MatchedCount = matchedCount;
    }

    /// <summary>
    /// MatchedCount can be greater than ModifiedCount 
    /// </summary>
    public long MatchedCount { get; }

    /// <summary>
    /// Total affected documents
    /// </summary>
    public long ModifiedCount { get; }
}