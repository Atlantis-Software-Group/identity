using asg.dbmigrator.CommandLineParser;
using asg.dbmigrator.CreateSeedTemplate.Interfaces;
using asg.dbmigrator.CreateSeedTemplate.Services;
using asg.dbmigrator.DbMigration.Interfaces;
using asg.dbmigrator.DbMigration.Services;
using asg.dbmigrator.HostedService;
using asg.dbmigrator.Shared.Interfaces;
using asg.dbmigrator.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace asg.dbmigrator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbMigratorHostedService(this IServiceCollection services, Action<DbMigratorOptions> configureOptions)
    {
        services.Configure(configureOptions);                
        services.AddHostedService<DatabaseMigrationService>();
        services.AddScoped<IFileProviderService, FileProviderService>();
        services.AddScoped<ICreateSeedTemplateService, CreateSeedTemplateService>();
        services.AddScoped<IUpdateDatabaseService, UpdateDatabaseService>();
        services.AddScoped<IAssemblyInformationService, AssemblyInformationService>();
        services.AddScoped<ISeeder, Seeder>();
        return services;
    }

    
}
