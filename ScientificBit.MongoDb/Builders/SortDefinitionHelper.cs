using MongoDB.Driver;

namespace ScientificBit.MongoDb.Builders;

/// <summary>
/// Defines methods for MongoDB Sort Definitions
/// </summary>
public static class SortDefinitionHelper
{
    /// <summary>
    /// Support for sorting on one field only. Use prefix '-' for descending sort
    /// </summary>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    public static SortDefinition<TEntity>? GetSortDefinition<TEntity>(string? orderBy)
    {
        return CreateSortDefinition<TEntity>(orderBy);
    }

    /// <summary>
    /// Support for sorting on multiple fields. Use prefix '-' for descending sort
    /// </summary>
    /// <param name="orderBy"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static SortDefinition<TEntity>? GetSortDefinition<TEntity>(IEnumerable<string> orderBy)
    {
        // Filter valid fields only
        var fields = orderBy.Select(f => f.Trim())
            .Where(f => !string.IsNullOrEmpty(f))
            .Distinct()
            .ToList();

        if (!fields.Any()) return default;

        var sortDefinitions = fields.Select(CreateSortDefinition<TEntity>)
            .Where(def => def != null)
            .ToList();

        if (!sortDefinitions.Any()) return default;

        return sortDefinitions.Count > 1 ? Builders<TEntity>.Sort.Combine(sortDefinitions) : sortDefinitions[0];
    }

    private static SortDefinition<TEntity>? CreateSortDefinition<TEntity>(string? field)
    {
        field = field?.Trim();

        if (string.IsNullOrEmpty(field)) return default;

        var isDescending = field[0] == '-';
        // Remove sort prefix
        var fieldName = field.TrimStart('-');

        // Check validity of orderBy param
        var prop = typeof(TEntity).GetProperties()
            .FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
        if (prop is null) return default;

        return isDescending
            ? Builders<TEntity>.Sort.Descending(prop.Name)
            : Builders<TEntity>.Sort.Ascending(prop.Name);
    }
}