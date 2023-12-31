namespace asg.dbmigrator.SeedData.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MigrationNameAttribute : Attribute
{
    public MigrationNameAttribute(string migrationName)
    {
        MigrationName = migrationName;
    }

    public string MigrationName { get; private set; }
}
