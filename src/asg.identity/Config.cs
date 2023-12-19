using Duende.IdentityServer.Models;

namespace asg.identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource("user", 
                                    userClaims: new[] { "id"}
                )
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("simple-budget-api"),
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "web-client",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:7112/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:7112/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:7112/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "email", "user", "profile", "simple-budget-api", "offline_access" }
                },
                new Client
                {
                    ClientId = "backend-for-frontend",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:3101/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:3101/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:3101/signout-callback-oidc" },
                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "email", "user", "profile", "simple-budget-api", "offline_access" }
                },
            };
    }
}
