namespace ScientificBit.MongoDb.Entities.Common;

public class DayTiming
{
    public DayOfWeek Day { get; set; }

    public int Start { get; set; }

    public int End { get; set; }
}