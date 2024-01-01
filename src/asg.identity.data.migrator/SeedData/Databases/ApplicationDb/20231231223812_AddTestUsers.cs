using System.Security.Claims;
using asg.dbmigrator.SeedData.Attributes;
using asg.dbmigrator.SeedData.Services;
using asg.identity.data.DbContexts;
using asg.identity.data.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace asg.Data.Migrator.SeedData.Databases.ApplicationDb;

[MigrationName("20231227022054_Users")]
[SeedEnvironment("Development")]
[SeedEnvironment("Local")]
[DatabaseName("ApplicationDbContext")]
public class AddTestUsers : SeedDataService
{
    public AddTestUsers(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AddTestUsers> logger) : base(configuration, logger)
    {
        UserManager = userManager;
    }
    public UserManager<ApplicationUser> UserManager { get; }

    public override async Task<bool> Seed()
    {
        var alice = await UserManager.FindByNameAsync("alice");
        if (alice == null)
        {
            alice = new ApplicationUser
            {
                UserName = "alice",
                Email = "AliceSmith@email.com",
                EmailConfirmed = true,
            };

            var result = await UserManager.CreateAsync(alice, "Pass123$");
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = await UserManager.AddClaimsAsync(alice, new Claim[]{
                new Claim(JwtClaimTypes.Name, "Alice Smith"),
                new Claim(JwtClaimTypes.GivenName, "Alice"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                new Claim("id", Guid.NewGuid().ToString())
            });

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Logger.LogInformation("alice created");
        }

        var bob = UserManager.FindByNameAsync("bob").Result;
        if (bob == null)
        {
            bob = new ApplicationUser
            {
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = true
            };
            var result = await UserManager.CreateAsync(bob, "Pass123$");
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = await UserManager.AddClaimsAsync(bob, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                    new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                    new Claim("id", Guid.NewGuid().ToString())
                    });

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Logger.LogInformation("bob created");
        }

        return true;
    }
}