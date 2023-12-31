using Microsoft.EntityFrameworkCore;

namespace asg.dbmigrator.DbMigration.Interfaces;

public interface IUpdateDatabaseService
{
    Exception? Error { get; set; }
    
    Task<bool> MigrateAndSeedDatabase(DbContext context);
}
