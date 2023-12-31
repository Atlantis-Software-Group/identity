using asg.dbmigrator.DbMigration.Interfaces;
using asg.dbmigrator.DbMigration.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace asg.dbmigrator;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder AddInternalServiceProvider(this DbContextOptionsBuilder options)
    {                
        IServiceCollection efServiceCollection = new ServiceCollection();                
        efServiceCollection.AddScoped<ISeedDataHistoryRepository, SeedDataHistoryRepository>();
        efServiceCollection.AddEntityFrameworkSqlServer();

        options.UseInternalServiceProvider(efServiceCollection.BuildServiceProvider());

        return options;
    }
}
