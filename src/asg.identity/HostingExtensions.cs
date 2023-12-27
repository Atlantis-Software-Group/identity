using asg.data.DbContexts;
using asg.data.Models;
using asg.identity.Pages.Admin.ApiScopes;
using asg.identity.Pages.Admin.Clients;
using asg.identity.Pages.Admin.IdentityScopes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace asg.identity
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddRazorPages();
            builder.Services.AddHealthChecks();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ApplicationDbContext>(options => 
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var isBuilder = builder.Services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    // see https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
                    options.EmitStaticAudienceClaim = false;
                })
                //.AddTestUsers(TestUsers.Users)
                // this adds the config data from DB (clients, resources, CORS)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
                })
                // this is something you will want in production to reduce load on and requests to the DB
                //.AddConfigurationStoreCache()
                //
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<ASGIdentityProfileService>();

            builder.Services.AddAuthentication();

            // this adds the necessary config for the simple admin/config pages
            builder.Services.AddAuthorization(options =>
                options.AddPolicy("admin",
                    policy => policy.RequireClaim("email", "AliceSmith@email.com", "BobSmith@email.com"))
            );

            builder.Services.Configure<RazorPagesOptions>(options =>
                options.Conventions.AuthorizeFolder("/Admin", "admin"));

            builder.Services.AddTransient<asg.identity.Pages.Portal.ClientRepository>();
            builder.Services.AddTransient<ClientRepository>();
            builder.Services.AddTransient<IdentityScopeRepository>();
            builder.Services.AddTransient<ApiScopeRepository>();

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.MapHealthChecks("/health");            
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapRazorPages()
                .RequireAuthorization();

            return app;
        }
    }
}