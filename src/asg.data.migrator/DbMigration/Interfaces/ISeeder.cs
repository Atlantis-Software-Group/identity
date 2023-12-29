namespace asg.data.migrator.DbMigration.Interfaces;

public interface ISeeder
{
    Task SeedAsync(Type seedDataType);
}
