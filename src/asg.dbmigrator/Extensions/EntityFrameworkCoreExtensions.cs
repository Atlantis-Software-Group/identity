using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using asg.dbmigrator.DbMigration.Interfaces;
using asg.dbmigrator.DbMigration.Models;

namespace Microsoft.EntityFrameworkCore;

public static class EntityFrameworkCoreExtensions
{
    public static async Task<string?> GetLastAppliedMigrationAsync(this DatabaseFacade databaseFacade, CancellationToken cancellationToken = default(CancellationToken))
    {
        IHistoryRepository historyRepo = databaseFacade.GetRelationalService<IHistoryRepository>();

        IReadOnlyList<HistoryRow> historyRows = await historyRepo.GetAppliedMigrationsAsync(cancellationToken);

        return historyRows.Select(hr => hr.MigrationId)?.LastOrDefault();
    }

    public static async Task<IEnumerable<string>> GetSeedScriptsForMigration(this DatabaseFacade databaseFacade, string migrationName, CancellationToken cancellationToken = default(CancellationToken))    
    {
        ISeedDataHistoryRepository seedDataHistoryRepo = databaseFacade.GetRelationalService<ISeedDataHistoryRepository>();
        IReadOnlyList<SeedScriptHistoryRow> historyRows = await seedDataHistoryRepo.GetAppliedSeedScriptsForMigrationAsync(migrationName);

        return historyRows.Select(x => x.Name).AsEnumerable<string>();
    }

    public static async Task<SeedScriptHistoryRow> InsertSeedScriptHistoryRow(this DatabaseFacade databaseFacade, string name, string migrationName, CancellationToken cancellationToken = default(CancellationToken))    
    {
        ISeedDataHistoryRepository seedDataHistoryRepo = databaseFacade.GetRelationalService<ISeedDataHistoryRepository>();
        SeedScriptHistoryRow historyRow = await seedDataHistoryRepo.InsertHistoryRow(name, migrationName);

        return historyRow;
    }
    
    private static TService GetRelationalService<TService>(this IInfrastructure<IServiceProvider> databaseFacade)
    {
        var service = databaseFacade.Instance.GetService<TService>();
        return service == null
            ? throw new InvalidOperationException(RelationalStrings.RelationalNotInUse)
            : service;
    }
}
