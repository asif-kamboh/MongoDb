namespace ScientificBit.MongoDb.Entities.Common;

public class GeoPoint
{
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }

    public override string ToString()
    {
        return $"{Latitude},{Longitude}";
    }
}