namespace asg.dbmigrator.Shared.Interfaces;

public interface IAssemblyInformationService
{
    IEnumerable<Type> GetSeedScripts(string dbContext, string migration, string environmentName);
    bool ClassExists(string typeName, string namespaceString);
}
