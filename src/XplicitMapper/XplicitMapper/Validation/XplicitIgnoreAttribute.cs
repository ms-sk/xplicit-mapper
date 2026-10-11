namespace XplicitMapper.Validation;

/// <summary>
/// Marks a property as explicitly ignored during mapping validation.
/// Properties with this attribute won't be reported as unmapped.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class XplicitIgnoreAttribute : Attribute
{
    /// <summary>
    /// Creates a new instance of XplicitIgnoreAttribute.
    /// </summary>
    public XplicitIgnoreAttribute()
    {
    }
}
