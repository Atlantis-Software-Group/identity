namespace asg.data.migrator;

public class SeedDataRecord
{
    public Type SeedScriptType {get; }
    public bool Applied { get; set; }

    public SeedDataRecord(Type seedScriptType)
        : this (seedScriptType, false) {}

    public SeedDataRecord(Type seedScriptType, bool applied)
    {
        SeedScriptType = seedScriptType;
        Applied = applied;
    }
}
