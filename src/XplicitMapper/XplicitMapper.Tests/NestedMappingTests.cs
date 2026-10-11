using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;
using XplicitMapper.DependencyInjection;

namespace XplicitMapper.Tests;

public class NestedMappingTests
{
    // Test models with nesting
    public class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
    }

    public class AddressDto
    {
        public string? FullAddress { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public Address? Address { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string? DisplayName { get; set; }
        public AddressDto? Address { get; set; }
    }

    // Circular reference models
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public Category? Parent { get; set; }  // Circular!
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public CategoryDto? Parent { get; set; }  // Circular!
    }

    // Mappers
    public class AddressToAddressDtoMapper : IXMapper<Address, AddressDto>
    {
        public AddressDto Map(Address source)
        {
            return new AddressDto
            {
                FullAddress = $"{source.Street}, {source.City}"
            };
        }
    }

    public class CustomerToCustomerDtoMapper : IXMapper<Customer, CustomerDto>
    {
        private readonly IMapperProvider _mapperProvider;

        public CustomerToCustomerDtoMapper(IMapperProvider mapperProvider)
        {
            _mapperProvider = mapperProvider;
        }

        public CustomerDto Map(Customer source)
        {
            return new CustomerDto
            {
                Id = source.Id,
                DisplayName = source.Name?.ToUpperInvariant(),
                Address = source.Address != null 
                    ? _mapperProvider.Map<Address, AddressDto>(source.Address) 
                    : null
            };
        }
    }

    // Circular reference mapper
    public class CategoryToCategoryDtoMapper : IXMapper<Category, CategoryDto>
    {
        private readonly IMapperProvider _mapperProvider;

        public CategoryToCategoryDtoMapper(IMapperProvider mapperProvider)
        {
            _mapperProvider = mapperProvider;
        }

        public CategoryDto Map(Category source)
        {
            return new CategoryDto
            {
                Id = source.Id,
                Name = source.Name,
                Parent = source.Parent != null
                    ? _mapperProvider.Map<Category, CategoryDto>(source.Parent)
                    : null
            };
        }
    }

    [Fact]
    public void Map_WithNestedProperty_MapsNestedType()
    {
        // Arrange using DI
        var services = new ServiceCollection();
        services.AddXplicitMapper(opt =>
        {
            opt.AddMapper<AddressToAddressDtoMapper>();
            opt.AddMapper<CustomerToCustomerDtoMapper>();
        });
        
        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IMapperProvider>();

        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Address = new Address { Street = "123 Main St", City = "Boston" }
        };

        // Act
        var dto = provider.Map<Customer, CustomerDto>(customer);

        // Assert
        dto.Should().NotBeNull();
        dto.DisplayName.Should().Be("JOHN DOE");
        dto.Address.Should().NotBeNull();
        dto.Address.FullAddress.Should().Be("123 Main St, Boston");
    }

    [Fact]
    public void Map_CircularReferenceWithThrowOption_Throws()
    {
        // Arrange
        var options = new XplicitMapperOptions
        {
            CircularReferenceHandling = CircularReferenceHandling.Throw
        };
        var provider = new MapperProvider(options);
        var mapper = new CategoryToCategoryDtoMapper(provider);
        provider.Register(mapper);

        // Create circular reference
        var cat1 = new Category { Id = 1, Name = "Parent" };
        var cat2 = new Category { Id = 2, Name = "Child", Parent = cat1 };
        cat1.Parent = cat2;  // Circular!

        // Act
        var act = () => provider.Map<Category, CategoryDto>(cat1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Circular reference detected*");
    }

    [Fact]
    public void Map_CircularReferenceWithReturnNull_ReturnsNull()
    {
        // Arrange
        var options = new XplicitMapperOptions
        {
            CircularReferenceHandling = CircularReferenceHandling.ReturnNull
        };
        var provider = new MapperProvider(options);
        var mapper = new CategoryToCategoryDtoMapper(provider);
        provider.Register(mapper);

        // Create circular reference
        var cat1 = new Category { Id = 1, Name = "Parent" };
        var cat2 = new Category { Id = 2, Name = "Child", Parent = cat1 };
        cat1.Parent = cat2;

        // Act - this should not throw, just return null at the cycle point
        var act = () => provider.Map<Category, CategoryDto>(cat1);

        // Assert - doesn't throw
        act.Should().NotThrow();
    }

    [Fact]
    public void MappingContext_TryVisit_TracksVisited()
    {
        // Arrange
        var context = new MappingContext(new MapperProvider());
        var source = typeof(Category);
        var dest = typeof(CategoryDto);

        // Act
        var firstVisit = context.TryVisit(source, dest);
        var secondVisit = context.TryVisit(source, dest);  // Same pair
        context.Exit(source, dest);
        var afterExit = context.TryVisit(source, dest);

        // Assert
        firstVisit.Should().BeTrue();
        secondVisit.Should().BeFalse();  // Duplicate visit
        afterExit.Should().BeTrue();  // After exit, can visit again
    }
}
