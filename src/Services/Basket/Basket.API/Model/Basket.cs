namespace Basket.API.Model;

public class Basket
{
    public string CustomerId { get; set; }
    public List<BasketItem> Items { get; set; } = new();
}