using asg.data.migrator.Constants;
using asg.data.migrator.CreateSeedScript.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace asg.data.migrator.tests;

public class CreatetSeedScriptServiceTests
{
    private ILogger<CreateSeedScriptService> mockLogger { get; }
    private IFileProviderService mockFileProviderService { get; }

    private CreateSeedScriptService Service { get; }
    public CreatetSeedScriptServiceTests(ITestOutputHelper testOutputHelper)
    {
        mockLogger = new UnitTestLogger<CreateSeedScriptService>(testOutputHelper);
        mockFileProviderService = Substitute.For<IFileProviderService>();

        Service = new CreateSeedScriptService(mockLogger, mockFileProviderService);
    }

    [Fact]
    public async Task CreateSeedScriptFile_True_OnlyMigrationName()
    {
        mockFileProviderService.CreateFile(Arg.Any<string>(), Arg.Any<byte[]>())
                                .Returns(true);
        string result = await Service.CreateSeedScriptFile(string.Empty, "random", "Users", "HelloDb", new string[0]);

        Assert.Equal(@"[MigrationName(""Users"")]
public class random : SeedData
{
    public random(IConfiguration configuration) : base (configuration) {}

  public override Task<bool> Seed()
  {
      //Seed data
  }
}

", result);
    }

    [Fact]
    public async Task CreateSeedScriptFile_True_OneEnvironmentName()
    {
        mockFileProviderService.CreateFile(Arg.Any<string>(), Arg.Any<byte[]>())
                                .Returns(true);
        string result = await Service.CreateSeedScriptFile(string.Empty, "random", "Users", "HelloDb", new string[] { "Development" });

        Assert.Equal(@"[MigrationName(""Users"")]
[SeedEnvironment(""Development"")]
public class random : SeedData
{
    public random(IConfiguration configuration) : base (configuration) {}

  public override Task<bool> Seed()
  {
      //Seed data
  }
}

", result);
    }

    [Fact]
    public async Task CreateSeedScriptFile_True_MultipleEnvironmentName()
    {
        mockFileProviderService.CreateFile(Arg.Any<string>(), Arg.Any<byte[]>())
                                .Returns(true);
        string result = await Service.CreateSeedScriptFile(string.Empty, "random", "Users", "HelloDb", new string[] { "Development", "Integration" });

        Assert.Equal(@"[MigrationName(""Users"")]
[SeedEnvironment(""Development"")]
[SeedEnvironment(""Integration"")]
public class random : SeedData
{
    public random(IConfiguration configuration) : base (configuration) {}

  public override Task<bool> Seed()
  {
      //Seed data
  }
}

", result);
    }

    [Fact]
    public async Task CreateSeedScriptFile_False_FileAlreadyExists()
    {
        mockFileProviderService.CreateFile(Arg.Any<string>(), Arg.Any<byte[]>())
                                .Returns(false);
        
        mockFileProviderService.ErrorMessage.Returns(ErrorMessageConstants.FileAlreadyExists);
        string result = await Service.CreateSeedScriptFile(string.Empty, "random", "Users", "HelloDb", new string[] { "Development", "Integration" });

        bool HasContent = !string.IsNullOrWhiteSpace(result);
        Assert.False(HasContent);
        Assert.Equal(ErrorMessageConstants.FileAlreadyExists, Service.ErrorMessage);
    }
}
