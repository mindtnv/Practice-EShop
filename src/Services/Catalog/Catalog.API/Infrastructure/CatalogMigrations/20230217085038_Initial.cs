#nullable disable
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Catalog.API.Infrastructure.CatalogMigrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "CatalogBrand",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                          .Annotation("Npgsql:ValueGenerationStrategy",
                              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Brand = table.Column<string>("character varying(50)", maxLength: 50, nullable: false),
            },
            constraints: table => { table.PrimaryKey("PK_CatalogBrand", x => x.Id); });

        migrationBuilder.CreateTable(
            "CatalogType",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                          .Annotation("Npgsql:ValueGenerationStrategy",
                              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Type = table.Column<string>("character varying(100)", maxLength: 100, nullable: false),
            },
            constraints: table => { table.PrimaryKey("PK_CatalogType", x => x.Id); });

        migrationBuilder.CreateTable(
            "Catalog",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                          .Annotation("Npgsql:ValueGenerationStrategy",
                              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>("character varying(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>("text", nullable: false),
                Price = table.Column<decimal>("numeric", nullable: false),
                PictureFileName = table.Column<string>("text", nullable: true),
                CatalogTypeId = table.Column<int>("integer", nullable: false),
                CatalogBrandId = table.Column<int>("integer", nullable: false),
                AvailableStock = table.Column<int>("integer", nullable: false),
                OnReorder = table.Column<bool>("boolean", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Catalog", x => x.Id);
                table.ForeignKey(
                    "FK_Catalog_CatalogBrand_CatalogBrandId",
                    x => x.CatalogBrandId,
                    "CatalogBrand",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_Catalog_CatalogType_CatalogTypeId",
                    x => x.CatalogTypeId,
                    "CatalogType",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            "IX_Catalog_CatalogBrandId",
            "Catalog",
            "CatalogBrandId");

        migrationBuilder.CreateIndex(
            "IX_Catalog_CatalogTypeId",
            "Catalog",
            "CatalogTypeId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Catalog");

        migrationBuilder.DropTable(
            "CatalogBrand");

        migrationBuilder.DropTable(
            "CatalogType");
    }
}