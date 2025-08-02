namespace DudNet.Sample;

public interface IEntity
{
	string Id { get; set; }
}

public interface IPerson : IEntity
{
	string? FirstName { get; set; }
	string? LastName { get; set; }
	string FullName();
	void FullName(string fullname);
}