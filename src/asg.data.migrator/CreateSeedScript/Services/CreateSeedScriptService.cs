using System.Text;
using asg.data.migrator.Constants;
using asg.data.migrator.CreateSeedScript.Interfaces;
using asg.data.migrator.SeedData.Attributes;
using asg.data.migrator.Services;
using Microsoft.Extensions.Logging;

namespace asg.data.migrator.CreateSeedScript.Services;

[MigrationName(migrationName: "User")]
[SeedEnvironment(environmentName: "Development")]
[SeedEnvironment(environmentName: "Integration")]
public class CreateSeedScriptService : ICreateSeedScriptService
{
    public CreateSeedScriptService(ILogger<CreateSeedScriptService> logger, IFileProviderService fileProviderService)
    {
        Logger = logger;
        FileProviderService = fileProviderService;
        scriptContent = new StringBuilder();
    }

    public async Task<string> CreateSeedScriptFile(string path, string scriptName, string migrationName, string DbContextName, params string[] environmentnames)
    {
        string namespaceString = $"asg.Data.Migrator.SeedData.Databases.{DbContextName}";
        scriptContent.AppendLine("using Microsoft.Extensions.Configuration;");        
        scriptContent.AppendLine("using Microsoft.Extensions.Logging;");
        scriptContent.AppendLine("using asg.data.migrator.Services;");
        scriptContent.AppendLine("using asg.data.migrator.SeedData.Attributes;");
        scriptContent.AppendLine();
        scriptContent.AppendLine($"namespace {namespaceString};");
        scriptContent.AppendLine();
        scriptContent.AppendLine(string.Format(MigrationNameTemplate, migrationName));
        foreach (string env in environmentnames)
        {
            scriptContent.AppendLine(string.Format(EnvironmentTemplate, env));
        }
        scriptContent.AppendLine(string.Format(DatabaseNameTemplate, DbContextName));
        scriptContent.AppendLine(GetClassTemplate(scriptName));
        byte[] contentBytes = Encoding.UTF8.GetBytes(scriptContent.ToString());

        // Check if class already exists
        bool classAlreadyExists = ClassAlreadyExists(scriptName, namespaceString);
        bool scriptCreated = false;

        if ( !classAlreadyExists )
            scriptCreated = await FileProviderService.CreateFile(path, contentBytes);
        else
            ErrorMessage = ErrorMessageConstants.DuplicateClass;

        if ( !scriptCreated && !classAlreadyExists )
            ErrorMessage = FileProviderService.ErrorMessage;

        return scriptCreated ? scriptContent.ToString() : string.Empty;
    }

    private bool ClassAlreadyExists(string scriptName, string namepsaceString)
    {
        Type? type = AppDomain.CurrentDomain.GetAssemblies()
                                    .SelectMany(da => da.GetTypes())
                                    .Where(type => type.IsSubclassOf(typeof(SeedDataService)) 
                                            && !type.IsAbstract 
                                            && type.Namespace == namepsaceString
                                            && type.Name == scriptName)
                                    .FirstOrDefault();

        return type is not null;
    }

    private string GetClassTemplate(string scriptName)
    {
        StringBuilder updatetdScriptName = new StringBuilder();
        if ( char.IsLower(scriptName[0]) )
        {
            updatetdScriptName.Append(char.ToUpper(scriptName[0]));
            for ( int i = 1; i < scriptName.Length; i++ )
                updatetdScriptName.Append(scriptName[i]);
        }
        else
            updatetdScriptName.Append(scriptName);        

        StringBuilder classTemplate = new StringBuilder();

        classTemplate.AppendLine(string.Format("public class {0} : SeedDataService", updatetdScriptName.ToString()));
        classTemplate.AppendLine("{");
        classTemplate.AppendLine(string.Format("    public {0}(IConfiguration configuration, ILogger<{0}> logger) : base (configuration, logger) {{}}", updatetdScriptName.ToString()));
        classTemplate.AppendLine();
        classTemplate.AppendLine("  public override Task<bool> Seed()");
        classTemplate.AppendLine("  {");
        classTemplate.AppendLine("      //Seed data");
        classTemplate.AppendLine("      throw new NotImplementedException();");
        classTemplate.AppendLine("  }");
        classTemplate.AppendLine("}");

        return classTemplate.ToString();
    }

    public string? ErrorMessage { get; set; }
    public ILogger<CreateSeedScriptService> Logger { get; }
    public IFileProviderService FileProviderService { get; }

    StringBuilder scriptContent;

    public const string MigrationNameTemplate = "[MigrationName(\"{0}\")]";
    public const string EnvironmentTemplate = "[SeedEnvironment(\"{0}\")]";
    public const string DatabaseNameTemplate = "[DatabaseName(\"{0}Context\")]";
}

