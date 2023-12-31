using asg.dbmigrator.CommandLineParser;
using asg.dbmigrator.Constants;
using asg.dbmigrator.CreateSeedTemplate.Interfaces;
using asg.dbmigrator.DbMigration.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace asg.dbmigrator.HostedService;

public class DatabaseMigrationService : IHostedService
{
    public string? ErrorMessage { get; private set; }
    public DatabaseMigrationService(ILogger<DatabaseMigrationService> logger,
                                    IHostApplicationLifetime applicationLifetime,
                                    IServiceScopeFactory serviceScopeFactory,
                                    IConfiguration configuration,
                                    IHostEnvironment hostEnvironment,
                                    IOptions<DbMigratorOptions> options)
    {
        Logger = logger;
        ApplicationLifetime = applicationLifetime;
        ServiceScopeFactory = serviceScopeFactory;
        Configuration = configuration;
        HostEnvironment = hostEnvironment;
        Options = options.Value;
        CommandLineArgs = Options.ParsedArgs;
        ApplicationLifetime.ApplicationStarted.Register(OnStarted);
    }

    private async void OnStarted()
    {
        Dictionary<string, object> scopeState = new Dictionary<string, object>{
            {"TraceId", Guid.NewGuid().ToString() }
        };
        using var scope = Logger.BeginScope(scopeState);
        Logger.LogInformation("OnStarted has been called");

        bool createSeedScript = CommandLineArgs.GetValue<bool>("-createSeedScript");
        Logger.LogInformation("Create SeedScript requested");

        if ( createSeedScript )
        {
            bool result = await CreateSeedTemplate();

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

    public async Task<bool> CreateSeedTemplate()
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

        using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
        CreateSeedTemplateService = scope.ServiceProvider.GetRequiredService<ICreateSeedTemplateService>();
        string scriptCreated = await CreateSeedTemplateService.CreateSeedTemplateFile(scriptFullPath, scriptName, migrationName, DbContextName, environmentNames);

        if ( string.IsNullOrWhiteSpace(scriptCreated) )
            ErrorMessage = CreateSeedTemplateService.ErrorMessage;

        return !string.IsNullOrWhiteSpace(scriptCreated);
    }

    public async Task MigrateDatabases()
    {
        using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();

        IServiceProvider sp = scope.ServiceProvider;
        List<DbContext> dbContexts = new List<DbContext>();

        UpdateDatabaseService = sp.GetRequiredService<IUpdateDatabaseService>();

        // ApplicationDbContext applicationDbContext = sp.GetRequiredService<ApplicationDbContext>();
        // ConfigurationDbContext configurationDbContext = sp.GetRequiredService<ConfigurationDbContext>();
        // PersistedGrantDbContext persistedGrantDbContext = sp.GetRequiredService<PersistedGrantDbContext>();

        foreach ( Type dbType in Options.DbContextTypes )
        {
            DbContext dbContext = (DbContext)sp.GetRequiredService(dbType);
            dbContexts.Add(dbContext);
        }       

        foreach (DbContext dbContext in dbContexts)
        {
            bool migrationSuccessful = false;
            try
            {
                migrationSuccessful = await UpdateDatabaseService.MigrateAndSeedDatabase(dbContext);
            }
            catch (Exception e )
            {
                Logger.LogError(e, "error occurred during Migration and Seeding process");
                throw;
            }

            if ( !migrationSuccessful )
            {
                if ( UpdateDatabaseService.Error is null )
                {
                    Exception unknownError = new Exception("Unknown error occured!");
                    Logger.LogCritical(unknownError, "Unknown error occurred while Migrating and Seeding - Database: {db}", typeof(DbContext));
                    throw unknownError;
                }
                
                Logger.LogError(UpdateDatabaseService.Error, "Error encountered while Migrating and Seeding - Database: {db}", typeof(DbContext));
                throw UpdateDatabaseService.Error;
            }
        }
    }

    public ILogger<DatabaseMigrationService> Logger { get; }
    public IHostApplicationLifetime ApplicationLifetime { get; }
    public IServiceScopeFactory ServiceScopeFactory { get; }
    public IConfiguration Configuration { get; }
    public IHostEnvironment HostEnvironment { get; }
    public DbMigratorOptions Options { get; }
    public ICommandLineArgs CommandLineArgs { get; }
    public ICreateSeedTemplateService? CreateSeedTemplateService { get; set; }
    public IUpdateDatabaseService? UpdateDatabaseService { get; set; }

    // initialize the application
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("StartAsync has been called");
        Logger.LogInformation("CommandLineArgs: {@args}", Options.ParsedArgs);
        return Task.CompletedTask;
    }

    // clean up
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("StopAsync has been called");
        return Task.CompletedTask;
    }
}
