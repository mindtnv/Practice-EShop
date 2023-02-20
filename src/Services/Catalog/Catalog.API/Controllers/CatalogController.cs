using System.Net;
using System.Runtime.CompilerServices;
using Catalog.API.Infrastructure;
using Catalog.API.Model;
using Catalog.API.ViewModel;
using Catalog.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Catalog.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class CatalogController : ControllerBase
{
    private readonly CatalogContext _catalogContext;
    private readonly CatalogSettings _catalogSettings;
    private readonly IPublishEndpoint _publishEndpoint;

    public CatalogController(CatalogContext catalogContext, IOptionsSnapshot<CatalogSettings> catalogSettings,
        IPublishEndpoint publishEndpoint)
    {
        _catalogContext = catalogContext;
        _publishEndpoint = publishEndpoint;
        _catalogSettings = catalogSettings.Value;
    }

    [HttpGet]
    [Route("types/{id:int}")]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(CatalogType), (int) HttpStatusCode.OK)]
    // GET api/v1/[controller]/types/{id}
    public async Task<ActionResult<CatalogType>> CatalogTypeByIdAsync([FromQuery] int id)
    {
        if (id < 0)
            return NotFound();

        var type = await _catalogContext.CatalogTypes.SingleOrDefaultAsync(x => x.Id == id);
        if (type == null)
            return NotFound();

        return type;
    }

    [HttpGet]
    [Route("types")]
    [ProducesResponseType(typeof(IEnumerable<CatalogType>), (int) HttpStatusCode.OK)]
    // GET api/v1/[controller]/types 
    public async Task<ActionResult<IEnumerable<CatalogType>>> GetCatalogTypesAsync() =>
        await _catalogContext.CatalogTypes.ToListAsync();

    [HttpPost]
    [Route("types")]
    [ProducesResponseType(typeof(CatalogType), (int) HttpStatusCode.Created)]
    // POST api/v1/[controller]/types 
    public async Task<ActionResult> CreateCatalogTypeAsync([FromBody] CreateCatalogTypeViewModel type)
    {
        var catalogType = new CatalogType
        {
            Type = type.Type,
        };

        _catalogContext.CatalogTypes.Add(catalogType);
        await _catalogContext.SaveChangesAsync();
        return CreatedAtAction("CatalogTypeById", new {catalogType.Id});
    }

    [HttpPut]
    [Route("types")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    // PUT api/v1/[controller]/types 
    public async Task<ActionResult> UpdateCatalogTypeAsync([FromBody] UpdateCatalogTypeViewModel type)
    {
        var catalogType = await _catalogContext.CatalogTypes.SingleOrDefaultAsync(x => x.Id == type.Id);
        if (catalogType == null)
            return NotFound();

        catalogType.Type = type.Type ?? catalogType.Type;
        await _catalogContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete]
    [Route("types/{id:int}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    // DELETE api/v1/[controller]/types/{id}
    public async Task<ActionResult> DeleteCatalogTypeAsync([FromRoute] int id)
    {
        var catalogType = await _catalogContext.CatalogTypes.SingleOrDefaultAsync(x => x.Id == id);
        if (catalogType == null)
            return NotFound();

        _catalogContext.CatalogTypes.Remove(catalogType);
        await _catalogContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Route("brands/{id:int}")]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(CatalogBrand), (int) HttpStatusCode.OK)]
    // GET api/v1/[controller]/brands/{id}
    public async Task<ActionResult<CatalogBrand>> CatalogBrandByIdAsync([FromRoute] int id)
    {
        if (id < 0)
            return BadRequest();

        var brand = await _catalogContext.CatalogBrands.SingleOrDefaultAsync(x => x.Id == id);
        if (brand == null)
            return NotFound();

        return brand;
    }

    [HttpGet]
    [Route("brands")]
    [ProducesResponseType(typeof(IEnumerable<CatalogBrand>), (int) HttpStatusCode.OK)]
    // GET api/v1/[controller]/brands
    public async Task<ActionResult<IEnumerable<CatalogBrand>>> GetCatalogBrandsAsync() =>
        await _catalogContext.CatalogBrands.ToListAsync();

    [HttpPost]
    [Route("brands")]
    [ProducesResponseType((int) HttpStatusCode.Created)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    // POST api/v1/[controller]/brands
    public async Task<ActionResult> CreateCatalogBrandAsync([FromBody] CreateCatalogBrandViewModel brand)
    {
        var catalogBrand = new CatalogBrand
        {
            Brand = brand.Brand,
        };

        _catalogContext.CatalogBrands.Add(catalogBrand);
        await _catalogContext.SaveChangesAsync();
        return CreatedAtAction("CatalogBrandById", new {id = catalogBrand.Id}, null);
    }

    [HttpPut]
    [Route("brands")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    // PUT api/v1/[controller]/brands 
    public async Task<ActionResult> UpdateCatalogBrandAsync([FromBody] UpdateCatalogBrandViewModel brand)
    {
        var catalogBrand = await _catalogContext.CatalogBrands.SingleOrDefaultAsync(x => x.Id == brand.Id);
        if (catalogBrand == null)
            return NotFound();

        catalogBrand.Brand = brand.Brand ?? catalogBrand.Brand;
        await _catalogContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete]
    [Route("brands/{id:int}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    // DELETE api/v1/[controller]/brands/{id}
    public async Task<ActionResult> DeleteCatalogBrandAsync([FromRoute] int id)
    {
        var catalogBrand = await _catalogContext.CatalogBrands.SingleOrDefaultAsync(x => x.Id == id);
        if (catalogBrand == null)
            return NotFound();

        _catalogContext.CatalogBrands.Remove(catalogBrand);
        await _catalogContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Route("items")]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(PaginatedViewModel<CatalogItem>), (int) HttpStatusCode.OK)]
    // GET api/v1/[controller]/items[?pageSize=10[&pageIndex=0]]
    public async Task<ActionResult<PaginatedViewModel<CatalogItem>>> GetItemsWithPaginationAsync(
        [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        if (pageSize < 1 || pageIndex < 0)
            return BadRequest();

        var count = _catalogContext.CatalogItems.LongCount();
        var items = await _catalogContext.CatalogItems
                                         .Include(x => x.CatalogType)
                                         .Include(x => x.CatalogBrand)
                                         .OrderBy(x => x.Id)
                                         .Skip(pageSize * pageIndex)
                                         .Take(pageSize)
                                         .ToListAsync();
        foreach (var item in items)
            FillPictureUri(item);

        var result = new PaginatedViewModel<CatalogItem>(pageIndex, pageSize, count, items);
        return result;
    }

    [HttpGet]
    [Route("items/{id:int}")]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(CatalogItem), (int) HttpStatusCode.OK)]
    // GET api/v1/[controller]/items/{id}
    public async Task<ActionResult<CatalogItem>> CatalogItemByIdAsync([FromRoute] int id)
    {
        if (id < 0)
            return BadRequest();

        var item = await _catalogContext.CatalogItems
                                        .Include(x => x.CatalogType)
                                        .Include(x => x.CatalogBrand)
                                        .SingleOrDefaultAsync(x => x.Id == id);
        if (item == null)
            return NotFound();

        FillPictureUri(item);
        return item;
    }

    [HttpPost]
    [Route("items")]
    [ProducesResponseType((int) HttpStatusCode.Created)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    //POST api/v1/[controller]/items
    public async Task<ActionResult> CreateCatalogItemAsync([FromBody] CreateCatalogItemViewModel product)
    {
        var item = new CatalogItem
        {
            CatalogBrandId = product.CatalogBrandId,
            CatalogTypeId = product.CatalogTypeId,
            Description = product.Description,
            Name = product.Name,
            PictureFileName = product.PictureFileName,
            Price = product.Price,
            AvailableStock = product.AvailableStock,
            OnReorder = product.OnReorder,
        };

        _catalogContext.CatalogItems.Add(item);
        await _catalogContext.SaveChangesAsync();
        return CreatedAtAction("CatalogItemById", new {id = item.Id}, null);
    }

    [HttpDelete]
    [Route("items/{id:int}")]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    // DELETE /api/v1/[controller]/items/{id}
    public async Task<ActionResult> DeleteCatalogItemAsync([FromRoute] int id)
    {
        if (id < 0)
            return BadRequest();

        var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null)
            return NotFound();

        _catalogContext.CatalogItems.Remove(item);
        await _catalogContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut]
    [Route("items")]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    // PUT /api/v1/[controller]/items
    public async Task<ActionResult> UpdateCatalogItemAsync([FromBody] UpdateCatalogItemViewModel item)
    {
        var catalogItem = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == item.Id);
        if (catalogItem == null)
            return NotFound();

        catalogItem.CatalogBrandId = item.CatalogBrandId ?? catalogItem.CatalogBrandId;
        catalogItem.CatalogTypeId = item.CatalogTypeId ?? catalogItem.CatalogTypeId;
        catalogItem.Description = item.Description ?? catalogItem.Description;
        catalogItem.Name = item.Name ?? catalogItem.Name;
        catalogItem.PictureFileName = item.PictureFileName ?? catalogItem.PictureFileName;
        catalogItem.AvailableStock = item.AvailableStock ?? catalogItem.AvailableStock;
        catalogItem.OnReorder = item.OnReorder ?? catalogItem.OnReorder;
        var oldPrice = catalogItem.Price;
        catalogItem.Price = item.Price ?? catalogItem.Price;

        await _catalogContext.SaveChangesAsync();
        if (oldPrice != catalogItem.Price)
            await _publishEndpoint.Publish<ICatalogItemPriceChanged>(new
            {
                ProductId = catalogItem.Id,
                NewPrice = catalogItem.Price,
                OldPrice = oldPrice,
            });

        return Ok();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillPictureUri(CatalogItem item)
    {
        item.PictureUri = new Uri(new Uri(_catalogSettings.PicturesBaseUrl), item.PictureFileName).ToString();
    }
}