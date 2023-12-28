using System.Text;
using asg.data.DbContexts;
using asg.data.migrator.Constants;
using asg.data.migrator.CreateSeedScript.Interfaces;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace asg.data.migrator;

public class DatabaseMigrationService : IHostedService
{
    public string? ErrorMessage { get; private set; }
    public DatabaseMigrationService(ILogger<DatabaseMigrationService> logger,
                                    IHostApplicationLifetime applicationLifetime,
                                    IServiceScopeFactory serviceScopeFactory,
                                    IConfiguration configuration,
                                    IHostEnvironment hostEnvironment,
                                    ICreateSeedScriptService createSeedScriptService,
                                    ICommandLineArgs commandLineArgs)
    {
        Logger = logger;
        ApplicationLifetime = applicationLifetime;
        ServiceScopeFactory = serviceScopeFactory;
        Configuration = configuration;
        HostEnvironment = hostEnvironment;
        CreateSeedScriptService = createSeedScriptService;
        CommandLineArgs = commandLineArgs;
        ApplicationLifetime.ApplicationStarted.Register(OnStarted);
    }

    private async void OnStarted()
    {
        Logger.LogInformation("OnStarted has been called");

        bool createSeedScript = CommandLineArgs.GetValue<bool>("-createSeedScript");
        Logger.LogInformation("Create SeedScript requested");

        if ( createSeedScript )
        {
            bool result = await CreateSeedScript();

            if ( !result )
            {
                Logger.LogError(ErrorMessage);
            }
        }
        else
        {
            await MigrateDatabases();
        }

        ApplicationLifetime.StopApplication();
    }

    public async Task<bool> CreateSeedScript()
    {
        string? scriptName = CommandLineArgs.GetValue<string>("-scriptName");

        if ( string.IsNullOrWhiteSpace(scriptName) )
        {
            ErrorMessage = ErrorMessageConstants.ScriptNameNotProvided;
            return false;
        }

        string? DbContextName = CommandLineArgs.GetValue<string>("-dbContextName");
        if ( string.IsNullOrWhiteSpace(DbContextName) )
        {
            ErrorMessage = ErrorMessageConstants.DbContextNameNotProvided;
            return false;
        }

        string scriptFullPath = Path.Combine(HostEnvironment.ContentRootPath, "SeedData", "Databases", DbContextName, $"{DateTime.Now:yyyyMMddHHmmss}_{scriptName}.cs");
        Logger.LogInformation("Scipt Output Path: {path}", scriptFullPath);

        string? migrationName = CommandLineArgs.GetValue<string>("-migrationName");
        if ( string.IsNullOrWhiteSpace(migrationName) )
        {
            ErrorMessage = ErrorMessageConstants.MigrationNameNotProvided;
            return false;
        }

        List<string> environmentNameArgs = CommandLineArgs.GetCollectionValue<string>("-environmentNames") ?? new List<string>();

        if ( environmentNameArgs.Count == 0 )
            environmentNameArgs.Add("Production");

        string[] environmentNames = environmentNameArgs.ToArray();

        string scriptCreated = await CreateSeedScriptService.CreateSeedScriptFile(scriptFullPath, scriptName, migrationName, environmentNames);

        if ( string.IsNullOrWhiteSpace(scriptCreated) )
            ErrorMessage = CreateSeedScriptService.ErrorMessage;

        return !string.IsNullOrWhiteSpace(scriptCreated);
    }

    public async Task MigrateDatabases()
    {
        using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();

        IServiceProvider sp = scope.ServiceProvider;
        List<DbContext> dbContexts = new List<DbContext>();

        ApplicationDbContext applicationDbContext = sp.GetRequiredService<ApplicationDbContext>();
        ConfigurationDbContext configurationDbContext = sp.GetRequiredService<ConfigurationDbContext>();
        PersistedGrantDbContext persistedGrantDbContext = sp.GetRequiredService<PersistedGrantDbContext>();

        dbContexts.Add(applicationDbContext);
        dbContexts.Add(configurationDbContext);
        dbContexts.Add(persistedGrantDbContext);

        foreach (DbContext dbContext in dbContexts)
        {
            Logger.LogInformation("DbContext: {name}", dbContext.GetType());
            IMigrator? migrator = dbContext.GetInfrastructure().GetService<IMigrator>();

            if (migrator is null)
            {
                Logger.LogWarning("Skipping DbContext: {context} - Migrator not found.", dbContext.GetType());
                continue;
            }

            IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            Logger.LogInformation("Found {MigrationCount} Migration(s)", pendingMigrations.Count());

            foreach (string migration in pendingMigrations)
            {
                Logger.LogInformation("Starting migration: {migration}", migration);
                await migrator.MigrateAsync(migration);
            }
        }
    }

    public ILogger<DatabaseMigrationService> Logger { get; }
    public IHostApplicationLifetime ApplicationLifetime { get; }
    public IServiceScopeFactory ServiceScopeFactory { get; }
    public IConfiguration Configuration { get; }
    public IHostEnvironment HostEnvironment { get; }
    public ICreateSeedScriptService CreateSeedScriptService { get; }
    public ICommandLineArgs CommandLineArgs { get; }

    // initialize the application
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("StartAsync has been called");
        Logger.LogInformation("CommandLineArgs: {@args}", CommandLineArgs.Options);
        return Task.CompletedTask;
    }

    // clean up
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("StopAsync has been called");
        return Task.CompletedTask;
    }
}
