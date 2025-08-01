namespace DudNet.Sample;

internal interface IEntity
{
	string Id { get; set; }
}

internal interface IPerson : IEntity
{
	string? FirstName { get; set; }
	string? LastName { get; set; }

	string FullName();
}