namespace ScientificBit.MongoDb.Queries;

/// <summary>
/// Defines params for free form text search
/// </summary>
public interface ITextSearchParams
{
    /// <summary>
    /// Text search query
    /// </summary>
    string? Query { get; }
}