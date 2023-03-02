using Basket.API.Model;

namespace Basket.API.ViewModel;

public class UpdateCustomerBasketViewModel
{
    public List<CustomerBasketItem> Items { get; set; } = new();
}