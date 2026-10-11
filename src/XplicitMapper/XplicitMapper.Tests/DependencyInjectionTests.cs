using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;
using XplicitMapper.DependencyInjection;

namespace XplicitMapper.Tests;

public class DependencyInjectionTests
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
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductMapper : IXMapper<Product, ProductDto>
    {
        public ProductDto Map(Product source)
        {
            return new ProductDto
            {
                Id = source.Id,
                Name = source.Name,
                Price = source.Price
            };
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public Product? Product { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class OrderMapper : IXMapper<Order, OrderDto>
    {
        private readonly IMapperProvider _mapperProvider;

        public OrderMapper(IMapperProvider mapperProvider)
        {
            _mapperProvider = mapperProvider;
        }

        public OrderDto Map(Order source)
        {
            return new OrderDto
            {
                Id = source.Id,
                Product = source.Product != null
                    ? _mapperProvider.Map<Product, ProductDto>(source.Product)
                    : null
            };
        }
    }

    [Fact]
    public void AddXplicitMapper_RegistersIMapperProvider()
    {
        var services = new ServiceCollection();
        services.AddXplicitMapper();
        
        var provider = services.BuildServiceProvider();
        var mapperProvider = provider.GetService<IMapperProvider>();

        mapperProvider.Should().NotBeNull();
        mapperProvider.Should().BeAssignableTo<IMapperProvider>();
    }

    [Fact]
    public void AddMapper_RegistersMapper()
    {
        var services = new ServiceCollection();
        services.AddXplicitMapper(opt =>
        {
            opt.AddMapper<ProductMapper>();
        });
        
        var provider = services.BuildServiceProvider();
        var mapperProvider = provider.GetRequiredService<IMapperProvider>();

        mapperProvider.HasMapper(typeof(Product), typeof(ProductDto)).Should().BeTrue();
    }

    [Fact]
    public void AddMapper_UsingRegisteredMapper_Works()
    {
        var services = new ServiceCollection();
        services.AddXplicitMapper(opt =>
        {
            opt.AddMapper<ProductMapper>();
        });
        
        var provider = services.BuildServiceProvider();
        var mapperProvider = provider.GetRequiredService<IMapperProvider>();

        var product = new Product { Id = 1, Name = "Test", Price = 10.99m };

        var dto = mapperProvider.Map<Product, ProductDto>(product);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Test");
        dto.Price.Should().Be(10.99m);
    }

    [Fact]
    public void ScanAssemblyContaining_FindsMappers()
    {
        var services = new ServiceCollection();
        services.AddXplicitMapper(opt =>
        {
            opt.ScanAssemblyContaining<ProductMapper>();
        });
        
        var provider = services.BuildServiceProvider();
        var mapperProvider = provider.GetRequiredService<IMapperProvider>();

        mapperProvider.HasMapper(typeof(Product), typeof(ProductDto)).Should().BeTrue();
    }

    [Fact]
    public void MapperWithConstructorDependency_GetsInjected()
    {
        var services = new ServiceCollection();
        services.AddXplicitMapper(opt =>
        {
            opt.AddMapper<ProductMapper>();
            opt.AddMapper<OrderMapper>();
        });
        
        var provider = services.BuildServiceProvider();
        var mapperProvider = provider.GetRequiredService<IMapperProvider>();

        var order = new Order
        {
            Id = 1,
            Product = new Product { Id = 100, Name = "Widget", Price = 99.99m }
        };

        var dto = mapperProvider.Map<Order, OrderDto>(order);

        dto.Should().NotBeNull();
        dto.Product.Should().NotBeNull();
        dto.Product.Name.Should().Be("Widget");
    }

    [Fact]
    public void ConfigureOptions_CustomizesSettings()
    {
        var services = new ServiceCollection();
        services.AddXplicitMapper(opt =>
        {
            opt.Configure(cfg =>
            {
                cfg.ThrowOnNullSource = true;
                cfg.CircularReferenceHandling = CircularReferenceHandling.ReturnNull;
            });
        });
        
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<XplicitMapperOptions>();

        options.Should().NotBeNull();
        options.ThrowOnNullSource.Should().BeTrue();
        options.CircularReferenceHandling.Should().Be(CircularReferenceHandling.ReturnNull);
    }
}
