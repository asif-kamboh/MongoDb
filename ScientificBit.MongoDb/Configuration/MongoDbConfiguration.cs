using ScientificBit.MongoDb.Enums;

namespace ScientificBit.MongoDb.Configuration;

/// <summary>
/// Define MongoDB configuration
/// </summary>
public class MongoDbConfiguration : IMongoDbConfiguration
{
    /// <summary>
    /// Connection string must be a valid MongoDB URL. It should also contain credentials
    /// Ref: https://www.mongodb.com/docs/manual/reference/connection-string/
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Document property naming convention. Default: camelCase
    /// </summary>
    public PropertyNamingStyles PropertyNamingStyle { get; set; } = PropertyNamingStyles.CamelCase;
}