namespace asg.dbmigrator.SeedData.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SeedEnvironmentAttribute : Attribute
{
    public string EnvironmentName { get; }
    
    public SeedEnvironmentAttribute(string environmentName)
    {
        EnvironmentName = environmentName;
    }
}
