using asg.dbmigrator.DbMigration.Interfaces;
using asg.dbmigrator.SeedData.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace asg.dbmigrator.DbMigration.Services;

public class Seeder : ISeeder
{    
    public Seeder(ILogger<Seeder> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    public ILogger<Seeder> Logger { get; }
    public IServiceProvider ServiceProvider { get; }

    public async Task SeedAsync(Type seedDataType)
    {        
        Logger.LogInformation("Starting {scriptName}",seedDataType.Name);
        ISeedData instance = (ISeedData)ActivatorUtilities.CreateInstance(ServiceProvider, seedDataType);

        try
        {
            await instance.Seed();   
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error encountered while running {scriptName}", seedDataType.Name);
            throw;
        }

        Logger.LogInformation("{scriptName} Completed successfully", seedDataType.Name);
    }
}
