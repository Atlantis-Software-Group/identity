using asg.data.DbContexts;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace asg.data.migrator;

public class DatabaseMigrationService : IHostedService
{
    public DatabaseMigrationService(ILogger<DatabaseMigrationService> logger, 
                                    IHostApplicationLifetime applicationLifetime,
                                    IServiceScopeFactory serviceScopeFactory)
    {
        Logger = logger;
        ApplicationLifetime = applicationLifetime;
        ServiceScopeFactory = serviceScopeFactory;
        ApplicationLifetime.ApplicationStarted.Register(OnStarted);
    }

    private async void OnStarted()
    {
        Logger.LogInformation("OnStarted has been called");

        using ( AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope() )
        {
            IServiceProvider sp = scope.ServiceProvider;
            List<DbContext> dbContexts = new List<DbContext>();

            ApplicationDbContext applicationDbContext = sp.GetRequiredService<ApplicationDbContext>();
            ConfigurationDbContext configurationDbContext = sp.GetRequiredService<ConfigurationDbContext>();
            PersistedGrantDbContext persistedGrantDbContext = sp.GetRequiredService<PersistedGrantDbContext>();
            
            dbContexts.Add(applicationDbContext);
            dbContexts.Add(configurationDbContext);
            dbContexts.Add(persistedGrantDbContext);

            foreach ( DbContext dbContext in dbContexts )
            {
                Logger.LogInformation("DbContext: {name}", dbContext.GetType());
                IMigrator? migrator = dbContext.GetInfrastructure().GetService<IMigrator>();

                if ( migrator is null )
                {
                    Logger.LogWarning("Skipping DbContext: {context} - Migrator not found.", dbContext.GetType());   
                    continue;
                }                

                IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                Logger.LogInformation("Found {MigrationCount} Migration(s)", pendingMigrations.Count());

                foreach ( string migration in pendingMigrations )
                {
                    Logger.LogInformation("Starting migration: {migration}", migration);                    
                    await migrator.MigrateAsync(migration);
                }
            }
        }
        ApplicationLifetime.StopApplication();
    }

    public ILogger<DatabaseMigrationService> Logger { get; }
    public IHostApplicationLifetime ApplicationLifetime { get; }
    public IServiceScopeFactory ServiceScopeFactory { get; }

    // initialize the application
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("StartAsync has been called");
        return Task.CompletedTask;
    }

    // clean up
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("StopAsync has been called");
        return Task.CompletedTask;
    }
}
