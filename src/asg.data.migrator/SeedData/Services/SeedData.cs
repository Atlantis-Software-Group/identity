using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace asg.data.migrator;

public abstract class SeedData : ISeedData
{
    public SeedData(IConfiguration configuration, ILogger logger)
    {
        Configuration = configuration;
        Logger = logger;
    }

    public IConfiguration Configuration { get; }
    public ILogger Logger { get; }

    public abstract Task<bool> Seed();
}
