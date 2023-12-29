using asg.data.DbContexts;
using asg.data.migrator.DbMigration.Services;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace asg.data.migrator.tests;

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
        IEnumerable<Type> types = Service.GetSeedScripts(typeof(ApplicationDbContext).Name, "Test");

        Assert.Collection(types, 
            type => Assert.Equal(typeof(TestSeedData), type)
        );
    }
}
