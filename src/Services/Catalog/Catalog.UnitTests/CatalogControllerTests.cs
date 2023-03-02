using Catalog.API.Controllers;
using Catalog.API.Infrastructure;
using Catalog.API.Model;
using Catalog.API.ViewModel;
using Catalog.Contracts;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Catalog.UnitTests;

public class CatalogControllerTests
{
    private CatalogContext _context = null!;
    private CatalogController _controller = null!;
    private Mock<IPublishEndpoint> _publishEndpointMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase("test-db").Options;
        _context = new CatalogContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        _context.CatalogBrands.AddRange(GetTestingCatalogBrands());
        _context.CatalogTypes.AddRange(GetTestingCatalogTypes());
        _context.CatalogItems.AddRange(GetTestingCatalogItems());
        _context.SaveChanges();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _publishEndpointMock.Setup(p => p.Publish(It.IsAny<ICatalogItemPriceChanged>(), CancellationToken.None))
                            .Returns(Task.CompletedTask);

        _controller = new CatalogController(_context, new TestingCatalogSettings(), _publishEndpointMock.Object,
            new Mock<ILogger<CatalogController>>().Object);
    }

    [Test]
    public async Task GetCatalogBrandByIdAsyncTest()
    {
        var brand = GetTestingCatalogBrands().First();
        var actionResult = await _controller.CatalogBrandByIdAsync(1);
        actionResult.Should().BeOfType<ActionResult<CatalogBrand>>();
        actionResult.Value?.Brand.Should().Be(brand.Brand);
        var notFoundActionResult = await _controller.CatalogBrandByIdAsync(0);
        notFoundActionResult.Result.Should().BeAssignableTo<NotFoundResult>();
    }

    [Test]
    public async Task GetCatalogItemByIdAsyncTest()
    {
        var item = GetTestingCatalogItems().First();
        var actionResult = await _controller.CatalogItemByIdAsync(1);
        actionResult.Should().BeOfType<ActionResult<CatalogItem>>();
        actionResult.Value?.Name.Should().Be(item.Name);
        var notFoundActionResult = await _controller.CatalogBrandByIdAsync(0);
        notFoundActionResult.Result.Should().BeAssignableTo<NotFoundResult>();
    }

    [Test]
    public async Task GetItemsWithPaginationAsync()
    {
        var items = GetTestingCatalogItems().ToArray();
        var actionResult = await _controller.GetItemsWithPaginationAsync(items.Length / 2);
        actionResult.Should().BeOfType<ActionResult<PaginatedViewModel<CatalogItem>>>();
        var data = actionResult.Value!.Items.ToArray();
        actionResult.Value.PageIndex.Should().Be(0);
        actionResult.Value.PageSize.Should().Be(items.Length / 2);
        data.Should().HaveCount(2);
        for (var i = 0; i < items[..(items.Length / 2)].Length; i++)
            data[i]
                .Should()
                .BeEquivalentTo(items[i], x =>
                {
                    return x.Excluding(o => o.Id)
                            .Excluding(o => o.PictureUri)
                            .Excluding(o => o.CatalogType)
                            .Excluding(o => o.CatalogBrand);
                });

        var actionResult2 = await _controller.GetItemsWithPaginationAsync(items.Length / 2, 1);
        actionResult2.Should().BeOfType<ActionResult<PaginatedViewModel<CatalogItem>>>();
        var data2 = actionResult.Value!.Items.ToArray();
        actionResult2.Value!.PageIndex.Should().Be(1);
        actionResult2.Value.PageSize.Should().Be(items.Length / 2);
        data2.Should().HaveCount(2);
        for (var i = 0; i < items[(items.Length / 2)..].Length; i++)
            data2[i]
                .Should()
                .BeEquivalentTo(items[i], x =>
                {
                    return x.Excluding(o => o.Id)
                            .Excluding(o => o.PictureUri)
                            .Excluding(o => o.CatalogType)
                            .Excluding(o => o.CatalogBrand);
                });
    }

    [Test]
    public async Task GetCatalogBrandsAsync()
    {
        var brands = GetTestingCatalogBrands().ToArray();
        var actionResult = await _controller.GetCatalogBrandsAsync();
        actionResult.Should().BeOfType<ActionResult<IEnumerable<CatalogBrand>>>();
        var value = actionResult.Value?.ToArray();
        value.Should().NotBeNull();
        value!.Length.Should().Be(brands.Length);
        for (var i = 0; i < value.Length; i++)
            value[i].Brand.Should().Be(brands[i].Brand);
    }

    [Test]
    public async Task GetCatalogTypesAsync()
    {
        var types = GetTestingCatalogTypes().ToArray();
        var actionResult = await _controller.GetCatalogTypesAsync();
        actionResult.Should().BeOfType<ActionResult<IEnumerable<CatalogType>>>();
        var value = actionResult.Value?.ToArray();
        value.Should().NotBeNull();
        value!.Length.Should().Be(types.Length);
        for (var i = 0; i < value.Length; i++)
            value[i].Type.Should().Be(types[i].Type);
    }

    [Test]
    public async Task GetCatalogTypeByIdAsyncTest()
    {
        var type = GetTestingCatalogTypes().First();
        var actionResult = await _controller.CatalogTypeByIdAsync(1);
        actionResult.Should().BeOfType<ActionResult<CatalogType>>();
        actionResult.Value?.Type.Should().Be(type.Type);
        var notFoundActionResult = await _controller.CatalogBrandByIdAsync(0);
        notFoundActionResult.Result.Should().BeAssignableTo<NotFoundResult>();
    }

    [Test]
    public async Task CreateCatalogBrandAsyncTest()
    {
        const string brandName = "new-brand";
        var actionResult = await _controller.CreateCatalogBrandAsync(new CreateCatalogBrandViewModel
        {
            Brand = brandName,
        });
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var brand = await _context.CatalogBrands.SingleOrDefaultAsync(x => x.Brand == brandName);
        brand.Should().NotBeNull();
        brand!.Brand.Should().Be(brandName);
    }

    [Test]
    public async Task CreateCatalogTypeAsyncTest()
    {
        const string typeName = "new-type";
        var actionResult = await _controller.CreateCatalogTypeAsync(new CreateCatalogTypeViewModel
        {
            Type = typeName,
        });
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var type = await _context.CatalogTypes.SingleOrDefaultAsync(x => x.Type == typeName);
        type.Should().NotBeNull();
        type!.Type.Should().Be(typeName);
    }

    [Test]
    public async Task CreateCatalogItemAsyncTest()
    {
        const string itemName = "new-item";
        const string itemDescription = "description";
        const string itemPictureFileName = "new.jpg";
        const decimal itemPrice = 100;
        const int itemAvailableStock = 10;
        const int itemTypeId = 1;
        const int itemBrandId = 1;

        var actionResult = await _controller.CreateCatalogItemAsync(new CreateCatalogItemViewModel
        {
            Name = itemName,
            Description = itemDescription,
            Price = itemPrice,
            AvailableStock = itemAvailableStock,
            CatalogBrandId = itemBrandId,
            CatalogTypeId = itemTypeId,
            PictureFileName = itemPictureFileName,
            OnReorder = false,
        });
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var item = await _context.CatalogItems.SingleOrDefaultAsync(x => x.Name == itemName);
        item.Should().NotBeNull();
        item!.Name.Should().Be(itemName);
        item.Description.Should().Be(itemDescription);
        item.Price.Should().Be(itemPrice);
        item.AvailableStock.Should().Be(itemAvailableStock);
        item.CatalogBrandId.Should().Be(itemBrandId);
        item.CatalogTypeId.Should().Be(itemTypeId);
        item.PictureFileName.Should().Be(itemPictureFileName);
        item.OnReorder.Should().Be(false);
    }

    [Test]
    public async Task DeleteCatalogBrandAsync()
    {
        var actionResult = await _controller.DeleteCatalogBrandAsync(1);
        actionResult.Should().BeOfType<OkResult>();
        var data = await _context.CatalogBrands.SingleOrDefaultAsync(x => x.Id == 1);
        data.Should().BeNull();
        var actionResult2 = await _controller.DeleteCatalogBrandAsync(0);
        actionResult2.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task DeleteCatalogTypeAsync()
    {
        var actionResult = await _controller.DeleteCatalogTypeAsync(1);
        actionResult.Should().BeOfType<OkResult>();
        var data = await _context.CatalogTypes.SingleOrDefaultAsync(x => x.Id == 1);
        data.Should().BeNull();
        var actionResult2 = await _controller.DeleteCatalogTypeAsync(0);
        actionResult2.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task DeleteCatalogItemAsyncTest()
    {
        var actionResult = await _controller.DeleteCatalogItemAsync(1);
        actionResult.Should().BeOfType<OkResult>();
        var data = await _context.CatalogItems.SingleOrDefaultAsync(x => x.Id == 1);
        data.Should().BeNull();
        var actionResult2 = await _controller.DeleteCatalogItemAsync(0);
        actionResult2.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task UpdateCatalogBrandAsyncTest()
    {
        const string newBrandName = "new-brand";

        var actionResult = await _controller.UpdateCatalogBrandAsync(new UpdateCatalogBrandViewModel
        {
            Id = 1,
            Brand = newBrandName,
        });
        actionResult.Should().BeOfType<OkResult>();
        var data = await _context.CatalogBrands.SingleOrDefaultAsync(x => x.Id == 1);
        data.Should().NotBeNull();
        data!.Brand.Should().Be(newBrandName);
        var actionResult2 = await _controller.UpdateCatalogBrandAsync(new UpdateCatalogBrandViewModel
        {
            Id = 0,
        });
        actionResult2.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task UpdateCatalogItemAsyncTest()
    {
        var item = GetTestingCatalogItems().First();
        var viewModel = new UpdateCatalogItemViewModel
        {
            Id = 1,
            Description = "new-description",
            Price = -100,
        };
        var actionResult = await _controller.UpdateCatalogItemAsync(viewModel);
        actionResult.Should().BeOfType<OkResult>();
        var data = await _context.CatalogItems.SingleOrDefaultAsync(x => x.Id == 1);
        data.Should().NotBeNull();
        data!.Name.Should().Be(item.Name);
        data.Description.Should().Be(viewModel.Description);
        data.Price.Should().Be(viewModel.Price);
        data.AvailableStock.Should().Be(item.AvailableStock);
        var actionResult2 = await _controller.UpdateCatalogItemAsync(new UpdateCatalogItemViewModel
        {
            Id = 0,
        });
        actionResult2.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task UpdateCatalogTypeAsyncTest()
    {
        const string newTypeName = "new-type";

        var actionResult = await _controller.UpdateCatalogTypeAsync(new UpdateCatalogTypeViewModel
        {
            Id = 1,
            Type = newTypeName,
        });
        actionResult.Should().BeOfType<OkResult>();
        var data = await _context.CatalogTypes.SingleOrDefaultAsync(x => x.Id == 1);
        data.Should().NotBeNull();
        data!.Type.Should().Be(newTypeName);
        var actionResult2 = await _controller.UpdateCatalogTypeAsync(new UpdateCatalogTypeViewModel
        {
            Id = 0,
        });
        actionResult2.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task UpdateCatalogItemPricePublishEventTest()
    {
        var item = GetTestingCatalogItems().First();
        var actionResult = await _controller.UpdateCatalogItemAsync(new UpdateCatalogItemViewModel
        {
            Id = 1,
            Price = item.Price * 2,
        });
        actionResult.Should().BeOfType<OkResult>();
        var publishArgs = _publishEndpointMock.Invocations.First().Arguments;
        var @event = publishArgs.First();
        @event.Should()
              .BeEquivalentTo(new
              {
                  ProductId = 1,
                  NewPrice = item.Price * 2,
                  OldPrice = item.Price,
              });
    }

    private IEnumerable<CatalogItem> GetTestingCatalogItems() =>
        new List<CatalogItem>
        {
            new()
            {
                Name = "Item1",
                Description = "Description1",
                Price = 1,
                PictureFileName = "1.jpg",
                CatalogBrandId = 1,
                CatalogTypeId = 1,
                AvailableStock = 10,
                OnReorder = false,
            },
            new()
            {
                Name = "Item2",
                Description = "Description2",
                Price = 2,
                PictureFileName = "2.jpg",
                CatalogBrandId = 1,
                CatalogTypeId = 2,
                AvailableStock = 10,
                OnReorder = false,
            },
            new()
            {
                Name = "Item3",
                Description = "Description3",
                Price = 3,
                PictureFileName = "3.jpg",
                CatalogBrandId = 1,
                CatalogTypeId = 3,
                AvailableStock = 10,
                OnReorder = false,
            },
            new()
            {
                Name = "Item4",
                Description = "Description4",
                Price = 4,
                PictureFileName = "4.jpg",
                CatalogBrandId = 2,
                CatalogTypeId = 1,
                AvailableStock = 10,
                OnReorder = false,
            },
        };

    private IEnumerable<CatalogBrand> GetTestingCatalogBrands() =>
        new List<CatalogBrand>
        {
            new()
            {
                Brand = "Brand1",
            },
            new()
            {
                Brand = "Brand2",
            },
        };

    private IEnumerable<CatalogType> GetTestingCatalogTypes() =>
        new List<CatalogType>
        {
            new()
            {
                Type = "Type1",
            },
            new()
            {
                Type = "Type2",
            },
            new()
            {
                Type = "Type3",
            },
        };
}