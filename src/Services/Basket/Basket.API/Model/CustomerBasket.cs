namespace Basket.API.Model;

public class CustomerBasket
{
    public string CustomerId { get; set; }
    public List<CustomerBasketItem> Items { get; set; } = new();
}