using FluentAssertions;
using Xunit;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;
using XplicitMapper.Extensions;

namespace XplicitMapper.Tests;

public class BasicMappingTests
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

    // Mapper implementation
    public class UserToUserDtoMapper : IXMapper<User, UserDto>
    {
        public UserDto Map(User source)
        {
            return new UserDto
            {
                Id = source.Id,
                FullName = $"{source.FirstName} {source.LastName}",
                Email = source.Email
            };
        }
    }

    [Fact]
    public void Map_WithRegisteredMapper_MapsCorrectly()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new UserToUserDtoMapper());

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act
        var dto = provider.Map<User, UserDto>(user);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.FullName.Should().Be("John Doe");
        dto.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Map_UsingMapToExtension_MapsCorrectly()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new UserToUserDtoMapper());

        var user = new User
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com"
        };

        // Act
        var dto = user.MapTo<User, UserDto>(provider);

        // Assert
        dto.Should().NotBeNull();
        dto.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void GetMapper_WithoutRegistration_Throws()
    {
        // Arrange
        var provider = new MapperProvider();

        // Act
        var act = () => provider.GetMapper<User, UserDto>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No mapper registered*");
    }

    [Fact]
    public void HasMapper_WithRegistration_ReturnsTrue()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new UserToUserDtoMapper());

        // Act
        var result = provider.HasMapper(typeof(User), typeof(UserDto));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Map_NullSource_ReturnsDefault()
    {
        // Arrange
        var provider = new MapperProvider();
        provider.Register(new UserToUserDtoMapper());

        // Act
        var result = provider.Map<User, UserDto>(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Map_NullSourceWithThrowOnNull_Throws()
    {
        // Arrange
        var options = new XplicitMapperOptions
        {
            ThrowOnNullSource = true
        };
        var provider = new MapperProvider(options);
        provider.Register(new UserToUserDtoMapper());

        // Act
        var act = () => provider.Map<User, UserDto>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
