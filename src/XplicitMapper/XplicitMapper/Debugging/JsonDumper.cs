using System.Text.Json;

namespace XplicitMapper.Debugging;

/// <summary>
/// Utility class for dumping objects to JSON format for debugging.
/// </summary>
public static class JsonDumper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
        IncludeFields = true,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes an object to a JSON string.
    /// Handles circular references by ignoring them.
    /// </summary>
    /// <typeparam name="T">Type of object to dump</typeparam>
    /// <param name="obj">The object to serialize</param>
    /// <param name="options">Optional JsonSerializerOptions (uses defaults if null)</param>
    /// <returns>Indented JSON string</returns>
    public static string DumpToJson<T>(T? obj, JsonSerializerOptions? options = null)
    {
        if (obj == null)
            return "null";

        try
        {
            return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
        }
        catch (Exception ex)
        {
            return $"[Serialization failed: {ex.Message}]";
        }
    }

    /// <summary>
    /// Dumps an object to the console as formatted JSON.
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="obj">The object to dump</param>
    /// <param name="header">Optional header to print before the JSON</param>
    /// <param name="options">Optional JsonSerializerOptions</param>
    public static void DumpToConsole<T>(T? obj, string? header = null, JsonSerializerOptions? options = null)
    {
        if (!string.IsNullOrEmpty(header))
        {
            Console.WriteLine(header);
        }

        string json = DumpToJson(obj, options);
        Console.WriteLine(json);
        Console.WriteLine();
    }

    /// <summary>
    /// Dumps both a source and destination object to console, useful for comparing before/after mapping.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object (before mapping)</param>
    /// <param name="destination">Destination object (after mapping)</param>
    /// <param name="sourceLabel">Label for source (default: "Source")</param>
    /// <param name="destLabel">Label for destination (default: "Destination")</param>
    public static void DumpMappingComparison<TSource, TDestination>(
        TSource? source,
        TDestination? destination,
        string sourceLabel = "Source",
        string destLabel = "Destination")
    {
        Console.WriteLine($"=== {typeof(TSource).Name} -> {typeof(TDestination).Name} ===");
        Console.WriteLine();
        DumpToConsole(source, $"--- {sourceLabel} ({typeof(TSource).Name}) ---");
        DumpToConsole(destination, $"--- {destLabel} ({typeof(TDestination).Name}) ---");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine();
    }
}

/// <summary>
/// Extension methods for convenient JSON dumping directly on objects.
/// </summary>
public static class JsonDumpExtensions
{
    /// <summary>
    /// Converts this object to a JSON string.
    /// </summary>
    public static string ToJsonDump<T>(this T? obj)
    {
        return JsonDumper.DumpToJson(obj);
    }

    /// <summary>
    /// Dumps this object to the console as formatted JSON.
    /// </summary>
    public static void DumpToConsole<T>(this T? obj, string? header = null)
    {
        JsonDumper.DumpToConsole(obj, header);
    }
}
