# XplicitMapper

An explicit, compile-time safe object mapper for .NET. Map every property yourself with no hidden conventions.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Why XplicitMapper?

XplicitMapper requires you to **explicitly define every mapping**. This means:

- ✅ **No hidden behavior** - Every mapping is visible in your code
- ✅ **Compile-time safety** - Refactoring won't silently break your mappings
- ✅ **Full control** - Complex transformations are straightforward
- ✅ **Simple to debug** - Step through your mapping logic like any other code
- ✅ **Convenient hooks** - `BeforeMap` and `AfterMap` for cross-cutting concerns
- ✅ **Circular reference detection** - Built-in protection against stack overflows
- ✅ **Validation** - Verify all properties are mapped

## Installation

```bash
# Core package
dotnet add package XplicitMapper

# Dependency Injection integration (optional)
dotnet add package XplicitMapper.DependencyInjection
```

## Quick Start

### 1. Create a Mapper

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}

// Implement IXMapper<TSource, TDestination>
public class UserMapper : IXMapper<User, UserDto>
{
    public UserDto Map(User source)
    {
        return new UserDto
        {
            Id = source.Id,
            FullName = $"{source.FirstName} {source.LastName}",  // Custom logic
            Email = source.Email
        };
    }
}
```

### 2. Basic Usage

```csharp
var provider = new MapperProvider();
provider.Register(new UserMapper());

var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
var dto = provider.Map<User, UserDto>(user);
```

### 3. With Dependency Injection

```csharp
// In Program.cs
builder.Services.AddXplicitMapper(opt =>
{
    // Scan an assembly for all mapper implementations
    opt.ScanAssemblyContaining<UserMapper>();
    
    // Or register explicitly
    opt.AddMapper<UserMapper>();
    
    // Configure circular reference handling
    opt.Configure(cfg =>
    {
        cfg.CircularReferenceHandling = CircularReferenceHandling.Throw;
    });
});

// In your controller/service
public class UserService
{
    private readonly IMapperProvider _mapperProvider;

    public UserService(IMapperProvider mapperProvider)
    {
        _mapperProvider = mapperProvider;
    }

    public UserDto GetUser(int id)
    {
        var user = _repository.GetUser(id);
        return _mapperProvider.Map<User, UserDto>(user);
    }
}
```

## Features

### BeforeMap / AfterMap Hooks

```csharp
public class OrderMapper : IXMapper<Order, OrderDto>, IBeforeMap<Order, OrderDto>, IAfterMap<Order, OrderDto>
{
    public void BeforeMap(Order source, MappingContext context)
    {
        // Called before Map() - e.g., validation, logging
    }

    public OrderDto Map(Order source)
    {
        return new OrderDto { /* ... */ };
    }

    public void AfterMap(Order source, OrderDto destination, MappingContext context)
    {
        // Called after Map() - e.g., additional transformations, audit logging
    }
}
```

### Nested Mapping

```csharp
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
            // Use IMapperProvider for nested objects
            Customer = _mapperProvider.Map<Customer, CustomerDto>(source.Customer),
            Items = source.Items.MapToList<OrderItem, OrderItemDto>(_mapperProvider)
        };
    }
}
```

### Circular Reference Detection

```csharp
// Configure behavior
var options = new XplicitMapperOptions
{
    CircularReferenceHandling = CircularReferenceHandling.ReturnNull  // or .Throw, .ReturnDefault
};
var provider = new MapperProvider(options);

// Circular references are automatically detected
// Parent -> Child -> Parent won't cause stack overflow
```

### Collection Mapping

```csharp
var users = new List<User> { user1, user2, user3 };

// Map to IEnumerable
var dtos = users.MapEach<User, UserDto>(_mapperProvider);

// Map to List<T>
var dtoList = users.MapToList<User, UserDto>(_mapperProvider);

// Map to Array
var dtoArray = users.MapToArray<User, UserDto>(_mapperProvider);
```

### Mapping Validation

```csharp
var result = MappingValidator.Validate<User, UserDto>(_mapperProvider);

if (!result.IsValid)
{
    foreach (var warning in result.UnmappedProperties)
    {
        Console.WriteLine($"Warning: Property '{warning}' not found in mapper");
    }
}

// Or throw an exception
try
{
    MappingValidator.ValidateAndThrow<User, UserDto>(_mapperProvider);
}
catch (UnmappedPropertyException ex)
{
    foreach (var prop in ex.UnmappedProperties)
    {
        Console.WriteLine($"Missing: {prop}");
    }
}

// Ignore specific properties with [XplicitIgnore]
public class UserDto
{
    public int Id { get; set; }
    
    [XplicitIgnore]  // Don't warn about this being unmapped
    public DateTime? CalculatedAt { get; set; }
}
```

## API Reference

### Interfaces

| Interface | Description |
|-----------|-------------|
| `IXMapper<TSource, TDestination>` | Core interface - implement `Map()` method |
| `IXMapperEx<TSource, TDestination>` | Optional - implement `Map(existing)` for mapping into existing objects |
| `IBeforeMap<TSource, TDestination>` | Hook - `BeforeMap()` called before mapping |
| `IAfterMap<TSource, TDestination>` | Hook - `AfterMap()` called after mapping |
| `IMapperProvider` | Resolve and use registered mappers |
| `IMapperRegistry` | Register mappers (implemented by `MapperProvider`) |

### Options

```csharp
new XplicitMapperOptions
{
    CircularReferenceHandling = CircularReferenceHandling.Throw  // Default
    // or:
    // CircularReferenceHandling.ReturnNull    - Return null for circular refs
    // CircularReferenceHandling.ReturnDefault - Return default(T) for circular refs
}
```

## License

MIT License - see [LICENSE](LICENSE) file.

## Contributing

Contributions are welcome!

## Why "Xplicit"?

Because you **explicitly** define every mapping. No magic, no conventions, just clear, maintainable code.
