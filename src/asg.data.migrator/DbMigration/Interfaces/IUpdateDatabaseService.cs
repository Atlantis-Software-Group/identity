using Microsoft.EntityFrameworkCore;

namespace asg.data.migrator.DbMigration.Interfaces;

public interface IUpdateDatabaseService
{
    Exception? Error { get; set; }
    
    Task<bool> MigrateAndSeedDatabase(DbContext context);
}
