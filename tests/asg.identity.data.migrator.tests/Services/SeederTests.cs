
using asg.data.migrator.tests.TestData;
using asg.dbmigrator.DbMigration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace asg.data.migrator.tests.Services;

public class SeederTests
{
    private ILogger<Seeder> mockLogger;
    private IServiceProvider ServiceProvider;
    private IConfiguration mockConfiguration;
    private Seeder Service { get; }
    public SeederTests(ITestOutputHelper testOutputHelper)
    {
        mockLogger = new UnitTestLogger<Seeder>(testOutputHelper);
        mockConfiguration = Substitute.For<IConfiguration>();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(mockConfiguration);
        services.AddSingleton<ILogger<TestSeedData>>(new UnitTestLogger<TestSeedData>(testOutputHelper));
        ServiceProvider = services.BuildServiceProvider();

        Service = new Seeder(mockLogger, ServiceProvider);
    }

    [Fact]
    public async Task Seeder_SeedDataRunSuccessfully()
    {
        await Service.SeedAsync(typeof(TestSeedData));

        Assert.True(true);
    }
}
