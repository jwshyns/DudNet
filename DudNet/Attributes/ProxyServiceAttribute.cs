namespace DudNet.Attributes;

/// <summary>
///     Used for making whether a service should be proxied.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
// ReSharper disable once ClassCanBeSealed.Global
public class ProxyServiceAttribute : Attribute
{
}