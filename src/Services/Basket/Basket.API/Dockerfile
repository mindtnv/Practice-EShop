FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# It's important to keep lines from here down to "COPY . ." identical in all Dockerfiles
# to take advantage of Docker's build cache, to speed up local container builds
COPY "EShop.sln" "EShop.sln"
COPY "Services/Basket/Basket.API/Basket.API.csproj" "Services/Basket/Basket.API/Basket.API.csproj"
COPY "Services/Basket/Basket.UnitTests/Basket.UnitTests.csproj" "Services/Basket/Basket.UnitTests/Basket.UnitTests.csproj"
COPY "Services/Catalog/Catalog.API/Catalog.API.csproj" "Services/Catalog/Catalog.API/Catalog.API.csproj"
COPY "Services/Catalog/Catalog.UnitTests/Catalog.UnitTests.csproj" "Services/Catalog/Catalog.UnitTests/Catalog.UnitTests.csproj"
COPY "Services/Catalog/Catalog.Contracts/Catalog.Contracts.csproj" "Services/Catalog/Catalog.Contracts/Catalog.Contracts.csproj"

RUN dotnet restore "EShop.sln"

COPY . .
WORKDIR /src/Services/Basket/Basket.API
RUN dotnet publish --no-restore -c Release -o /app

FROM build as unittest
WORKDIR /src/Services/Basket/Basket.UnitTests

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "Basket.API.dll"]