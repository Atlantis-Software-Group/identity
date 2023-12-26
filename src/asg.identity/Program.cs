using asg.identity;
using Serilog;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, lc) => {
            IConfiguration config = ctx.Configuration;
            string seqUrl = config["Seq:ServerUrl"] ?? string.Empty;

            lc
                .MinimumLevel.Debug()
                .Enrich.WithProperty("Application", "ASG - Identity")
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .WriteTo.Seq(seqUrl)
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);        
    });
        // lc
        // .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        // .WriteTo.Seq("https://seq:45341")
        // .Enrich.FromLogContext()
        // .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    // this seeding is only for the template to bootstrap the DB and users.
    // in production you will likely want a different approach.
    if (args.Contains("/seed"))
    {
        Log.Information("Seeding database...");
        SeedData.EnsureSeedData(app);
        Log.Information("Done seeding database. Exiting.");
        return;
    }

    app.Run();
}
catch (Exception ex) when (
                            // https://github.com/dotnet/runtime/issues/60600
                            ex.GetType().Name is not "StopTheHostException"
                            // HostAbortedException was added in .NET 7, but since we target .NET 6 we
                            // need to do it this way until we target .NET 8
                            && ex.GetType().Name is not "HostAbortedException"
                        )
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}