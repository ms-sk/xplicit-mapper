using FluentAssertions;
using Xunit;
using XplicitMapper.Validation;

namespace XplicitMapper.Tests;

public class ValidationTests
{
    // Test types that match
    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public class PersonDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime BirthDate { get; set; }
    }

    // Test types with some different properties
    public class Employee
    {
        public int EmployeeId { get; set; }  // Different name
        public string? Name { get; set; }
        public string? InternalCode { get; set; }  // Only in source
    }

    public class EmployeeDto
    {
        public int Id { get; set; }  // Different name
        public string? Name { get; set; }
        public string? Department { get; set; }  // Only in dest
    }

    // With [XplicitIgnore]
    public class Document
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }

    public class DocumentDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        
        [XplicitIgnore]
        public string? ComputedField { get; set; }  // Explicitly ignored
    }

    [Fact]
    public void Validate_WithMatchingProperties_IsValid()
    {
        // Act
        var result = MappingValidator.Validate<Person, PersonDto>();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DifferentPropertyNames_ReportsUnmapped()
    {
        // Act
        var result = MappingValidator.Validate<Employee, EmployeeDto>();

        // Assert
        result.IsValid.Should().BeFalse();
        result.UnmappedDestinationProperties.Should().Contain("Id");
        result.UnmappedDestinationProperties.Should().Contain("Department");
    }

    [Fact]
    public void Validate_WithXplicitIgnore_IgnoresProperty()
    {
        // Act
        var result = MappingValidator.Validate<Document, DocumentDto>();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Messages.Should().Contain(m => m.Contains("ComputedField") && m.Contains("[XplicitIgnore]"));
    }

    [Fact]
    public void Validate_WithIgnoredPropertiesConfig_IsValid()
    {
        // Act
        var result = MappingValidator.Validate<Employee, EmployeeDto>(opt =>
        {
            opt.IgnoredDestinationProperties.Add("Id");
            opt.IgnoredDestinationProperties.Add("Department");
        });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithCheckSourceProperties_ReportsSourceProperties()
    {
        // Act
        var result = MappingValidator.Validate<Employee, EmployeeDto>(opt =>
        {
            opt.CheckSourceProperties = true;
        });

        // Assert
        result.UnmappedSourceProperties.Should().Contain("EmployeeId");
        result.UnmappedSourceProperties.Should().Contain("InternalCode");
    }

    [Fact]
    public void AssertValid_WhenInvalid_ThrowsUnmappedPropertyException()
    {
        // Act
        var act = () => MappingValidator.AssertValid<Employee, EmployeeDto>();

        // Assert
        var exception = act.Should().Throw<UnmappedPropertyException>().Which;
        exception.SourceType.Should().Be(typeof(Employee));
        exception.DestinationType.Should().Be(typeof(EmployeeDto));
        exception.UnmappedDestinationProperties.Should().NotBeEmpty();
    }

    [Fact]
    public void MappingValidationResult_AddsPropertiesCorrectly()
    {
        // Arrange
        var result = new MappingValidationResult();
        
        // Act
        result.AddUnmappedDestinationProperty("Test1");
        result.AddUnmappedSourceProperty("Test2");
        result.AddMessage("Info message");

        // Assert
        result.IsValid.Should().BeFalse();
        result.UnmappedDestinationProperties.Should().Contain("Test1");
        result.UnmappedSourceProperties.Should().Contain("Test2");
        result.Messages.Should().Contain("Info message");
    }
}
