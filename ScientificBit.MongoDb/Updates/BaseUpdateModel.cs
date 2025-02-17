namespace ScientificBit.MongoDb.Updates;

public interface IUpdateModel
{
    bool IsValid();
}

public class BaseUpdateModel : IUpdateModel
{
    public bool IsValid()
    {
        return true;
    }
}