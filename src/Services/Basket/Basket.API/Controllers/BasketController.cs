using System.Net;
using System.Security.Claims;
using Basket.API.Infrastructure.Repositories;
using Basket.API.Model;
using Basket.API.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _basketRepository;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketRepository basketRepository, ILogger<BasketController> logger)
    {
        _basketRepository = basketRepository;
        _logger = logger;
    }

    [Route("all")]
    [HttpGet]
    [Authorize(Roles = "Administrator")]
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
    [Route("{customerId}")]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    public async Task<ActionResult> UpdateCustomerBasketByIdAsync([FromRoute] string customerId,
        [FromBody] UpdateCustomerBasketViewModel viewModel)
    {
        if (!HttpContext.User.IsInRole("Administrator") &&
            !HttpContext.User.HasClaim(ClaimTypes.NameIdentifier, customerId))
            return Forbid();

        _logger.LogInformation("Updating basket for customer {@CustomerId}", customerId);
        var isSuccess = await _basketRepository.UpdateBasketAsync(new CustomerBasket
        {
            CustomerId = customerId,
            Items = viewModel.Items,
        });
        return isSuccess ? Ok() : BadRequest();
    }

    [HttpPost]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    public async Task<ActionResult> UpdateCustomerBasketByClaimsAsync(
        [FromBody] UpdateCustomerBasketViewModel viewModel)
    {
        var customerId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (customerId == null)
            return BadRequest();

        return await UpdateCustomerBasketByIdAsync(customerId, viewModel);
    }

    [HttpGet]
    [Route("{customerId}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket>> GetCustomerBasketByIdAsync([FromRoute] string customerId)
    {
        if (!HttpContext.User.IsInRole("Administrator") &&
            !HttpContext.User.HasClaim(ClaimTypes.NameIdentifier, customerId))
            return Forbid();

        var basket = await _basketRepository.GetBasketAsync(customerId);
        return basket ?? new CustomerBasket
        {
            CustomerId = customerId,
        };
    }

    [HttpGet]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    public async Task<ActionResult<CustomerBasket>> GetCustomerBasketByClaimsAsync()
    {
        var customerId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (customerId == null)
            return BadRequest();

        return await GetCustomerBasketByIdAsync(customerId);
    }

    [HttpDelete]
    [Route("{customerId}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteCustomerBasketByIdAsync([FromRoute] string customerId)
    {
        if (!HttpContext.User.IsInRole("Administrator") &&
            !HttpContext.User.HasClaim(ClaimTypes.NameIdentifier, customerId))
            return Forbid();

        _logger.LogInformation("Deleting basket for customer {@CustomerId}", customerId);
        var isSuccess = await _basketRepository.DeleteBasketAsync(customerId);
        return isSuccess ? Ok() : NotFound();
    }

    [HttpDelete]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    public async Task<ActionResult> DeleteCustomerBasketByClaimsAsync()
    {
        var customerId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (customerId == null)
            return BadRequest();

        return await DeleteCustomerBasketByIdAsync(customerId);
    }
}