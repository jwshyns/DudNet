namespace DudNet.Attributes;

/// <summary>
/// Used for making whether a service should be proxied.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ProxyServiceAttribute : Attribute
{
}