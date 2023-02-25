namespace Catalog.Contracts;

public interface ICatalogItemPriceChanged
{
    public int ProductId { get; }
    public decimal NewPrice { get; }
    public decimal OldPrice { get; }
}