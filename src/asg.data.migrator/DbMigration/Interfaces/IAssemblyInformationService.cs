namespace asg.data.migrator.DbMigration.Interfaces;

public interface IAssemblyInformationService
{
    IEnumerable<Type> GetSeedScripts(string dbContext, string migration);
}
