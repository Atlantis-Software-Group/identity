namespace asg.dbmigrator.DbMigration.Interfaces;

public interface ISeeder
{
    Task SeedAsync(Type seedDataType);
}
