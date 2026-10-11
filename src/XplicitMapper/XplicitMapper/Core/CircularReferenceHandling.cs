namespace XplicitMapper.Core;

/// <summary>
/// Specifies how circular references should be handled during mapping.
/// </summary>
public enum CircularReferenceHandling
{
    /// <summary>
    /// Throw an exception when a circular reference is detected. (Default)
    /// </summary>
    Throw = 0,

    /// <summary>
    /// Return the default value for the destination type when a circular reference is detected.
    /// For reference types, this means null.
    /// </summary>
    ReturnDefault = 1,

    /// <summary>
    /// Return null when a circular reference is detected.
    /// </summary>
    ReturnNull = 2
}
