using asg.data.migrator;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

[MigrationName("Configuration")]
[SeedEnvironment("Development")]
[SeedEnvironment("Local")]
public class Configuration : SeedData
{
    public Configuration(IConfiguration configuration, 
                          ILogger<Configuration> logger, 
                          ConfigurationDbContext configurationDb) 
                          : base (configuration, logger)
    {
        ConfigurationDb = configurationDb;
    }

    public ConfigurationDbContext ConfigurationDb { get; }

    public override async Task<bool> Seed()
  {
      //Seed data
      Client webClient = new() {
        ClientId = "web-client",
        ClientSecrets = { new Secret($"{Configuration["WebClient:Secret"]}".Sha256()) },
        AllowedGrantTypes = GrantTypes.Code,
        RedirectUris = { "https://localhost:7112/signin-oidc" },
        FrontChannelLogoutUri = "https://localhost:7112/signout-oidc",
        PostLogoutRedirectUris = { "https://localhost:7112/signout-callback-oidc" },
        AllowOfflineAccess = true,
        AllowedScopes = { "openid", "email", "user", "profile", "simple-budget-api", "offline_access" }
      };

      await ConfigurationDb.Clients.AddAsync(webClient.ToEntity()).ConfigureAwait(false);

      await ConfigurationDb.SaveChangesAsync();
      throw new NotImplementedException();
  }
}

