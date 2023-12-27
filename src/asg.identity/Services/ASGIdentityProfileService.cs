using System.Security.Claims;
using asg.data.Models;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;

namespace asg.identity;

public class ASGIdentityProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ILogger _logger;
    public ASGIdentityProfileService(UserManager<ApplicationUser> users, ILogger<ASGIdentityProfileService> logger)
    {
        _users = users;
        _logger = logger;
    }
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        if (context.RequestedClaimTypes.Any())
        {
            // OPTION 1A: load claims from the user's session cookie
            // AddRequestedClaims will inspect the claims passed and only add the ones 
            // that match the claim types in the RequestedClaimTypes collection.
            context.AddRequestedClaims(context.Subject.Claims);

            // OPTION 1B: load claims from the user database
            // this adds any claims that were requested from the claims in the user store
            var user = await _users.FindByIdAsync(context.Subject.GetSubjectId());
            if (user != null)
            {
                IList<Claim> claims = await _users.GetClaimsAsync(user);
                context.AddRequestedClaims(claims);
            }
        }
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}
