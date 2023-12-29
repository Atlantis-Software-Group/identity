using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using asg.data.migrator.Services;
using asg.data.migrator.SeedData.Attributes;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.EntityFramework.Mappers;

namespace asg.Data.Migrator.SeedData.Databases.ConfigurationDb;

[MigrationName("20231227022050_Configuration")]
[SeedEnvironment("Development")]
[SeedEnvironment("Local")]
[DatabaseName("ConfigurationDbContext")]
public class Configuration : SeedDataService
{
    public Configuration(IConfiguration configuration,
                          ILogger<Configuration> logger,
                          ConfigurationDbContext context) : base(configuration, logger)
    {
      Context = context;
    }

    public ConfigurationDbContext Context { get; }

    public override async Task<bool> Seed()
    {
        //Seed data
        Client webClient = new Client
        {
          ClientId = "web-client",
          ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

          AllowedGrantTypes = GrantTypes.Code,

          RedirectUris = { "https://localhost:7112/signin-oidc" },
          FrontChannelLogoutUri = "https://localhost:7112/signout-oidc",
          PostLogoutRedirectUris = { "https://localhost:7112/signout-callback-oidc" },

          AllowOfflineAccess = true,
          AllowedScopes = { "openid", "email", "user", "profile", "simple-budget-api", "offline_access" }
        };
        await Context.Clients.AddAsync(webClient.ToEntity());

        await Context.SaveChangesAsync();

        return true;
    }
}

