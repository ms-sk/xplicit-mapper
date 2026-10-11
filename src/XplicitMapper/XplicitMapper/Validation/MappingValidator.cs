using System.Reflection;

namespace XplicitMapper.Validation;

/// <summary>
/// Validates mappings between types by comparing public properties.
/// Detects properties that may be unintentionally unmapped.
/// </summary>
public class MappingValidator
{
    /// <summary>
    /// Options for validation.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Properties to ignore in the destination type.
        /// </summary>
        public HashSet<string> IgnoredDestinationProperties { get; } = new();

        /// <summary>
        /// Properties to ignore in the source type.
        /// </summary>
        public HashSet<string> IgnoredSourceProperties { get; } = new();

        /// <summary>
        /// Whether to check for unmapped source properties.
        /// Default: false
        /// </summary>
        public bool CheckSourceProperties { get; set; } = false;
    }

    /// <summary>
    /// Validates a mapping between two types.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="configure">Optional configuration</param>
    /// <returns>Validation result</returns>
    public static MappingValidationResult Validate<TSource, TDestination>(Action<Options>? configure = null)
    {
        var options = new Options();
        configure?.Invoke(options);

        var result = new MappingValidationResult();

        var sourceType = typeof(TSource);
        var destType = typeof(TDestination);

        // Get public writable properties from destination
        var destProps = GetPublicWritableProperties(destType);
        var sourceProps = GetPublicReadableProperties(sourceType);

        // Properties marked with [XplicitIgnore] are automatically ignored
        var explicitlyIgnoredInDest = destProps
            .Where(p => p.GetCustomAttribute<XplicitIgnoreAttribute>() != null)
            .Select(p => p.Name);

        foreach (var prop in explicitlyIgnoredInDest)
        {
            options.IgnoredDestinationProperties.Add(prop);
            result.AddMessage($"Property '{prop}' is explicitly ignored with [XplicitIgnore]");
        }

        // Check destination properties for mapping
        var sourcePropNames = new HashSet<string>(sourceProps.Select(p => p.Name), StringComparer.Ordinal);

        foreach (var destProp in destProps)
        {
            if (options.IgnoredDestinationProperties.Contains(destProp.Name))
                continue;

            // Simple heuristic: if property name doesn't exist in source at all,
            // flag it as potentially unmapped. This is a basic check - it can't
            // detect computed mappings like FullName = FirstName + " " + LastName
            if (!sourcePropNames.Contains(destProp.Name))
            {
                result.AddUnmappedDestinationProperty(destProp.Name);
            }
        }

        // Check source properties if enabled
        if (options.CheckSourceProperties)
        {
            var destPropNames = new HashSet<string>(
                destProps.Where(p => !options.IgnoredDestinationProperties.Contains(p.Name))
                    .Select(p => p.Name),
                StringComparer.Ordinal);

            foreach (var sourceProp in sourceProps)
            {
                if (options.IgnoredSourceProperties.Contains(sourceProp.Name))
                    continue;

                if (!destPropNames.Contains(sourceProp.Name))
                {
                    result.AddUnmappedSourceProperty(sourceProp.Name);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Validates and throws if any unmapped properties are found.
    /// </summary>
    public static void AssertValid<TSource, TDestination>(Action<Options>? configure = null)
    {
        var result = Validate<TSource, TDestination>(configure);
        result.AssertValid<TSource, TDestination>();
    }

    private static List<PropertyInfo> GetPublicWritableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .Where(p => p.GetIndexParameters().Length == 0) // Not an indexer
            .ToList();
    }

    private static List<PropertyInfo> GetPublicReadableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Where(p => p.GetIndexParameters().Length == 0)
            .ToList();
    }
}
