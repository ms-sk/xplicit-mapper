using XplicitMapper.Debugging;

namespace XplicitMapper.Core;

/// <summary>
/// Callback invoked before mapping occurs.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
/// <param name="source">Source object being mapped</param>
/// <param name="context">Current mapping context</param>
public delegate void BeforeMapDumpCallback<TSource, TDestination>(TSource source, MappingContext context);

/// <summary>
/// Callback invoked after mapping occurs.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
/// <param name="source">Source object</param>
/// <param name="destination">Mapped destination object</param>
/// <param name="context">Current mapping context</param>
public delegate void AfterMapDumpCallback<TSource, TDestination>(TSource source, TDestination destination, MappingContext context);

/// <summary>
/// Non-generic callback invoked before mapping occurs.
/// </summary>
/// <param name="sourceType">Type of source</param>
/// <param name="destType">Type of destination</param>
/// <param name="source">Source object</param>
/// <param name="context">Mapping context</param>
public delegate void BeforeMapDumpCallback(Type sourceType, Type destType, object source, MappingContext context);

/// <summary>
/// Non-generic callback invoked after mapping occurs.
/// </summary>
/// <param name="sourceType">Type of source</param>
/// <param name="destType">Type of destination</param>
/// <param name="source">Source object</param>
/// <param name="destination">Destination object</param>
/// <param name="context">Mapping context</param>
public delegate void AfterMapDumpCallback(Type sourceType, Type destType, object source, object? destination, MappingContext context);

/// <summary>
/// Configuration options for XplicitMapper.
/// </summary>
public class XplicitMapperOptions
{
    /// <summary>
    /// How to handle circular references during mapping.
    /// Default: Throw
    /// </summary>
    public CircularReferenceHandling CircularReferenceHandling { get; set; } 
        = CircularReferenceHandling.Throw;

    /// <summary>
    /// Whether to throw an exception when mapping a null source.
    /// Default: false (returns null/default)
    /// </summary>
    public bool ThrowOnNullSource { get; set; } = false;

    /// <summary>
    /// Callback invoked before every mapping operation.
    /// Use this for debugging, logging, or JSON dumping.
    /// </summary>
    public event BeforeMapDumpCallback? BeforeMapDump;

    /// <summary>
    /// Callback invoked after every mapping operation.
    /// Use this for debugging, logging, or comparing source/destination as JSON.
    /// </summary>
    public event AfterMapDumpCallback? AfterMapDump;

    /// <summary>
    /// Invokes the BeforeMapDump event. Used by mapper provider implementations.
    /// </summary>
    public void InvokeBeforeMapDump(Type sourceType, Type destType, object source, MappingContext context)
    {
        BeforeMapDump?.Invoke(sourceType, destType, source, context);
    }

    /// <summary>
    /// Invokes the AfterMapDump event. Used by mapper provider implementations.
    /// </summary>
    public void InvokeAfterMapDump(Type sourceType, Type destType, object source, object? destination, MappingContext context)
    {
        AfterMapDump?.Invoke(sourceType, destType, source, destination, context);
    }
}

/// <summary>
/// Extension methods to conveniently enable JSON dumping on XplicitMapperOptions.
/// </summary>
public static class XplicitMapperOptionsDebugExtensions
{
    /// <summary>
    /// Enables automatic JSON console output before and after every mapping operation.
    /// Great for debugging what gets mapped.
    /// </summary>
    /// <param name="options">The options to configure</param>
    /// <param name="dumpBeforeMap">If true, dumps source object before mapping (default: true)</param>
    /// <param name="dumpAfterMap">If true, dumps both source and destination after mapping for comparison (default: true)</param>
    /// <returns>The options for chaining</returns>
    public static XplicitMapperOptions EnableJsonConsoleDumps(
        this XplicitMapperOptions options,
        bool dumpBeforeMap = true,
        bool dumpAfterMap = true)
    {
        if (dumpBeforeMap)
        {
            options.BeforeMapDump += (sourceType, destType, source, context) =>
            {
                // Only dump at root level (not nested mappings) to reduce noise
                if (context.Depth == 1)
                {
                    Console.WriteLine($"[XplicitMapper] Before map: {sourceType.Name} -> {destType.Name}");
                    JsonDumper.DumpToConsole(source, "Source:");
                }
            };
        }

        if (dumpAfterMap)
        {
            options.AfterMapDump += (sourceType, destType, source, destination, context) =>
            {
                if (context.Depth == 1)  // Called before Exit(), so still at depth 1 for root
                {
                    Console.WriteLine($"[XplicitMapper] After map: {sourceType.Name} -> {destType.Name}");
                    JsonDumper.DumpMappingComparison(source, destination);
                }
            };
        }

        return options;
    }

    /// <summary>
    /// Enables JSON dumps only for specific type pairs. More targeted than EnableJsonConsoleDumps().
    /// </summary>
    public static XplicitMapperOptions EnableJsonConsoleDumpsFor<TSource, TDestination>(
        this XplicitMapperOptions options,
        bool dumpBeforeMap = true,
        bool dumpAfterMap = true)
    {
        var sourceType = typeof(TSource);
        var destType = typeof(TDestination);

        if (dumpBeforeMap)
        {
            options.BeforeMapDump += (sType, dType, source, context) =>
            {
                if (sType == sourceType && dType == destType)
                {
                    Console.WriteLine($"[XplicitMapper] Before map: {sourceType.Name} -> {destType.Name}");
                    JsonDumper.DumpToConsole(source, "Source:");
                }
            };
        }

        if (dumpAfterMap)
        {
            options.AfterMapDump += (sType, dType, source, destination, context) =>
            {
                if (sType == sourceType && dType == destType)
                {
                    Console.WriteLine($"[XplicitMapper] After map: {sourceType.Name} -> {destType.Name}");
                    JsonDumper.DumpMappingComparison(source, destination);
                }
            };
        }

        return options;
    }
}
