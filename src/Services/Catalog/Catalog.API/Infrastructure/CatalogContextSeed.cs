using Catalog.API.Model;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure;

public class CatalogContextSeed
{
    private readonly ILogger<CatalogContextSeed> _logger;

    public CatalogContextSeed(ILogger<CatalogContextSeed> logger)
    {
        _logger = logger;
    }

    public async Task SeedAsync(CatalogContext context)
    {
        _logger.LogInformation("Seeding database associated with context {ContextType}", context.GetType());
        await SeedCatalogBrandsAsync(context);
        await SeedCatalogTypesAsync(context);
        await SeedCatalogItemsAsync(context);
    }

    private async Task SeedCatalogBrandsAsync(CatalogContext context)
    {
        if (!await context.CatalogBrands.AnyAsync())
        {
            _logger.LogInformation("Seeding Catalog Brands...");
            await context.CatalogBrands.AddRangeAsync(GetCatalogBrands());
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedCatalogTypesAsync(CatalogContext context)
    {
        if (!await context.CatalogTypes.AnyAsync())
        {
            _logger.LogInformation("Seeding Catalog Types...");
            await context.CatalogTypes.AddRangeAsync(GetCatalogTypes());
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedCatalogItemsAsync(CatalogContext context)
    {
        if (!await context.CatalogItems.AnyAsync())
        {
            _logger.LogInformation("Seeding Catalog Items...");
            await context.CatalogItems.AddRangeAsync(GetCatalogItems());
            await context.SaveChangesAsync();
        }
    }

    private List<CatalogItem> GetCatalogItems() =>
        new List<CatalogItem>
        {
            new()
            {
                Name = "Philips Coffee Machine MX 21",
                Description = "Powerful Philips Coffee Machine",
                Price = 200,
                PictureFileName = "philips-coffee-machine-mx-21.jpg",
                AvailableStock = 10,
                CatalogBrandId = 5,
                CatalogTypeId = 1,
            },
            new()
            {
                Name = "Bosh Coffee Maker",
                Description = "Coffee Machine by Bosh",
                Price = 120,
                PictureFileName = "bosh-coffee-maker.jpg",
                AvailableStock = 14,
                CatalogBrandId = 4,
                CatalogTypeId = 1,
            },
            new()
            {
                Name = "Mega Coffee Maker",
                Description = "Coffee Machine by Bosh",
                Price = 120,
                PictureFileName = "bosh-coffee-maker.jpg",
                AvailableStock = 14,
                CatalogBrandId = 4,
                CatalogTypeId = 1,
            },
            new()
            {
                Name = "Triple S Sneaker",
                Description = "Triple S Sneaker in black rubber",
                Price = 1100,
                PictureFileName = "balenciaga-triple-s-sneaker.jpg",
                AvailableStock = 4,
                CatalogBrandId = 1,
                CatalogTypeId = 3,
            },
            new()
            {
                Name = "Track Sneaker",
                Description = "Track Sneaker in white mesh and nylon",
                Price = 1050,
                PictureFileName = "balenciaga-track-sneaker.jpg",
                AvailableStock = 7,
                CatalogBrandId = 1,
                CatalogTypeId = 3,
            },
            new()
            {
                Name = "Track Sneaker",
                Description = "Track Sneaker in white mesh and nylon",
                Price = 1050,
                PictureFileName = "balenciaga-track-sneaker.jpg",
                AvailableStock = 7,
                CatalogBrandId = 1,
                CatalogTypeId = 3,
            },
            new()
            {
                Name = "Star Sneaker",
                Description = "White Calfskin and Suede",
                Price = 890,
                PictureFileName = "dior-star-sneaker.jpg",
                AvailableStock = 13,
                CatalogBrandId = 2,
                CatalogTypeId = 3,
            },
            new()
            {
                Name = "Versace Trainer Maxi Sneaker",
                Description = "Versace Initials on the tongue, side and back",
                Price = 949,
                PictureFileName = "versace-trainer-maxi-sneaker.jpg",
                AvailableStock = 2,
                CatalogBrandId = 3,
                CatalogTypeId = 3,
            },
            new()
            {
                Name = "Vacuum Cleaner T12",
                Description = "Brand new vacuum cleaner by Philips",
                Price = 319,
                PictureFileName = "philips-vacuum-cleaner-t12.jpg",
                AvailableStock = 23,
                CatalogBrandId = 5,
                CatalogTypeId = 2,
            },
            new()
            {
                Name = "Bosh vacuum master VC456",
                Description = "Vacuum Cleaner VC456 by Bosh",
                Price = 449,
                PictureFileName = "bosh-vacuum-master-vc456.jpg",
                AvailableStock = 19,
                CatalogBrandId = 4,
                CatalogTypeId = 2,
            },
            new()
            {
                Name = "Power Wrench",
                Description = "Power Wrench by Makita",
                Price = 79,
                PictureFileName = "makita-power-wrench.jpg",
                AvailableStock = 8,
                CatalogBrandId = 6,
                CatalogTypeId = 5,
            },
            new()
            {
                Name = "Versace Flower",
                Description = "Versace Flower perfume",
                Price = 219,
                PictureFileName = "versace-flower-perfume.jpg",
                AvailableStock = 5,
                CatalogBrandId = 3,
                CatalogTypeId = 5,
            },
            new()
            {
                Name = "Versace Breeze",
                Description = "Versace Breeze perfume",
                Price = 229,
                PictureFileName = "versace-breeze-perfume.jpg",
                AvailableStock = 5,
                CatalogBrandId = 3,
                CatalogTypeId = 5,
            },
            new()
            {
                Name = "Dior King",
                Description = "Dior King perfume",
                Price = 329,
                PictureFileName = "dior-king-perfume.jpg",
                AvailableStock = 3,
                CatalogBrandId = 2,
                CatalogTypeId = 5,
            },
        };

    private List<CatalogType> GetCatalogTypes() =>
        new List<CatalogType>
        {
            new()
            {
                Type = "Coffee Machine",
            },
            new()
            {
                Type = "Vacuum Cleaner",
            },
            new()
            {
                Type = "Sneakers",
            },
            new()
            {
                Type = "Perfume",
            },
            new()
            {
                Type = "Tools",
            },
        };

    private List<CatalogBrand> GetCatalogBrands() =>
        new List<CatalogBrand>
        {
            new()
            {
                Brand = "Balenciaga",
            },
            new()
            {
                Brand = "Dior",
            },
            new()
            {
                Brand = "Versace",
            },
            new()
            {
                Brand = "Bosch",
            },
            new()
            {
                Brand = "Philips",
            },
            new()
            {
                Brand = "Makita",
            },
        };
}