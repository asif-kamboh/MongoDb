using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using ScientificBit.MongoDb.Abstractions.Db;
using ScientificBit.MongoDb.Configuration;
using ScientificBit.MongoDb.Enums;
using ScientificBit.MongoDb.Utils;

namespace ScientificBit.MongoDb.Extensions;

public static class MongoDbServiceExtension
{
    /// <summary>
    /// Configure MongoDB for given DB name and connection string
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public static IServiceCollection AddMongoDb(this IServiceCollection services, string connectionString, string databaseName)
    {
        return services.AddMongoDb(config =>
        {
            config.DatabaseName = databaseName;
            config.ConnectionString = connectionString;
        });
    }

    /// <summary>
    /// Configure MongoDB for given configuration
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurator"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IServiceCollection AddMongoDb(this IServiceCollection services, Action<MongoDbConfiguration> configurator)
    {
        var dbConfig = new MongoDbConfiguration();
        configurator.Invoke(dbConfig);

        if (string.IsNullOrEmpty(dbConfig.ConnectionString)) throw new ArgumentException("MongoDb connection string is required");

        if (string.IsNullOrEmpty(dbConfig.DatabaseName)) throw new ArgumentException("No database name configured");
        
        // Setup naming convention for MongoDb data. We don't need to setup TitleCase naming convention explicitly
        if (dbConfig.PropertyNamingStyle == PropertyNamingStyles.CamelCase)
        {
            ConventionRegistry.Register("camelCaseNamingConvention", new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            }, _ => true);
        }

        PropertyNamingStylesHelper.CurrentNamingStyle = dbConfig.PropertyNamingStyle;

        var mongoClient = new MongoClient(dbConfig.ConnectionString);

        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton<IMongoDbContext>(new MongoDbContext(mongoClient, dbConfig.DatabaseName));

        return services;
    }
}