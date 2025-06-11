namespace ScientificBit.MongoDb.Updates;

public interface IUpdateModel
{
    /// <summary>
    /// ID is only required for bulk updates.
    /// </summary>
    string? Id { get; }

    bool IsValid();
}

public class BaseUpdateModel : IUpdateModel
{
    public string? Id { get; set; }

    public virtual bool IsValid() => true;
}