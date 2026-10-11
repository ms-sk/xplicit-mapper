using System.Linq.Expressions;
using FluentAssertions;
using Xunit;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;
using XplicitMapper.Extensions;

namespace XplicitMapper.Tests;

public class ProjectionTests
{
    // Test models
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    // Projection mapper implementation
    public class UserProjectionMapper : IXMapperProjection<User, UserDto>
    {
        public Expression<Func<User, UserDto>> GetProjection()
        {
            return user => new UserDto
            {
                Id = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email
            };
        }
    }

    [Fact]
    public void GetProjection_WithRegisteredMapper_ReturnsExpression()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.RegisterProjection(new UserProjectionMapper());

        // Act
        var expression = provider.GetProjection<User, UserDto>();

        // Assert
        expression.Should().NotBeNull();
        expression.Parameters.Should().HaveCount(1);
        expression.Parameters[0].Name.Should().Be("user");
    }

    [Fact]
    public void GetProjection_WithoutRegistration_Throws()
    {
        // Arrange
        var provider = new MapperProvider();

        // Act
        var act = () => provider.GetProjection<User, UserDto>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No projection mapper registered*");
    }

    [Fact]
    public void ProjectTo_WithInMemoryQueryable_ProjectsCorrectly()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.RegisterProjection(new UserProjectionMapper());

        var users = new[]
        {
            new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        }.AsQueryable();

        // Act
        var dtos = provider.ProjectTo<User, UserDto>(users).ToList();

        // Assert
        dtos.Should().HaveCount(2);
        dtos[0].Id.Should().Be(1);
        dtos[0].FullName.Should().Be("John Doe");
        dtos[0].Email.Should().Be("john@example.com");
        dtos[1].Id.Should().Be(2);
        dtos[1].FullName.Should().Be("Jane Smith");
        dtos[1].Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void ProjectTo_WithExtensionMethod_ProjectsCorrectly()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.RegisterProjection(new UserProjectionMapper());

        var users = new[]
        {
            new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" }
        }.AsQueryable();

        // Act
        var dtos = users.ProjectTo<User, UserDto>(provider).ToList();

        // Assert
        dtos.Should().HaveCount(1);
        dtos[0].FullName.Should().Be("John Doe");
    }

    [Fact]
    public void ProjectTo_WithFilterBeforeProjection_AppliesFilter()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.RegisterProjection(new UserProjectionMapper());

        var users = new[]
        {
            new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        }.AsQueryable();

        // Act
        var dtos = users
            .Where(u => u.Id == 1)
            .ProjectTo<User, UserDto>(provider)
            .ToList();

        // Assert
        dtos.Should().HaveCount(1);
        dtos[0].Id.Should().Be(1);
        dtos[0].FullName.Should().Be("John Doe");
    }

    [Fact]
    public void ProjectTo_WithNullSource_Throws()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.RegisterProjection(new UserProjectionMapper());

        // Act
        var act = () => provider.ProjectTo<User, UserDto>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProjectTo_WithNullProvider_Throws()
    {
        // Arrange
        var users = new[] { new User() }.AsQueryable();

        // Act
        var act = () => users.ProjectTo<User, UserDto>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProjectTo_ExpressionCompilesAndExecutesCorrectly()
    {
        // Arrange
        var mapper = new UserProjectionMapper();
        var expression = mapper.GetProjection();
        var func = expression.Compile();

        var user = new User
        {
            Id = 42,
            FirstName = "Alice",
            LastName = "Wonderland",
            Email = "alice@example.com"
        };

        // Act
        var dto = func(user);

        // Assert
        dto.Id.Should().Be(42);
        dto.FullName.Should().Be("Alice Wonderland");
        dto.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public void RegisterProjection_OnMapperProvider_StoresProjection()
    {
        // Arrange
        var provider = new MapperProvider();

        // Act
        provider.RegisterProjection(new UserProjectionMapper());

        // Assert
        provider.HasMapper(typeof(User), typeof(UserDto)).Should().BeTrue();
    }
}
