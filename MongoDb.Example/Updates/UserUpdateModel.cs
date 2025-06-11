using ScientificBit.MongoDb.Updates;

namespace MongoDb.Example.Updates;

public class UserUpdateModel : BaseUpdateModel
{
    /// <summary>
    ///  Allow updates on `Name` field only
    /// </summary>
    public string? Name { get; set; }

    public override bool IsValid()
    {
        // Don't allow empty names
        return !string.IsNullOrEmpty(Name);
    }
}