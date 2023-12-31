namespace asg.data.migrator.CreateSeedTemplate.Interfaces;

public interface ICreateSeedTemplateService
{
    string? ErrorMessage { get; set; }
    Task<string> CreateSeedTemplateFile(string path, string scriptName, string migrationName, string dbContextName, params string[] environmentnames);
}
