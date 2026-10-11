namespace XplicitMapper.Core;

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
}
