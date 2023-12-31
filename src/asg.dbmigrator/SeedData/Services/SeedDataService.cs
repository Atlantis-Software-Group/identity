using asg.dbmigrator.SeedData.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace asg.dbmigrator.SeedData.Services;

public abstract class SeedDataService : ISeedData
{
    public SeedDataService(IConfiguration configuration, ILogger logger)
    {
        Configuration = configuration;
        Logger = logger;
    }

    public IConfiguration Configuration { get; }
    public ILogger Logger { get; }

    public abstract Task<bool> Seed();
}
