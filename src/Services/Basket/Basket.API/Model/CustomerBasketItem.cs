namespace Basket.API.Model;

public class CustomerBasketItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal OldUnitPrice { get; set; }
    public string PictureUrl { get; set; }
}