using asg.data.DbContexts;
using asg.data.migrator.tests.TestData;
using asg.dbmigrator.Shared.Services;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace asg.data.migrator.tests.Services;

public class AssemblyInformationServiceTests
{
    ILogger<AssemblyInformationService> Logger { get; }

    AssemblyInformationService Service { get; }

    public AssemblyInformationServiceTests(ITestOutputHelper testOutputHelper)
    {
        Logger = new UnitTestLogger<AssemblyInformationService>(testOutputHelper);
        Service = new AssemblyInformationService(Logger);
    }

    [Fact]
    public void AssemblyInformationService_Find_MigrationAttribute()
    {
        IEnumerable<Type> types = Service.GetSeedScripts(typeof(ApplicationDbContext).Name, "Test", "Development");

        Assert.Collection(types, 
            type => Assert.Equal(typeof(TestSeedData), type)
        );
    }

    [Fact]
    public void AssemblyInformationService_Find_DatabaseAttribute()
    {
        IEnumerable<Type> types = Service.GetSeedScripts(typeof(ApplicationDbContext).Name, "Test", "Development");

        Assert.Collection(types, 
            type => Assert.Equal(typeof(TestSeedData), type)
        );
    }

    [Fact]
    public void AssemblyInformationService_Find_EnvironmentNamettribute_Integration()
    {
        IEnumerable<Type> types = Service.GetSeedScripts(typeof(ApplicationDbContext).Name, "Test", "Integration");

        Assert.Collection(types, 
            type => Assert.Equal(typeof(TestSeedData), type)
        );
    }

    [Fact]
    public void AssemblyInformationService_Find_EnvironmentNamettribute_Integration_DifferentCase()
    {
        IEnumerable<Type> types = Service.GetSeedScripts(typeof(ApplicationDbContext).Name, "Test", "integration");

        Assert.Collection(types, 
            type => Assert.Equal(typeof(TestSeedData), type)
        );
    }

    [Fact]
    public void AssemblyInformationService_Find_EnvironmentNamettribute_Local_DifferentCase()
    {
        IEnumerable<Type> types = Service.GetSeedScripts(typeof(ApplicationDbContext).Name, "Test", "LOCAL");

        Assert.Collection(types, 
            type => Assert.Equal(typeof(TestSeedDataLocal), type)
        );
    }

    [Fact]
    public void AssemblyInformationService_Find_EnvironmentNamettribute_Local()
    {
        IEnumerable<Type> types = Service.GetSeedScripts(typeof(ApplicationDbContext).Name, "Test", "Local");

        Assert.Collection(types, 
            type => Assert.Equal(typeof(TestSeedDataLocal), type)
        );
    }

    [Fact]
    public void AssemblyInformationService_ClassExists_True()
    {
        bool result = Service.ClassExists("TestSeedData", "asg.data.migrator.tests.TestData");

        Assert.True(result);
    }

    [Fact]
    public void AssemblyInformationService_ClassExists_False()
    {
        bool result = Service.ClassExists("TestSeedDataDoesNotExist", "asg.data.migrator.tests.TestData");

        Assert.False(result);
    }
}
