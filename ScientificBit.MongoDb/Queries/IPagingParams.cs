namespace ScientificBit.MongoDb.Queries;

/// <summary>
/// Defines params for pagination
/// </summary>
public interface IPagingParams
{
    /// <summary>
    /// current offset 
    /// </summary>
    public int? Offset { get; }

    /// <summary>
    /// Page size
    /// </summary>
    public int? Limit { get; }
}