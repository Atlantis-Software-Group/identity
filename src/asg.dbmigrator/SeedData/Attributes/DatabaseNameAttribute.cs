namespace asg.dbmigrator.SeedData.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DatabaseNameAttribute : Attribute
{
    public DatabaseNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
