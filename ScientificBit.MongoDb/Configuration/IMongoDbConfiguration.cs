using ScientificBit.MongoDb.Enums;

namespace ScientificBit.MongoDb.Configuration;

/// <summary>
/// MongoDB Configuration
/// </summary>
public interface IMongoDbConfiguration
{
    /// <summary>
    /// MongoDB Connection string in the format of "mongodb://user:pass@host1:port,host2:port?params" 
    /// </summary>
    string ConnectionString { get;}

    /// <summary>
    /// Database name
    /// </summary>
    string DatabaseName { get; }

    /// <summary>
    /// Database property naming convention. Default is camelCase
    /// </summary>
    PropertyNamingStyles PropertyNamingStyle { get; }
}