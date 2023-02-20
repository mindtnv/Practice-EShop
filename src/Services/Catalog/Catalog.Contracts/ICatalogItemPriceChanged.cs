namespace Catalog.Contracts;

public interface ICatalogItemPriceChanged
{
    public int ItemId { get; }
    public decimal NewPrice { get; }
    public decimal OldPrice { get; }
}