using DudNet.Attributes;

namespace DudNet.Example;

[ProxyService]
public class Person : IPerson
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }

	public string FullName()
	{
		return FirstName + " " + LastName;
	}
}