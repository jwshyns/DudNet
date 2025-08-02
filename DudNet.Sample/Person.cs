using DudNet.Attributes;

namespace DudNet.Sample;

[ProxyService]
public sealed class Person : IPerson
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string FullName()
    {
        return FirstName + " " + LastName;
    }

    public void FullName(string fullname)
    {
    }

    public string Id { get; set; }
}