using Basket.API.Infrastructure.Repositories;
using Basket.API.Model;
using Catalog.Contracts;
using MassTransit;

namespace Basket.API.IntegrationEvents.Consumers;

public class CatalogItemPriceChangedConsumer : IConsumer<ICatalogItemPriceChanged>
{
    private readonly IBasketRepository _basketRepository;

    public CatalogItemPriceChangedConsumer(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    public async Task Consume(ConsumeContext<ICatalogItemPriceChanged> context)
    {
        var basketToUpdate = new List<CustomerBasket>();
        await foreach (var basket in _basketRepository.GetBasketsAsync())
        {
            var basketNeedUpdate = UpdatePriceAsync(basket.Items, context.Message);
            if (basketNeedUpdate)
                basketToUpdate.Add(basket);
        }

        foreach (var basket in basketToUpdate)
            await _basketRepository.UpdateBasketAsync(basket);
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