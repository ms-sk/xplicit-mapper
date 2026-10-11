using FluentAssertions;
using Xunit;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;
using XplicitMapper.Extensions;

namespace XplicitMapper.Tests;

public class CollectionMappingTests
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string? DisplayName { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductToProductDtoMapper : IXMapper<Product, ProductDto>
    {
        public ProductDto Map(Product source)
        {
            return new ProductDto
            {
                Id = source.Id,
                DisplayName = $"Product: {source.Name}",
                Price = source.Price
            };
        }
    }

    [Fact]
    public void MapEach_WithItems_MapsEachItem()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new ProductToProductDtoMapper());

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple", Price = 1.99m },
            new() { Id = 2, Name = "Banana", Price = 0.99m }
        };

        // Act
        var results = products.MapEach<Product, ProductDto>(provider).ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].DisplayName.Should().Be("Product: Apple");
        results[1].DisplayName.Should().Be("Product: Banana");
    }

    [Fact]
    public void MapToList_WithItems_ReturnsList()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new ProductToProductDtoMapper());

        var products = new[]
        {
            new Product { Id = 1, Name = "Apple", Price = 1.99m },
            new Product { Id = 2, Name = "Banana", Price = 0.99m }
        };

        // Act
        var results = products.MapToList<Product, ProductDto>(provider);

        // Assert
        results.Should().BeOfType<List<ProductDto>>();
        results.Should().HaveCount(2);
    }

    [Fact]
    public void MapToArray_WithItems_ReturnsArray()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new ProductToProductDtoMapper());

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple", Price = 1.99m }
        };

        // Act
        var results = products.MapToArray<Product, ProductDto>(provider);

        // Assert
        results.Should().BeOfType<ProductDto[]>();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void MapEach_NullSource_ReturnsEmpty()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new ProductToProductDtoMapper());

        // Act
        var results = ((List<Product>)null!).MapEach<Product, ProductDto>(provider).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void MapToList_UsingProviderExtension_Works()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new ProductToProductDtoMapper());

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Test", Price = 10m }
        };

        // Act
        var results = provider.MapToList<Product, ProductDto>(products);

        // Assert
        results.Should().HaveCount(1);
    }
}
