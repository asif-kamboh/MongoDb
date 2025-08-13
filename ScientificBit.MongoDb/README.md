# Introduction
The package implements a repository layer over MongoDB collections.
Developers can easily define CRUD operations on a Collection by just
deriving their collection-wise repository classes from `BaseRepository` class.

# Release Notes
## v3.0.0 (Release Date: 2025-08-11)
### Changes and Fixes
* Introduced `FindOneAsync` methods variants in `BaseRepository` class to find a single document by filter or by id.
* Added `BaseRepository` method variants to accept `IClientSessionHandle` for operations that require a session.
* **DEPRECATION:** `StartSession` method in `IMongoDbContext` is now deprecated. Use `BaseRepository.StartTransactionAsync` instead.
### Breaking Changes
* Removed `DeletedAt` field from `BaseEntity` class. If you want to support
  soft deletes, you can just define a `DeletedAt` field in your entity model.
* Removed `GetDeleteAtFilter` method from `FiltersBuilder` class.

## v2.2.3 (Release Date: 2025-08-10)
### Changes and Fixes
* **BUG FIX**: `CreateIndexes` method in `IMongoDbContext` now awaits for index creation to complete before returning.
* **BREAKING CHANGE:** Since `CreateIndexes` is no more fire-and-forget, it may break existing code that relies on the previous behavior. Ensure to handle the asynchronous nature of this method in your code.

## How to Integrate
The package is built for .NET 8, 9 and supports MongoDB v6+. 

### Configure Database
The package defines `IServiceCollection` extension to easily setup MongoDB.

The developer may use the following methods defined in `MongoDbServiceExtension` class:

```csharp
services.AddMongoDb(connectionString, databaseName);
```
OR
```csharp
services.AddMongoDb(opts =>
{
    opts.DatabaseName = "your-db-name";
    opts.ConnectionString = "db-connection-string";
    opts.PropertyNamingStyle = PropertyNamingStyles.CamelCase;
});
```

### Define an Entity Model
All entity models should be derived from `BaseEntity`. A default
collection name will be generated for the entity. If the developer
want to set a custom name, he can use `CollectionNameAttribute` attribute
when define the entity.
<br/><br/>Example:
```csharp
using ScientificBit.MongoDb.Entities;

[CollectionName("shopUsers")]
public class User : BaseEntity
{
    public string Name {get; set;}

    public string Email {get; set;}
}
```
Please note that, for the above entity definition, a default collection name
would have been `users`. 
 
### Define a Collection Repository
To define a repository class on your collection, you just need
to derive a class from `BaseRepository`.

Example implementation:

Define interface for users repository
```csharp
using ScientificBit.MongoDb.Abstractions.Repositories;

public interface IUsersRepository : IBaseRepository<User>
{
}
```

Define class for users repository
 
```csharp
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ScientificBit.MongoDb.Abstractions.Db;

internal class UsersRepository : BaseRepository<User>, IUsersRepository
{
    public UsersRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }

    public UsersRepository(ILogger<UsersRepository> logger, IMongoDbContext dbContext)
        : base(logger, dbContext)
    {
    }

    #region Overrides

    protected override IEnumerable<CreateIndexModel<User>> BuildIndexModels()
    {
        // Define default indexes
        var indexModels = base.BuildIndexModels().ToList();
        // Add Unique index for Email address
        indexModels.Add(
            new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(user => user.Email),
                new CreateIndexOptions {Background = true, Unique = true})
        );

        return indexModels;
    }

    protected override bool ShouldUpdateField(string field)
    {
        // Disallow updating user's email address
        return !string.Equals(field, nameof(User.Email), StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion
}
```

You may also define an update model to update specific user fields, e.g:

```csharp
using ScientificBit.MongoDb.Updates;

public class UserUpdateModel : IUpdateModel
{
    /// <summary>
    ///  Allow updates on `Name` field only
    /// </summary>
    public string? Name { get; set; }

    public bool IsValid()
    {
        // Don't allow empty names
        return !string.IsNullOrEmpty(Name);
    }
}
```

Please don't forget to take care of dependency injections. e.g
```csharp
services.AddScoped<IUsersRepository, UsersRepository>();
```

That's it! Your MongoDB is configured and you are all set to play
with your users collection.

## Using a repository
Please review code documentation to know more about CRUD operations.

## Documentation
Please refer code and inline docs

## Contribution
Contribution guidelines will be added shortly.

## Support The Developer
Please support me to help cover ongoing development, infrastructure costs, and the potential expansion of the project (including new features, testing, and documentation).

## Contact
Please contact asif@scientificbit.com for any information.
