namespace XplicitMapper.Validation;

/// <summary>
/// Result of a mapping validation check.
/// </summary>
public class MappingValidationResult
{
    private readonly List<string> _unmappedDestinationProperties = new();
    private readonly List<string> _unmappedSourceProperties = new();
    private readonly List<string> _messages = new();

    /// <summary>
    /// Whether the validation passed (no unmapped properties found).
    /// </summary>
    public bool IsValid => !_unmappedDestinationProperties.Any() && !_unmappedSourceProperties.Any();

    /// <summary>
    /// Properties in the destination type that appear to be unmapped.
    /// </summary>
    public IReadOnlyList<string> UnmappedDestinationProperties => _unmappedDestinationProperties.AsReadOnly();

    /// <summary>
    /// Properties in the source type that appear to be unused.
    /// </summary>
    public IReadOnlyList<string> UnmappedSourceProperties => _unmappedSourceProperties.AsReadOnly();

    /// <summary>
    /// Informational messages from validation.
    /// </summary>
    public IReadOnlyList<string> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Adds an unmapped destination property.
    /// </summary>
    public void AddUnmappedDestinationProperty(string propertyName)
    {
        _unmappedDestinationProperties.Add(propertyName);
    }

    /// <summary>
    /// Adds an unmapped source property.
    /// </summary>
    public void AddUnmappedSourceProperty(string propertyName)
    {
        _unmappedSourceProperties.Add(propertyName);
    }

    /// <summary>
    /// Adds an informational message.
    /// </summary>
    public void AddMessage(string message)
    {
        _messages.Add(message);
    }

    /// <summary>
    /// Throws an UnmappedPropertyException if validation failed.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public void AssertValid<TSource, TDestination>()
    {
        if (IsValid)
            return;

        var message = $"Mapping validation failed for {typeof(TSource).Name} -> {typeof(TDestination).Name}.\n";
        
        if (UnmappedDestinationProperties.Any())
        {
            message += $"Unmapped destination properties: {string.Join(", ", UnmappedDestinationProperties)}\n";
        }
        
        if (UnmappedSourceProperties.Any())
        {
            message += $"Unmapped source properties: {string.Join(", ", UnmappedSourceProperties)}\n";
        }

        throw new UnmappedPropertyException(
            message.Trim(),
            typeof(TSource),
            typeof(TDestination),
            UnmappedDestinationProperties,
            UnmappedSourceProperties);
    }
}
