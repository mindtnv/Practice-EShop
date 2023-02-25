using Catalog.API.Extensions;
using MassTransit;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Host.UseSerilog(ConfigureSerilogLogger(builder.Configuration));
builder.Services
       .CustomConfigure(builder.Configuration)
       .AddConfiguredMvc()
       .AddConfiguredDbContext(builder.Configuration)
       .AddConfiguredSwagger()
       .AddConfiguredMasstransit(builder.Configuration);

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();
app.UseRouting();
app.MapControllers();

app.MapGet("/", () => Results.LocalRedirect("/swagger", true));

app.Run();

ILogger ConfigureSerilogLogger(IConfiguration configuration)
{
    var seqHost = configuration.GetValue(ConfigurationPath.Combine("Seq", "Host"), "localhost") ??
                  throw new ConfigurationException("You must configure Seq host");
    return new LoggerConfiguration()
           .MinimumLevel.Verbose()
           .Enrich.WithProperty("ApplicationContext", "Catalog.API")
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.Seq(seqHost, LogEventLevel.Information)
           .CreateLogger();
}