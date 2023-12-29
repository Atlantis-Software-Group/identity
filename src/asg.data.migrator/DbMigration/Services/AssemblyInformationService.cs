using System.Reflection;
using asg.data.migrator.DbMigration.Interfaces;
using asg.data.migrator.SeedData.Attributes;
using asg.data.migrator.Services;
using Microsoft.Extensions.Logging;

namespace asg.data.migrator.DbMigration.Services;

public class AssemblyInformationService : IAssemblyInformationService
{
    public AssemblyInformationService(ILogger<AssemblyInformationService> logger)
    {
        Logger = logger;
    }

    public ILogger<AssemblyInformationService> Logger { get; }

    public IEnumerable<Type> GetSeedScripts(string dbContext, string migration)
    {
        Logger.LogInformation("DbContext: {DbContext} - Migration: {migration}", dbContext, migration);
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                                    .SelectMany(da => da.GetTypes())
                                    .Where(type => type.IsSubclassOf(typeof(SeedDataService)) 
                                            && !type.IsAbstract
                                            && type.GetCustomAttribute<MigrationNameAttribute>()?.MigrationName == migration
                                            && type.GetCustomAttribute<DatabaseNameAttribute>()?.Name == dbContext);

        return types;
    }
}
