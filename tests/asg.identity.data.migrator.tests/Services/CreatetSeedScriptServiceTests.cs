using asg.dbmigrator.Constants;
using asg.dbmigrator.CreateSeedTemplate.Interfaces;
using asg.dbmigrator.CreateSeedTemplate.Services;
using asg.dbmigrator.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace asg.data.migrator.tests.Services;

public class CreateSeedScriptServiceTests
{
    private ILogger<CreateSeedTemplateService> mockLogger { get; }
    private IFileProviderService mockFileProviderService { get; }
    private IAssemblyInformationService mockAssemblyInformationService { get; }

    private CreateSeedTemplateService Service { get; }
    public CreateSeedScriptServiceTests(ITestOutputHelper testOutputHelper)
    {
        mockLogger = new UnitTestLogger<CreateSeedTemplateService>(testOutputHelper);
        mockFileProviderService = Substitute.For<IFileProviderService>();
        mockAssemblyInformationService = Substitute.For<IAssemblyInformationService>();

        Service = new CreateSeedTemplateService(mockLogger, mockFileProviderService, mockAssemblyInformationService);
    }

    [Fact]
    public async Task CreateSeedTemplateFile_True_OnlyMigrationName()
    {
        mockFileProviderService.CreateFile(Arg.Any<string>(), Arg.Any<byte[]>())
                                .Returns(true);

        string result = await Service.CreateSeedTemplateFile(string.Empty, "random", "Users", "HelloDb", new string[0]);

        Assert.Equal(@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using asg.data.migrator.Services;
using asg.data.migrator.SeedData.Attributes;

namespace asg.Data.Migrator.SeedData.Databases.HelloDb;

[MigrationName(""Users"")]
[DatabaseName(""HelloDbContext"")]
public class Random : SeedDataService
{
    public Random(IConfiguration configuration, ILogger<Random> logger) : base (configuration, logger) {}

  public override Task<bool> Seed()
  {
      //Seed data
      throw new NotImplementedException();
  }
}

", result);
    }

    [Fact]
    public async Task CreateSeedTemplateFile_True_OneEnvironmentName()
    {
        mockFileProviderService.CreateFile(Arg.Any<string>(), Arg.Any<byte[]>())
                                .Returns(true);
        string result = await Service.CreateSeedTemplateFile(string.Empty, "random", "Users", "HelloDb", new string[] { "Development" });

        Assert.Equal(@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using asg.data.migrator.Services;
using asg.data.migrator.SeedData.Attributes;

namespace asg.Data.Migrator.SeedData.Databases.HelloDb;

[MigrationName(""Users"")]
[SeedEnvironment(""Development"")]
[DatabaseName(""HelloDbContext"")]
public class Random : SeedDataService
{
    public Random(IConfiguration configuration, ILogger<Random> logger) : base (configuration, logger) {}

  public override Task<bool> Seed()
  {
      //Seed data
      throw new NotImplementedException();
  }
}

", result);
    }

    [Fact]
    public async Task CreateSeedTemplateFile_True_MultipleEnvironmentName()
    {
        mockFileProviderService.CreateFile(Arg.Any<string>(), Arg.Any<byte[]>())
                                .Returns(true);
        string result = await Service.CreateSeedTemplateFile(string.Empty, "random", "Users", "HelloDb", new string[] { "Development", "Integration" });

        Assert.Equal(@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using asg.data.migrator.Services;
using asg.data.migrator.SeedData.Attributes;

namespace asg.Data.Migrator.SeedData.Databases.HelloDb;

[MigrationName(""Users"")]
[SeedEnvironment(""Development"")]
[SeedEnvironment(""Integration"")]
[DatabaseName(""HelloDbContext"")]
public class Random : SeedDataService
{
    public Random(IConfiguration configuration, ILogger<Random> logger) : base (configuration, logger) {}

  public override Task<bool> Seed()
  {
      //Seed data
      throw new NotImplementedException();
  }
}

", result);
    }

    [Fact]
    public async Task CreateSeedTemplateFile_False_FileAlreadyExists()
    {
        mockFileProviderService.CreateFile(Arg.Any<string>(), Arg.Any<byte[]>())
                                .Returns(false);
        
        mockFileProviderService.ErrorMessage.Returns(ErrorMessageConstants.FileAlreadyExists);
        string result = await Service.CreateSeedTemplateFile(string.Empty, "random", "Users", "HelloDb", new string[] { "Development", "Integration" });

        bool HasContent = !string.IsNullOrWhiteSpace(result);
        Assert.False(HasContent);
        Assert.Equal(ErrorMessageConstants.FileAlreadyExists, Service.ErrorMessage);
    }
}
