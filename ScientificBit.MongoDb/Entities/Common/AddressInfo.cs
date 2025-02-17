namespace ScientificBit.MongoDb.Entities.Common;

public class AddressInfo
{
    public string Address1 { get; set; } = string.Empty;

    public string? Address2 { get; set; }

    public string Area { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public GeoPoint? Location { get; set; }
}