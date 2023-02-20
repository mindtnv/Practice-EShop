using Catalog.API;
using Microsoft.Extensions.Options;

namespace Catalog.UnitTests;

public class TestingCatalogSettings : IOptionsSnapshot<CatalogSettings>
{
    public CatalogSettings Value => new()
    {
        PicturesBaseUrl = "https://test-server.com/",
    };
    public CatalogSettings Get(string? name) => Value;
}