using Basket.API.Extensions;
using MassTransit;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Host.UseSerilog(ConfigureSerilogLogger(builder.Configuration));
builder.Services
       .AddConfiguredSwagger()
       .AddConfiguredControllers()
       .AddConfiguredRedisBasketRepository(builder.Configuration)
       .AddConfiguredMassTransit(builder.Configuration);

var app = builder.Build();
app.UseRouting();
app.UseSwagger().UseSwaggerUI();
app.MapControllers();

app.MapGet("/", () => Results.LocalRedirect("/swagger", true));

app.Run();

ILogger ConfigureSerilogLogger(IConfiguration configuration)
{
    var seqHost = configuration.GetValue(ConfigurationPath.Combine("Seq", "Host"), "localhost") ??
                  throw new ConfigurationException("You must configure Seq host");
    return new LoggerConfiguration()
           .MinimumLevel.Verbose()
           .Enrich.WithProperty("ApplicationContext", "Basket.API")
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.Seq(seqHost, LogEventLevel.Information)
           .CreateLogger();
}