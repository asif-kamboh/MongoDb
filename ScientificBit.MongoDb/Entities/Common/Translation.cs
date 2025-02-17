namespace ScientificBit.MongoDb.Entities.Common;

public class Translation
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Locale { get; set; } = "en";
}