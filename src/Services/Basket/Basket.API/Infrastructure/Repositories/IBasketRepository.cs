using Basket.API.Model;

namespace Basket.API.Infrastructure.Repositories;

public interface IBasketRepository
{
    IAsyncEnumerable<CustomerBasket> GetBasketsAsync();
    Task<CustomerBasket?> GetBasketAsync(string customerId);
    Task<bool> UpdateBasketAsync(CustomerBasket customerBasket);
    Task<bool> DeleteBasketAsync(string customerId);
}