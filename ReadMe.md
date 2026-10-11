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

### 4. Convenient Extension Methods

XplicitMapper provides many convenient extension methods to improve readability:

#### MapTo - "Natural" mapping flow

Instead of calling on the provider, call `.MapTo()` on your source object:

```csharp
// Provider style
var dto = _mapperProvider.Map<User, UserDto>(user);

// More natural: MapTo extension on the source
var dto2 = user.MapTo<User, UserDto>(_mapperProvider);

// Async version
var dto3 = await user.MapToAsync<User, UserDto>(_mapperProvider, cancellationToken);
```

#### TryMap - Non-throwing mapping

Safe mapping that doesn't throw when no mapper is registered:

```csharp
// With out parameter
if (_mapperProvider.TryMap<Product, ProductDto>(product, out var dto))
{
    // Use dto
}

// With nullable return
var dto2 = _mapperProvider.TryMap<Product, ProductDto>(product);
if (dto2 != null) { /* ... */ }

// Or call on the source
if (product.TryMapTo(_mapperProvider, out ProductDto? dto3)) { /* ... */ }

var dto4 = product.TryMapTo<Product, ProductDto>(_mapperProvider);

// Async versions
var dto5 = await _mapperProvider.TryMapAsync<Product, ProductDto>(product, ct);
var dto6 = await product.TryMapToAsync<Product, ProductDto>(_mapperProvider, ct);
```

#### MapEach on IMapperProvider - fluent collection mapping

All collection methods are available directly on `IMapperProvider`:

```csharp
var users = new List<User> { user1, user2, user3 };

// Enumerables
IEnumerable<UserDto> dtos = _mapperProvider.MapEach<User, UserDto>(users);
IAsyncEnumerable<UserDto> dtosAsync = _mapperProvider.MapEachAsync<User, UserDto>(users, ct);

// Lists
List<UserDto> list = _mapperProvider.MapToList<User, UserDto>(users);
List<UserDto> listAsync = await _mapperProvider.MapToListAsync<User, UserDto>(users, ct);

// Arrays
UserDto[] array = _mapperProvider.MapToArray<User, UserDto>(users);
UserDto[] arrayAsync = await _mapperProvider.MapToArrayAsync<User, UserDto>(users, ct);

// Works with IAsyncEnumerable<T> directly (great with EF Core)
var asyncProducts = dbContext.Products.AsAsyncEnumerable();
var mappedStream = _mapperProvider.MapEachAsync<Product, ProductDto>(asyncProducts, ct);
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

// Or call on provider directly
var dtos2 = _mapperProvider.MapEach<User, UserDto>(users);

// Map to List<T>
var dtoList = users.MapToList<User, UserDto>(_mapperProvider);
var dtoList2 = _mapperProvider.MapToList<User, UserDto>(users);

// Map to Array
var dtoArray = users.MapToArray<User, UserDto>(_mapperProvider);
var dtoArray2 = _mapperProvider.MapToArray<User, UserDto>(users);
```

### Async Mapping (I/O-bound scenarios like EF Core)

XplicitMapper supports async mapping for I/O-bound scenarios (e.g., loading related data from a database via EF Core, making HTTP calls, etc.).

#### Example: EF Core Entity → DTO with async loading

```csharp
// Entity (EF Core model)
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    // Foreign key
    public int CategoryId { get; set; }
}

// DTO
public class ProductDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; }  // Needs async lookup
}

// Async mapper - implement IAsyncXMapper<TSource, TDestination> (keeps "X" consistent with IXMapper)
public class ProductMapper : IAsyncXMapper<Product, ProductDto>
{
    private readonly AppDbContext _dbContext;

    public ProductMapper(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDto> MapAsync(Product source, CancellationToken cancellationToken = default)
    {
        // Async call to EF Core - load category name from database
        var categoryName = await _dbContext.Categories
            .Where(c => c.Id == source.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return new ProductDto
        {
            Id = source.Id,
            DisplayName = $"Product: {source.Name}",
            Price = source.Price,
            CategoryName = categoryName ?? "Unknown"
        };
    }
}
```

#### Registering and using async mappers

```csharp
// Register with MapperProvider
var provider = new MapperProvider();
provider.Register(new ProductMapper(dbContext));

// Or with Dependency Injection
builder.Services.AddXplicitMapper(opt =>
{
    opt.ScanAssemblyContaining<ProductMapper>();  // Discovers both sync and async mappers
});

// Usage in your service
public class ProductService
{
    private readonly IMapperProvider _mapperProvider;
    private readonly AppDbContext _dbContext;

    public ProductService(IMapperProvider mapperProvider, AppDbContext dbContext)
    {
        _mapperProvider = mapperProvider;
        _dbContext = dbContext;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product == null)
            return null;

        // Uses IAsyncXMapper<Product, ProductDto> if available
        return await _mapperProvider.MapAsync<Product, ProductDto>(product, ct);
    }
}
```

#### Async collection mapping with EF Core

```csharp
public async Task<List<ProductDto>> GetAllProductsAsync(CancellationToken ct = default)
{
    var products = await _dbContext.Products
        .OrderBy(p => p.Name)
        .ToListAsync(ct);

    // Option 1: Map each item asynchronously
    return await products.MapToListAsync<Product, ProductDto>(_mapperProvider, ct);

    // Option 2: Call on provider directly
    // return await _mapperProvider.MapToListAsync<Product, ProductDto>(products, ct);
}

// Works with IAsyncEnumerable<T> too
public async IAsyncEnumerable<ProductDto> StreamProductsAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    var asyncProducts = _dbContext.Products
        .AsAsyncEnumerable();

    await foreach (var dto in _mapperProvider.MapEachAsync<Product, ProductDto>(asyncProducts, ct))
    {
        yield return dto;
    }
}
```

#### Async hooks: IBeforeMapAsync / IAfterMapAsync

```csharp
public class OrderMapper : 
    IAsyncXMapper<Order, OrderDto>, 
    IBeforeMapAsync<Order, OrderDto>, 
    IAfterMapAsync<Order, OrderDto>
{
    private readonly IAuditLogService _auditLog;

    public OrderMapper(IAuditLogService auditLog)
    {
        _auditLog = auditLog;
    }

    public async Task BeforeMapAsync(Order source, OrderDto destination, MappingContext context, CancellationToken ct = default)
    {
        // Async pre-mapping logic (e.g., validation, loading related data)
        await _auditLog.LogMappingStartAsync(typeof(Order), typeof(OrderDto), ct);
    }

    public async Task<OrderDto> MapAsync(Order source, CancellationToken ct = default)
    {
        return new OrderDto
        {
            Id = source.Id,
            // ...
        };
    }

    public async Task AfterMapAsync(Order source, OrderDto destination, MappingContext context, CancellationToken ct = default)
    {
        // Async post-mapping logic
        await _auditLog.LogMappingCompleteAsync(source.Id, ct);
    }
}
```

#### Non-throwing mapping: TryMap / TryMapAsync

```csharp
// Safe mapping - returns false if no mapper is registered
if (_mapperProvider.TryMap<Product, ProductDto>(product, out var dto))
{
    // Mapping succeeded
}

// Or with nullable return
var dto2 = _mapperProvider.TryMap<Product, ProductDto>(product);
if (dto2 != null) { /* ... */ }

// Async versions
var dto3 = await _mapperProvider.TryMapAsync<Product, ProductDto>(product, ct);
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
| `IXMapper<TSource, TDestination>` | Core sync interface - implement `Map()` method |
| `IXMapperEx<TSource, TDestination>` | Optional sync - implement `Map(existing)` for mapping into existing objects |
| `IBeforeMap<TSource, TDestination>` | Sync hook - `BeforeMap()` called before mapping |
| `IAfterMap<TSource, TDestination>` | Sync hook - `AfterMap()` called after mapping |
| `IAsyncXMapper<TSource, TDestination>` | **Async interface** for I/O-bound mappers - implement `MapAsync()` (e.g., EF Core, HTTP calls) - "X" for eXplicit, consistent with `IXMapper` |
| `IAsyncXMapperEx<TSource, TDestination>` | Optional async - `MapAsync(existing)` for mapping into existing objects |
| `IBeforeMapAsync<TSource, TDestination>` | Async hook - `BeforeMapAsync()` |
| `IAfterMapAsync<TSource, TDestination>` | Async hook - `AfterMapAsync()` |
| `IMapperProvider` | Resolve and use registered mappers (sync + async) |
| `IMapperRegistry` | Register mappers (implemented by `MapperProvider`) |

### Convenient Extension Methods (on `IMapperProvider` and objects)

| Method | Description |
|--------|-------------|
| `obj.MapTo<TSource, TDestination>(provider)` | Extension on any object - more natural mapping flow |
| `await obj.MapToAsync<TSource, TDestination>(provider, ct)` | Async version of MapTo |
| `provider.TryMap<TSource, TDestination>(source, out dest)` | Non-throwing sync mapping with `out` parameter |
| `provider.TryMap<TSource, TDestination>(source)` | Returns nullable - `null`/`default` if no mapper registered |
| `await provider.TryMapAsync<TSource, TDestination>(source, ct)` | Async non-throwing mapping |
| `source.TryMapTo<TSource, TDestination>(provider)` / `source.TryMapTo(provider, out dest)` | Non-throwing extensions directly on objects |
| `await source.TryMapToAsync<TSource, TDestination>(provider, ct)` | Async non-throwing on objects |
| `provider.MapEach<TSource, TDestination>(collection)` | Collection mapping - returns `IEnumerable<TDestination>` |
| `provider.MapEachAsync<TSource, TDestination>(collection, ct)` | Async collection - returns `IAsyncEnumerable<TDestination>` |
| `provider.MapEachAsync<TSource, TDestination>(asyncEnumerable, ct)` | Works directly with `IAsyncEnumerable<T>` (EF Core streaming) |
| `provider.MapToList<...>()` / `provider.MapToArray<...>()` | Collection to `List<T>` or `T[]` |
| `await provider.MapToListAsync<...>()` / `await provider.MapToArrayAsync<...>()` | Async collection materialization |

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
