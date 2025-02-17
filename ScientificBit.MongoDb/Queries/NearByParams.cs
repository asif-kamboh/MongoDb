namespace ScientificBit.MongoDb.Queries;

/// <summary>
/// Defines details of nearby search params
/// </summary>
public class NearByParams
{
    /// <summary>
    /// latitude
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// longitude
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Search radius in meters - Default is 200.0 meters
    /// </summary>
    public double Radius { get; set; } = 200.0; // Default
}