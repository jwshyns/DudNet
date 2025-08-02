using DudNet.Sample;
using FluentAssertions;
using NSubstitute;

namespace DudNet.Tests.Sample;

public sealed class PersonProxyTests
{
    private readonly PersonProxy _sut;
    private readonly IPerson _service = Substitute.For<IPerson>();
    
    public PersonProxyTests()
    {
        _sut = new PersonProxy(_service);
    }

    public sealed record GetCallConfiguration(Func<IPerson, object?> Func, object Value);

    public sealed record SetCallConfiguration(Action<IPerson> Func);

    public static TheoryData<GetCallConfiguration> GetConfigurations() =>
    [
        new(person => person.FirstName, nameof(IPerson.FirstName)),
        new(person => person.LastName, nameof(IPerson.LastName)),
        new(person => person.Id, nameof(IPerson.Id)),
        new(person => person.FullName(), nameof(IPerson.FullName))
    ];

    public static TheoryData<SetCallConfiguration> SetConfigurations() =>
    [
        new(person => person.FirstName = nameof(IPerson.FirstName)),
        new(person => person.LastName = nameof(IPerson.LastName)),
        new(person => person.Id = nameof(IPerson.Id)),
        new(person => person.FullName(nameof(IPerson.FullName)))
    ];

    [Theory]
#pragma warning disable xUnit1044
    [MemberData(nameof(GetConfigurations))]
#pragma warning restore xUnit1044
    public void GetMethods_ReturnServiceValues_WhenCalled(GetCallConfiguration getCallConfiguration)
    {
        // Arrange
        var (func, value) = getCallConfiguration;
        func(_service).Returns(value);

        // Act
        var result = func(_sut);

        // Assert
#pragma warning disable NS5000
        _ = func(_service.Received());
#pragma warning restore NS5000
        result.Should().Be(value);
    }

    [Theory]
#pragma warning disable xUnit1044
    [MemberData(nameof(SetConfigurations))]
#pragma warning restore xUnit1044
    public void SetMethods_SetServiceValues_WhenCalled(SetCallConfiguration setCallConfiguration)
    {
        // Arrange
        var func = setCallConfiguration.Func;
        
        // Act
        func(_sut);
        
        // Assert
#pragma warning disable NS5000
        func(_service.Received());
#pragma warning restore NS5000

        _service.ReceivedCalls();
    }
}