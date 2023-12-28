namespace asg.data.migrator.CreateSeedScript.Interfaces;

public interface ICreateSeedScriptService
{
    string? ErrorMessage { get; set; }
    Task<string> CreateSeedScriptFile(string path, string scriptName, string migrationName, string dbContextName, params string[] environmentnames);
}
