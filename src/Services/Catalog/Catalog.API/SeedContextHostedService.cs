using Catalog.API.Infrastructure;

namespace Catalog.API;

public class SeedContextHostedService : IHostedService
{
    private readonly CatalogContextSeed _catalogContextSeed;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public SeedContextHostedService(IConfiguration configuration, CatalogContextSeed catalogContextSeed,
        IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _catalogContextSeed = catalogContextSeed;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var needSeed = _configuration.GetValue("SeedDbContext", false);
        if (needSeed)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
            await _catalogContextSeed.SeedAsync(context);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}