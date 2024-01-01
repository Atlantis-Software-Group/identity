using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using asg.dbmigrator.SeedData.Services;
using asg.dbmigrator.SeedData.Attributes;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.EntityFramework.Mappers;

namespace asg.Data.Migrator.SeedData.Databases.ConfigurationDb;

[MigrationName("20231227022050_Configuration")]
[SeedEnvironment("Development")]
[SeedEnvironment("Local")]
[DatabaseName("ConfigurationDbContext")]
public class AdditionalConfiguration : SeedDataService
{
    public AdditionalConfiguration(ConfigurationDbContext ctx, 
                                    IConfiguration configuration, 
                                    ILogger<AdditionalConfiguration> logger) 
                                    : base (configuration, logger)
    {
        Context = ctx;
    }

    public ConfigurationDbContext Context { get; }

    public override async Task<bool> Seed()
	{
		//Seed data
		Client bffClient = new Client
		{
			ClientId = "backend-for-frontend",
			ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },
			AllowedGrantTypes = GrantTypes.Code,
			RedirectUris = { "https://localhost:3101/signin-oidc" },
			FrontChannelLogoutUri = "https://localhost:3101/signout-oidc",
			PostLogoutRedirectUris = { "https://localhost:3101/signout-callback-oidc" },
			AllowOfflineAccess = true,
			AllowedScopes = { "openid", "email", "user", "profile", "simple-budget-api", "offline_access" }
		};	

		await Context.Clients.AddAsync(bffClient.ToEntity());
		await Context.SaveChangesAsync();

		IEnumerable<IdentityResource> identityResources = new IdentityResource[]
		{
			new IdentityResources.OpenId(),
			new IdentityResources.Profile(),
			new IdentityResources.Email(),
			new IdentityResource("user", 
								userClaims: new[] { "id"}
			)
		};		

		foreach ( IdentityResource resource in identityResources )
		{
			await Context.IdentityResources.AddAsync(resource.ToEntity());
		}
		
		await Context.SaveChangesAsync();

		ApiScope simpleBudgetApi = new ApiScope("simple-budget-api");
		await Context.ApiScopes.AddAsync(simpleBudgetApi.ToEntity());
		await Context.SaveChangesAsync();

		return true;
	}
}

