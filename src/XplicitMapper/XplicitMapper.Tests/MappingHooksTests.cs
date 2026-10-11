using FluentAssertions;
using Xunit;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;

namespace XplicitMapper.Tests;

public class MappingHooksTests
{
    public class Order
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public string? CurrencyFromContext { get; set; }
        public DateTime MappedAt { get; set; }
    }

    // Mapper with both BeforeMap and AfterMap
    public class OrderWithHooksMapper : 
        IXMapper<Order, OrderDto>, 
        IBeforeMap<Order, OrderDto>, 
        IAfterMap<Order, OrderDto>
    {
        public void BeforeMap(Order source, OrderDto destination, MappingContext context)
        {
            // Store currency in context for later use
            context.Items["Currency"] = source.Currency ?? "USD";
        }

        public OrderDto Map(Order source)
        {
            return new OrderDto
            {
                Id = source.Id,
                Total = source.Amount
            };
        }

        public void AfterMap(Order source, OrderDto destination, MappingContext context)
        {
            // Retrieve from context
            destination.CurrencyFromContext = context.Items["Currency"]?.ToString();
            destination.MappedAt = DateTime.UtcNow;
        }
    }

    // Mapper with only BeforeMap
    public class OnlyBeforeMapMapper : 
        IXMapper<Order, OrderDto>, 
        IBeforeMap<Order, OrderDto>
    {
        public bool BeforeMapCalled { get; private set; }

        public void BeforeMap(Order source, OrderDto destination, MappingContext context)
        {
            BeforeMapCalled = true;
        }

        public OrderDto Map(Order source)
        {
            return new OrderDto { Id = source.Id, Total = source.Amount };
        }
    }

    // Mapper with only AfterMap
    public class OnlyAfterMapMapper : 
        IXMapper<Order, OrderDto>, 
        IAfterMap<Order, OrderDto>
    {
        public bool AfterMapCalled { get; private set; }

        public OrderDto Map(Order source)
        {
            return new OrderDto { Id = source.Id, Total = source.Amount };
        }

        public void AfterMap(Order source, OrderDto destination, MappingContext context)
        {
            AfterMapCalled = true;
        }
    }

    [Fact]
    public void Map_WithBothHooks_CallsInOrder()
    {
        // Arrange
        var provider = new MapperProvider();
        var mapper = new OrderWithHooksMapper();
        provider.Register(mapper);

        var order = new Order
        {
            Id = 1,
            Amount = 100m,
            Currency = "EUR"
        };

        // Act
        var dto = provider.Map<Order, OrderDto>(order);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Total.Should().Be(100m);
        dto.CurrencyFromContext.Should().Be("EUR");
        dto.MappedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Map_OnlyBeforeMap_CallsBeforeMap()
    {
        // Arrange
        var provider = new MapperProvider();
        var mapper = new OnlyBeforeMapMapper();
        provider.Register(mapper);

        var order = new Order { Id = 1, Amount = 50m };

        // Act
        provider.Map<Order, OrderDto>(order);

        // Assert
        mapper.BeforeMapCalled.Should().BeTrue();
    }

    [Fact]
    public void Map_OnlyAfterMap_CallsAfterMap()
    {
        // Arrange
        var provider = new MapperProvider();
        var mapper = new OnlyAfterMapMapper();
        provider.Register(mapper);

        var order = new Order { Id = 1, Amount = 50m };

        // Act
        provider.Map<Order, OrderDto>(order);

        // Assert
        mapper.AfterMapCalled.Should().BeTrue();
    }

    [Fact]
    public void MappingContext_ProvidesMapperProvider()
    {
        // Arrange
        var provider = new MapperProvider();
        var context = new MappingContext(provider);

        // Act & Assert
        context.MapperProvider.Should().BeSameAs(provider);
    }

    [Fact]
    public void MappingContext_Items_Storage()
    {
        // Arrange
        var context = new MappingContext(new MapperProvider());

        // Act
        context.Items["Key"] = "Value";

        // Assert
        context.Items["Key"].Should().Be("Value");
    }
}
