using DudNet.Attributes;

namespace DudNet.Sample;

[ProxyService]
public class Person : IPerson
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }

	public string FullName()
	{
		return FirstName + " " + LastName;
	}

	public string Id { get; set; }
}