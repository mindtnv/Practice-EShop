using System.Net;
using Basket.API.Infrastructure.Repositories;
using Basket.API.Model;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _basketRepository;

    public BasketController(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    [Route("all")]
    [HttpGet]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket[]>> GetCustomerBasketsAsync()
    {
        var baskets = new List<CustomerBasket>();
        await foreach (var basket in _basketRepository.GetBasketsAsync())
            baskets.Add(basket);

        return Ok(baskets);
    }

    [HttpPost]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    public async Task<ActionResult> UpdateCustomerBasketAsync([FromRoute] CustomerBasket basket)
    {
        var isSuccess = await _basketRepository.UpdateBasketAsync(basket);
        return isSuccess ? Ok() : BadRequest();
    }

    [HttpGet]
    [Route("{customerId}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket>> GetCustomerBasketByIdAsync(string customerId)
    {
        var basket = await _basketRepository.GetBasketAsync(customerId);
        return basket ?? new CustomerBasket
        {
            CustomerId = customerId,
        };
    }

    [HttpDelete]
    [Route("{customerId}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteCustomerBasketAsync([FromRoute] string customerId)
    {
        var isSuccess = await _basketRepository.DeleteBasketAsync(customerId);
        return isSuccess ? Ok() : NotFound();
    }
}