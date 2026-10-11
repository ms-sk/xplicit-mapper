namespace XplicitMapper.Validation;

/// <summary>
/// Exception thrown when unmapped properties are discovered during validation.
/// </summary>
public class UnmappedPropertyException : Exception
{
    /// <summary>
    /// The source type being validated.
    /// </summary>
    public Type? SourceType { get; }

    /// <summary>
    /// The destination type being validated.
    /// </summary>
    public Type? DestinationType { get; }

    /// <summary>
    /// List of unmapped destination property names.
    /// </summary>
    public IReadOnlyList<string> UnmappedDestinationProperties { get; }

    /// <summary>
    /// List of unmapped source property names.
    /// </summary>
    public IReadOnlyList<string> UnmappedSourceProperties { get; }

    /// <summary>
    /// Creates a new UnmappedPropertyException.
    /// </summary>
    public UnmappedPropertyException(
        string message,
        Type sourceType,
        Type destinationType,
        IEnumerable<string> unmappedDestinationProperties,
        IEnumerable<string>? unmappedSourceProperties = null)
        : base(message)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        UnmappedDestinationProperties = unmappedDestinationProperties.ToList().AsReadOnly();
        UnmappedSourceProperties = (unmappedSourceProperties ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
    }
}
