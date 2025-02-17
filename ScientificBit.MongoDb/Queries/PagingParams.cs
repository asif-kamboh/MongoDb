namespace ScientificBit.MongoDb.Queries;

/// <inheritdoc cref="IPagingParams" />
public class PagingParams : IPagingParams, ISortParams
{
    /// <inheritdoc />
    public int? Offset { get; set; }

    /// <inheritdoc />
    public int? Limit { get; set; }
    
    /// <inheritdoc />
    public string? OrderBy { get; set; }
    
    /// <inheritdoc />
    public bool? IsDescending { get; set; }

    public string? ToOrderBy()
    {
        return !string.IsNullOrWhiteSpace(OrderBy) && IsDescending.HasValue && IsDescending.Value
            ? $"-{OrderBy}"
            : OrderBy;
    }
}