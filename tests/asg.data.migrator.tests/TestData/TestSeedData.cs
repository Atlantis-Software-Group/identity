using asg.data.migrator.SeedData.Attributes;
using asg.data.migrator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace asg.data.migrator.tests;

[MigrationName("Test")]
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
