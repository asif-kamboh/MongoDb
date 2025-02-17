using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDb.Example.Entities;
using ScientificBit.MongoDb.Abstractions.Db;
using ScientificBit.MongoDb.Abstractions.Repositories;
using ScientificBit.MongoDb.Repositories;

namespace MongoDb.Example.Repositories;

public interface IUsersRepository : IBaseRepository<User>
{
}

public class UsersRepository : BaseRepository<User>, IUsersRepository
{
    public UsersRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }

    public UsersRepository(ILogger<UsersRepository>? logger, IMongoDbContext dbContext) : base(logger, dbContext)
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