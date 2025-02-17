namespace ScientificBit.MongoDb.Queries;

/// <summary>
/// Defines params for $geoNear search
/// </summary>
public interface INearBySearchParams
{
    /// <summary>
    /// $geoNear search params
    /// </summary>
    NearByParams? NearBy { get; }
}