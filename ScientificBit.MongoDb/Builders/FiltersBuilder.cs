using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using ScientificBit.MongoDb.Entities;
using ScientificBit.MongoDb.Entities.Common;
using ScientificBit.MongoDb.Queries;
using ScientificBit.MongoDb.Utils;

namespace ScientificBit.MongoDb.Builders;

/// <summary>
/// Helper utility for filter definitions
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public static class FiltersBuilder<TEntity>
{
    public static FilterDefinition<TEntity> GetIdFilter(string documentId)
    {
        if (!ObjectId.TryParse(documentId, out _))
        {
            throw new ArgumentException($"Invalid document Id ({documentId})");
        }

        return And(new List<FilterDefinition<TEntity>>
        {
            Builders<TEntity>.Filter.Eq(nameof(BaseEntity.Id), documentId),
            GetDeleteAtFilter()
        });
    }

    public static FilterDefinition<TEntity> And(IList<FilterDefinition<TEntity>> filters)
    {
        if (filters.Count == 0) return Builders<TEntity>.Filter.Empty;
        return filters.Count == 1 ? filters[0] : Builders<TEntity>.Filter.And(filters);
    }

    public static FilterDefinition<TEntity> Or(IList<FilterDefinition<TEntity>> filters)
    {
        if (filters.Count == 0) return Builders<TEntity>.Filter.Empty;
        return filters.Count == 1 ? filters[0] : Builders<TEntity>.Filter.Or(filters);
    }

    public static FilterDefinition<TEntity> GetDeleteAtFilter()
    {
        return Builders<TEntity>.Filter.Eq(nameof(BaseEntity.DeletedAt), BsonNull.Value);
    }

    public static FilterDefinition<TEntity>? GetTextSearchFilter(string? value)
    {
        // filter = {$text: {$search: "court"}}
        return !string.IsNullOrEmpty(value)
            ? Builders<TEntity>.Filter.Text(value, new TextSearchOptions {CaseSensitive = false})
            : null;
    }

    public static FilterDefinition<TEntity> GetTextFilter(string field, string? value)
    {
        field = ValidateField(field);
        return string.IsNullOrEmpty(value)
            ? Builders<TEntity>.Filter.Eq(field, value)
            : Builders<TEntity>.Filter.Regex(field, new BsonRegularExpression(new Regex(value, RegexOptions.IgnoreCase)));
    }

    public static FilterDefinition<TEntity> GetEqualityFilter<TField>(string field, TField? value)
    {
        field = ValidateField(field);
        return Builders<TEntity>.Filter.Eq(field, value);
    }

    public static FilterDefinition<TEntity> GetInequalityFilter<TField>(string field, TField? value)
    {
        field = ValidateField(field);
        return Builders<TEntity>.Filter.Ne(field, value);
    }
    
    public static FilterDefinition<TEntity> GetListInequalityFilter<TField>(string field, IEnumerable<TField?> value)
    {
        field = ValidateField(field);
        return Builders<TEntity>.Filter.Nin(field, value);
    }
    
    public static FilterDefinition<TEntity> GetListEqualityFilter<TField>(string field, IEnumerable<TField?> value)
    {
        field = ValidateField(field);
        return Builders<TEntity>.Filter.In(field, value);
    }

    public static FilterDefinition<TEntity> GetGreaterThanOrEqualFilter<TField>(string field, TField value)
    {
        field = ValidateField(field);
        return Builders<TEntity>.Filter.Gte(field, value);
    }

    public static FilterDefinition<TEntity> GetDateRangeFilter(string field, DateTime from, DateTime to)
    {
        field = ValidateField(field);
        var filters = new List<FilterDefinition<TEntity>>
        {
            Builders<TEntity>.Filter.Gte(field, from),
            Builders<TEntity>.Filter.Lt(field, to)
        };
        return Builders<TEntity>.Filter.And(filters);
    }

    public static FilterDefinition<TEntity> GetTimeAvailabilityFilter(string field, bool isOpen)
    {
        field = ValidateField(field);

        // TODO: Use DateUtils
        var date = DateTime.UtcNow.AddHours(3); // for Qatar only
        var timeOfDay = date.Hour * 60 + date.Minute;
        var dayOfWeek = (int) date.DayOfWeek;
        var avFilters = isOpen
            ? GetIsOpenFilter(dayOfWeek, timeOfDay)
            : GetIsClosedFilter(dayOfWeek, timeOfDay);
        // TODO: Add Filters for next day
        return Builders<TEntity>.Filter.ElemMatch(field, avFilters);
    }

    public static FilterDefinition<TEntity> GetGeoNearFilter(string field, NearByParams query)
    {
        return GetGeoNearFilter(field, query.Latitude, query.Longitude, query.Radius);
    }

    private static FilterDefinition<TEntity> GetGeoNearFilter(string field, double lat, double lon, double radius)
    {
        field = ValidateField(field);

        var coords = new BsonDocument
        {
            {"type", "Point"},
            {"coordinates", new BsonArray(new [] { lon, lat })}
        };

        var geometry = new BsonDocument
        {
            {"$geometry", coords},
            {"$maxDistance", radius}
        };

        var near = new BsonDocument("$near", geometry);

        // Since we are using Bson document, we are responsible for naming convention
        field = PropertyNamingStylesHelper.ToCurrentNamingStyle(field);

        return new BsonDocumentFilterDefinition<TEntity>(new BsonDocument(field, near));
    }

    #region Helper methods

    private static FilterDefinition<DayTiming> GetIsOpenFilter(int dayOfWeek, int timeOfDay)
    {
        var avBuilder = Builders<DayTiming>.Filter;
        return avBuilder.And(
            avBuilder.Eq(d => (int)d.Day, dayOfWeek),
            avBuilder.Lte(d => d.Start, timeOfDay),
            avBuilder.Gt(d => d.End, timeOfDay)
        );
    }

    private static FilterDefinition<DayTiming> GetIsClosedFilter(int dayOfWeek, int timeOfDay)
    {
        var avBuilder = Builders<DayTiming>.Filter;
        return avBuilder.And(
            avBuilder.Eq(d => (int) d.Day, dayOfWeek),
            avBuilder.Or(avBuilder.Gt(d => d.Start, timeOfDay), avBuilder.Lte(d => d.End, timeOfDay))
        );
    }

    private static string ValidateField(string field)
    {
        if (string.IsNullOrEmpty(field)) throw new ArgumentException("Filter key is empty");
        var prop = typeof(TEntity).GetProperties()
            .FirstOrDefault(p => p.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase));
        if (prop != null) return prop.Name;
        throw new ArgumentException($"Invalid field - {field}");
    }

    #endregion
}