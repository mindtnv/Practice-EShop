using Microsoft.EntityFrameworkCore;

namespace Catalog.API;

public class MigrationsHostedService<TContext> : IHostedService where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationsHostedService<TContext>> _logger;

    public MigrationsHostedService(IServiceProvider serviceProvider, ILogger<MigrationsHostedService<TContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Applying migrations to {Context}", typeof(TContext));
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync(cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}