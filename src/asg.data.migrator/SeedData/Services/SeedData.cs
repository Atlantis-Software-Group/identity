using Microsoft.Extensions.Configuration;

namespace asg.data.migrator;

public abstract class SeedData : ISeedData
{
    public SeedData(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public abstract Task<bool> Seed();
}
