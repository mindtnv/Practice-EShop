using Basket.API.Controllers;
using Basket.API.Infrastructure.Repositories;
using Basket.API.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Basket.UnitTests;

[TestFixture]
public class BasketControllerTests
{
    [SetUp]
    public void SetUp()
    {
        _customerId = "customer-id";

        var repositoryMock = new Mock<IBasketRepository>();
        repositoryMock.Setup(x => x.GetBasketAsync(It.IsAny<string>()))
                      .ReturnsAsync((string customerId) => GetCustomerBasket(customerId));
        repositoryMock.Setup(x => x.DeleteBasketAsync(It.IsAny<string>()))
                      .ReturnsAsync((string x) => x == _customerId);
        repositoryMock.Setup(x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>()))
                      .ReturnsAsync((CustomerBasket x) => x.CustomerId == _customerId);

        _repository = repositoryMock.Object;
        _controller = new BasketController(_repository);
    }

    private string _customerId = null!;
    private IBasketRepository _repository = null!;
    private BasketController _controller = null!;

    private CustomerBasket GetCustomerBasket(string customerId) => new()
    {
        CustomerId = customerId,
        Items = new List<CustomerBasketItem>
        {
            new()
            {
                ProductId = 0,
                Quantity = 0,
                UnitPrice = 0,
                OldUnitPrice = 0,
                PictureUrl = "",
            },
        },
    };

    [Test]
    public async Task DeleteCustomerBasketByIdAsync()
    {
        var actionResult = await _controller.DeleteCustomerBasketAsync(_customerId);
        actionResult.Should().BeOfType<OkResult>();
        var actionResult2 = await _controller.DeleteCustomerBasketAsync("notfound-id");
        actionResult2.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetCustomerBasketByIdAsync()
    {
        var basket = await _repository.GetBasketAsync(_customerId);
        var actionResult = await _controller.GetCustomerBasketByIdAsync(_customerId);
        actionResult.Should().BeOfType<ActionResult<CustomerBasket>>();
        actionResult.Value.Should().NotBeNull();
        actionResult.Value!.Should().BeEquivalentTo(basket);
    }

    [Test]
    public async Task UpdateCustomerBasketByIdAsync()
    {
        var actionResult = await _controller.UpdateCustomerBasketAsync(new CustomerBasket
        {
            CustomerId = _customerId,
        });
        actionResult.Should().BeOfType<OkResult>();
        var actionResult2 = await _controller.UpdateCustomerBasketAsync(new CustomerBasket
        {
            CustomerId = "notfound-id",
        });
        actionResult2.Should().BeOfType<BadRequestResult>();
    }
}