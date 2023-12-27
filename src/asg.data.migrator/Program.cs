using System.Text;
using asg.data.DbContexts;
using asg.data.migrator;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                        .WriteTo.Console()
                        .CreateBootstrapLogger();

        Log.Information("Args: {@args}", args);

        // Serilog.Debugging.SelfLog.Enable(Console.Error);
        try
        {
            Log.Information("Configuring Host");
            IHostBuilder builder = Host.CreateDefaultBuilder();

            string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Local";
            Log.Information("Environment: {environment}", environment);

            StringBuilder workingDirPath = new StringBuilder();
            if (Environment.CurrentDirectory.EndsWith("asg.data.migrator"))
            {
                workingDirPath.Append(Environment.CurrentDirectory);
            }
            else
            {
                workingDirPath.Append(Path.Combine(Environment.CurrentDirectory, "asg.data.migrator"));
            }

            Log.Information("Current Working Directory: {directory}", workingDirPath);
            builder.UseContentRoot(workingDirPath.ToString());

            if ( args.Contains("-ef") && string.Equals(environment, "Local", StringComparison.OrdinalIgnoreCase))
                builder.UseEnvironment("Development");

            builder.ConfigureServices((ctx, services) =>
            {
                services.AddHostedService<DatabaseMigrationService>();

                var connectionString = ctx.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
                });

                services.AddConfigurationDbContext(opts =>
                {
                    opts.ConfigureDbContext = options => {
                        options.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));                        
                    };
                });

                services.AddOperationalDbContext(opts =>
                {
                    opts.ConfigureDbContext = options => {
                        options.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));                        
                    };
                });
            });

            builder.UseSerilog((ctx, services, configuration) =>
            {
                IConfiguration config = ctx.Configuration;
                string seqUrl = config["Seq:ServerUrl"] ?? string.Empty;
                Log.Information("Seq URL: {url}", seqUrl);
                configuration
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Debug)
                    .Enrich.WithProperty("Application", "Identity.DatabaseMigrator")
                    .Enrich.WithExceptionDetails()
                    .Enrich.FromLogContext()
                    .WriteTo.Seq(seqUrl)
                    .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
            });

            IHost host = builder.Build();

            if (args.Contains("-migrate"))
            {
                Log.Information("Starting Host");                
                await host.StartAsync();

                await host.WaitForShutdownAsync();
                Log.Information("Host shutdown successfully.");
            }
            else
            {
                Log.Information("Migration not requested");
            }
        }
        catch (Exception e)
        {
            if ( !args.Contains("-ef") )
            {
                Log.Fatal(e, "Host Terminatted unexpectedly");
                return 1;
            }
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return 0;
    }
}