using asg.dbmigrator.SeedData.Attributes;
using asg.dbmigrator.SeedData.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace asg.data.migrator.tests.TestData;

[MigrationName("Test")]
[SeedEnvironment("Development")]
[SeedEnvironment("Integration")]
[DatabaseName("ApplicationDbContext")]
public class TestSeedData : SeedDataService
{
    public TestSeedData(IConfiguration configuration, ILogger<TestSeedData> logger) 
        : base(configuration, logger)
    {
    }

    public override Task<bool> Seed()
    {
        return Task.FromResult(true);
    }
}
