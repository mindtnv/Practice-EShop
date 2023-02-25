using System.Text.Json;
using Basket.API.Model;
using StackExchange.Redis;

namespace Basket.API.Infrastructure.Repositories;

public class RedisBasketRepository : IBasketRepository
{
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _redis;

    public RedisBasketRepository(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = _redis.GetDatabase();
    }

    public async IAsyncEnumerable<CustomerBasket> GetBasketsAsync()
    {
        var keys = _redis.GetServers().First().KeysAsync(pattern: "basket*");
        await foreach (var key in keys)
        {
            var bytes = (byte[]) (await _database.StringGetAsync(key))! ?? throw new InvalidOperationException();
            using var stream = new MemoryStream(bytes);
            var basket = await JsonSerializer.DeserializeAsync<CustomerBasket>(stream);
            if (basket != null)
                yield return basket;
        }
    }

    public async Task<bool> UpdateBasketAsync(CustomerBasket customerBasket)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, customerBasket);
        var key = $"basket:{customerBasket.CustomerId}";
        var isSuccess = await _database.StringSetAsync(key, RedisValue.CreateFrom(stream));
        return isSuccess;
    }

    public async Task<CustomerBasket?> GetBasketAsync(string customerId)
    {
        var redisValue = await _database.StringGetAsync(GetBasketKey(customerId));
        if (redisValue.IsNull)
            return null;

        var bytes = (byte[]) redisValue!;
        using var stream = new MemoryStream(bytes);
        var basket = await JsonSerializer.DeserializeAsync<CustomerBasket>(stream);
        return basket;
    }

    public async Task<bool> DeleteBasketAsync(string customerId) =>
        await _database.KeyDeleteAsync(GetBasketKey(customerId));

    private string GetBasketKey(string customerId) => $"basket:{customerId}";
}