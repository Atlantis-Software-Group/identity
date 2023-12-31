using System.Reflection;
using asg.dbmigrator.SeedData.Attributes;
using asg.dbmigrator.SeedData.Services;
using asg.dbmigrator.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace asg.dbmigrator.Shared.Services;

public class AssemblyInformationService : IAssemblyInformationService
{
    public AssemblyInformationService(ILogger<AssemblyInformationService> logger)
    {
        Logger = logger;
    }

    public ILogger<AssemblyInformationService> Logger { get; }

    public bool ClassExists(string typeName, string namespaceString)
    {
        Logger.LogInformation("Looking TypeName: {typeName} in Namespace: {namespace}", typeName, namespaceString);

        Type? type = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(da => da.GetTypes())
                                .Where(type => !type.IsAbstract 
                                            && type.IsSubclassOf(typeof(SeedDataService))
                                            && type.Namespace == namespaceString
                                            && type.Name == typeName)
                                .FirstOrDefault();

        return type is not null;
    }

    public IEnumerable<Type> GetSeedScripts(string dbContext, string migration, string environmentName)
    {
        Logger.LogInformation("DbContext: {DbContext} - Migration: {migration}", dbContext, migration);
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                                    .SelectMany(da => da.GetTypes())
                                    .Where(type => type.IsSubclassOf(typeof(SeedDataService)) 
                                            && !type.IsAbstract
                                            && type.GetCustomAttribute<MigrationNameAttribute>()?.MigrationName == migration
                                            && type.GetCustomAttribute<DatabaseNameAttribute>()?.Name == dbContext
                                            && type.GetCustomAttributes<SeedEnvironmentAttribute>()
                                                        .Any(sea => string.Compare(sea.EnvironmentName, environmentName, true) == 0));

        return types;
    }
}
