using Basket.API.Infrastructure.Repositories;
using Basket.API.Model;
using Catalog.Contracts;
using MassTransit;
using LogContext = Serilog.Context.LogContext;

namespace Basket.API.IntegrationEvents.Consumers;

public class CatalogItemPriceChangedConsumer : IConsumer<ICatalogItemPriceChanged>
{
    private readonly IBasketRepository _basketRepository;
    private readonly ILogger<CatalogItemPriceChangedConsumer> _logger;

    public CatalogItemPriceChangedConsumer(IBasketRepository basketRepository,
        ILogger<CatalogItemPriceChangedConsumer> logger)
    {
        _basketRepository = basketRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ICatalogItemPriceChanged> context)
    {
        using (LogContext.PushProperty("IntegrationEventConversationId", $"{context.ConversationId}-Basket.API"))
        {
            _logger.LogInformation(
                "Consume integration event: {IntegrationEventConversationId} - ({@IntegrationEvent})",
                context.ConversationId, context.Message);
            var basketToUpdate = new List<CustomerBasket>();
            await foreach (var basket in _basketRepository.GetBasketsAsync())
            {
                var basketNeedUpdate = UpdatePriceAsync(basket.Items, context.Message);
                if (basketNeedUpdate)
                    basketToUpdate.Add(basket);
            }

            foreach (var basket in basketToUpdate)
            {
                _logger.LogInformation("Updating items in basket for user: {BuyerId} ({@Items})", basket.CustomerId,
                    basket.Items);
                await _basketRepository.UpdateBasketAsync(basket);
            }
        }
    }

    private bool UpdatePriceAsync(IEnumerable<CustomerBasketItem> items, ICatalogItemPriceChanged @event)
    {
        var somethingChanges = false;
        foreach (var item in items)
        {
            if (item.ProductId != @event.ProductId)
                continue;

            item.OldUnitPrice = @event.OldPrice;
            item.UnitPrice = @event.NewPrice;
            somethingChanges = true;
        }

        return somethingChanges;
    }
}