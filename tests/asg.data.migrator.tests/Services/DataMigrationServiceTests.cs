using asg.data.migrator.Constants;
using asg.data.migrator.CreateSeedScript.Interfaces;
using asg.data.migrator.DbMigration.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace asg.data.migrator.tests.Services;

public class DataMigrationServiceTests
{
    private ILogger<DatabaseMigrationService> mockLogger {get;}
    private IHostApplicationLifetime mockHostApplicationLifeTime { get; }
    private IServiceScopeFactory mockServiceScopeFactory { get; }
    private IConfiguration mockConfiguration { get; }
    private IHostEnvironment mockHostEnvironment { get; }
    private ICreateSeedScriptService mockCreateSeedScriptService { get; }
    private ICommandLineArgs mockCommandLineArgs { get; }
    private IUpdateDatabaseService mockUpdateDatabaseService { get; }
    private DatabaseMigrationService Service { get; }


    public DataMigrationServiceTests(ITestOutputHelper testOutputHelper)
    {
        mockLogger = new UnitTestLogger<DatabaseMigrationService>(testOutputHelper);
        mockHostApplicationLifeTime = Substitute.For<IHostApplicationLifetime>();
        mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        mockConfiguration = Substitute.For<IConfiguration>();
        mockHostEnvironment = Substitute.For<IHostEnvironment>();
        mockCreateSeedScriptService = Substitute.For<ICreateSeedScriptService>();
        mockCommandLineArgs = Substitute.For<ICommandLineArgs>();
        mockUpdateDatabaseService = Substitute.For<IUpdateDatabaseService>();

        Service = new DatabaseMigrationService(mockLogger, 
                                                mockHostApplicationLifeTime, 
                                                mockServiceScopeFactory, 
                                                mockConfiguration, 
                                                mockHostEnvironment, 
                                                mockCreateSeedScriptService,
                                                mockCommandLineArgs,
                                                mockUpdateDatabaseService);
    }

    [Fact]
    public async Task CreateSeedScript_False_ScriptNameNotProvided()
    {        
        // Act
        bool result = await Service.CreateSeedScript();

        // Assert
        Assert.False(result);
        Assert.Equal(ErrorMessageConstants.ScriptNameNotProvided, Service.ErrorMessage);
    }

    [Fact]
    public async Task CreateSeedScript_False_DbContextNameNotProvided()
    {   
        mockCommandLineArgs.GetValue<string>("-scriptName").Returns("random");
        
        // Act
        bool result = await Service.CreateSeedScript();

        // Assert
        Assert.False(result);
        Assert.Equal(ErrorMessageConstants.DbContextNameNotProvided, Service.ErrorMessage);
    }

    [Fact]
    public async Task CreateSeedScript_False_MigrationNameNotProvided()
    {
        mockCommandLineArgs.GetValue<string>("-scriptName").Returns("random");
        mockCommandLineArgs.GetValue<string>("-dbContextName").Returns("db");
        mockHostEnvironment.ContentRootPath.Returns("C:\\Testing");

        //Act
        bool result = await Service.CreateSeedScript();

        Assert.False(result);
        Assert.Equal(ErrorMessageConstants.MigrationNameNotProvided, Service.ErrorMessage);
    }

    [Fact]
    public async Task CreateSeedScript_False_FileI_O_Error()
    {
        mockCommandLineArgs.GetValue<string>("-scriptName").Returns("random");
        mockCommandLineArgs.GetValue<string>("-dbContextName").Returns("db");
        mockCommandLineArgs.GetValue<string>("-migrationName").Returns("Users");
        mockCommandLineArgs.GetCollectionValue<string>("-environmentNames").Returns(new List<string>{"Development","Integration"});
        mockHostEnvironment.ContentRootPath.Returns("C:\\Testing");

        string[] capturedEnvironmentNames = new string[0];

        mockCreateSeedScriptService.CreateSeedScriptFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string[]>(x => capturedEnvironmentNames = x))
                                            .Returns(string.Empty);
        
        mockCreateSeedScriptService.ErrorMessage.Returns(ErrorMessageConstants.FileIOError);
        //Act
        bool result = await Service.CreateSeedScript();

        Assert.False(result);
        Assert.Equal(ErrorMessageConstants.FileIOError, Service.ErrorMessage);
    }

    [Fact]
    public async Task CreateSeedScript_True_SeedScriptCreated_OneEnvironment()
    {
        mockCommandLineArgs.GetValue<string>("-scriptName").Returns("random");
        mockCommandLineArgs.GetValue<string>("-dbContextName").Returns("db");
        mockCommandLineArgs.GetValue<string>("-migrationName").Returns("Users");
        mockCommandLineArgs.GetCollectionValue<string>("-environmentNames").Returns(new List<string>{"Development"});
        mockHostEnvironment.ContentRootPath.Returns("C:\\Testing");

        string[] capturedEnvironmentNames = new string[0];

        mockCreateSeedScriptService.CreateSeedScriptFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string[]>(x => capturedEnvironmentNames = x))
                                            .Returns("Hi");
        //Act
        bool result = await Service.CreateSeedScript();

        Assert.True(result);
        Assert.Collection(capturedEnvironmentNames, 
            env => Assert.Equal("Development", env));
    }

    [Fact]
    public async Task CreateSeedScript_True_SeedScriptCreated_MultipleEnvironment()
    {
        mockCommandLineArgs.GetValue<string>("-scriptName").Returns("random");
        mockCommandLineArgs.GetValue<string>("-dbContextName").Returns("db");
        mockCommandLineArgs.GetValue<string>("-migrationName").Returns("Users");
        mockCommandLineArgs.GetCollectionValue<string>("-environmentNames").Returns(new List<string>{"Development","Integration"});
        mockHostEnvironment.ContentRootPath.Returns("C:\\Testing");

        string[] capturedEnvironmentNames = new string[0];

        mockCreateSeedScriptService.CreateSeedScriptFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string[]>(x => capturedEnvironmentNames = x))
                                            .Returns("Hello there");
        //Act
        bool result = await Service.CreateSeedScript();

        Assert.True(result);
        Assert.Collection(capturedEnvironmentNames, 
            env => Assert.Equal("Development", env), 
            env => Assert.Equal("Integration", env));
    }
}
