namespace ScientificBit.MongoDb.Queries;

/// <summary>
/// Defines params for sorting
/// </summary>
public interface ISortParams
{
    /// <summary>
    /// Sort field
    /// </summary>
    string? OrderBy { get; }

    /// <summary>
    /// true for descending sort
    /// </summary>
    bool? IsDescending { get; }

    /// <summary>
    /// Returns sort field with sign
    /// </summary>
    /// <returns></returns>
    string? ToOrderBy();
}