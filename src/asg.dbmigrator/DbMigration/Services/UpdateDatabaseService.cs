using asg.dbmigrator.DbMigration.Interfaces;
using asg.dbmigrator.DbMigration.Models;
using asg.dbmigrator.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace asg.dbmigrator.DbMigration.Services;

public class UpdateDatabaseService : IUpdateDatabaseService
{
    public UpdateDatabaseService(ILogger<UpdateDatabaseService> logger, 
                                    IAssemblyInformationService assemblyInformation,
                                    IHostEnvironment hostEnvironment,
                                    ISeeder seeder)
    {
        Logger = logger;
        AssemblyInformation = assemblyInformation;
        HostEnvironment = hostEnvironment;
        Seeder = seeder;
    }

    public ILogger<UpdateDatabaseService> Logger { get; }
    public IAssemblyInformationService AssemblyInformation { get; }
    public IHostEnvironment HostEnvironment { get; }
    public ISeeder Seeder { get; }
    public Exception? Error { get; set; }

    public async Task<bool> MigrateAndSeedDatabase(DbContext context)
    {        
        Logger.LogInformation("DbContext: {name}", context.GetType());
        
        IMigrator? migrator = context.GetInfrastructure().GetService<IMigrator>();

        if (migrator is null)
        {
            Logger.LogError("DbContext: {context} - Migrator not found.", context.GetType());
            Error = new NullReferenceException("Migrator not found for DbContext");
            return false;
        }

        List<MigrationRecord> migrationMap = await BuildMigrationMap(context);

        foreach ( MigrationRecord migration in migrationMap )
        {
            if ( !migration.MigrationAlreadyApplied )
            {
                await migrator.MigrateAsync(migration.Migration);
                migration.MigrationAlreadyApplied = true;

                foreach ( SeedDataRecord seedData in migration.SeedDataRecords )
                {
                    if ( seedData.Applied )
                        continue;

                    // run through each seed script
                    await Seeder.SeedAsync(seedData.SeedScriptType);

                    // update SeedDataScriptHistory
                    await context.Database.InsertSeedScriptHistoryRow(seedData.SeedScriptType.Name, migration.Migration);
                }
            }
        }
        
        return true;
    }

    /// <summary>
    /// Build a Map of all the migrations that need to be applied along with SeedScripts for each migrations. 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns>Collection of MigrationRecords that need to be applied to the database</returns>
    public async Task<List<MigrationRecord>> BuildMigrationMap(DbContext context)
    {        
        List<MigrationRecord> map = new List<MigrationRecord>();

        string? lastMigration = await context.Database.GetLastAppliedMigrationAsync();

        if ( lastMigration is not null )
        {
            // get SeedScripts for this migration. 
            List<SeedDataRecord> records = await GetSeedDataRecords(context, lastMigration);
            map.Add(new MigrationRecord(lastMigration, true, records));
        }

        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        foreach ( string pendingMigration in pendingMigrations )
        {
            // get SeedScripts for this migration. 
            List<SeedDataRecord> records = await GetSeedDataRecords(context, pendingMigration);
            map.Add(new MigrationRecord(pendingMigration, false, records));
        }

        return map;
    }

    public async Task<List<SeedDataRecord>> GetSeedDataRecords(DbContext context, string migrationName)
    {
        List<SeedDataRecord> records = new List<SeedDataRecord>();

        IEnumerable<string> appliedSeedRecords = await context.Database.GetSeedScriptsForMigration(migrationName);

        IEnumerable<Type> seedScriptTypes = AssemblyInformation.GetSeedScripts(context.GetType().Name, migrationName, HostEnvironment.EnvironmentName);
        foreach ( Type seedScriptType in seedScriptTypes )
        {            
            string? appliedSeedScriptType = appliedSeedRecords.Where(sr => sr == seedScriptType.Name).SingleOrDefault() ;
            records.Add(new SeedDataRecord(seedScriptType, !string.IsNullOrWhiteSpace(appliedSeedScriptType)));
        }

        return records;
    }
}
