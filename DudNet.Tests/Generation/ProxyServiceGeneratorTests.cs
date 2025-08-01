using DudNet.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace DudNet.Tests.Generation;

using GeneratorTest =
	Microsoft.CodeAnalysis.CSharp.Testing.CSharpSourceGeneratorTest<SourceGeneratorAdapter<ProxyServiceGenerator>,
		Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

public sealed class ProxyServiceGeneratorTests
{
	private static readonly ReferenceAssemblies Reference = ReferenceAssemblies.Net.Net60;

	[Fact]
	public async Task ShouldGenerateNothing_WhenNoValidTargetFound()
	{
		await new GeneratorTest
		{
			ReferenceAssemblies = Reference,
			TestState =
			{
				Sources = { string.Empty },
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(ProxyServiceGenerator).Assembly.Location)
				}
			},
		}.RunAsync();
	}

	[Fact]
	public async Task ShouldGenerate_WhenValidTargetFound()
	{
		const string input = """
using DudNet.Attributes;
	
namespace TestProject;
	
public interface IExampleService {
	public void ExampleFunction();
	public int ExampleFunctionWithArgumentAndReturn(int number);
}
	
[ProxyService]
public class ExampleService : IExampleService {
	public void ExampleFunction(){
		// omitted for brevity
	}
	public int ExampleFunctionWithArgumentAndReturn(int number){
		// omitted for brevity
		return 0;
	}
	public void FunctionNotOnInterface(){
		// omitted for brevity
	}
}
""";
		
		const string expectedProxy = """
using System.Runtime.CompilerServices;
using DudNet.Attributes;

namespace TestProject;

/// <inheritdoc cref="IExampleService"/>
public partial class ExampleServiceProxy : IExampleService {

	private readonly IExampleService _service;

	public void ExampleFunction() {
		Interceptor();
		ExampleFunctionInterceptor();
		_service.ExampleFunction();
	}

	public int ExampleFunctionWithArgumentAndReturn(int number) {
		Interceptor();
		ExampleFunctionWithArgumentAndReturnInterceptor(number);
		return _service.ExampleFunctionWithArgumentAndReturn(number);
	}

	partial void Interceptor([CallerMemberName]string callerName = null);

	partial void ExampleFunctionInterceptor();

	partial void ExampleFunctionWithArgumentAndReturnInterceptor(int number);

}
""";
		
		const string expectedDud = """
using System.Runtime.CompilerServices;
using DudNet.Attributes;

namespace TestProject;

/// <inheritdoc cref="IExampleService"/>
public partial class ExampleServiceDud : IExampleService {

	public void ExampleFunction() {
	}

	public int ExampleFunctionWithArgumentAndReturn(int number) {
		return (int) default;
	}

}
""";

		await new GeneratorTest
		{
			ReferenceAssemblies = Reference,
			TestState =
			{
				Sources = { input },
				GeneratedSources =
				{
					(typeof(SourceGeneratorAdapter<ProxyServiceGenerator>), "ExampleServiceProxy.g.cs", expectedProxy),
					(typeof(SourceGeneratorAdapter<ProxyServiceGenerator>), "ExampleServiceDud.g.cs", expectedDud)
				},
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(ProxyServiceGenerator).Assembly.Location)
				}
			}
		}.RunAsync();
	}
	
	[Fact]
	public async Task ShouldGenerate_WhenValidTargetFoundAndHasProperties()
	{
		const string input = """
using DudNet.Attributes;

namespace TestProject;

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
""";
		
		const string expectedProxy = """
using System.Runtime.CompilerServices;
using DudNet.Attributes;

namespace TestProject;

/// <inheritdoc cref="IPerson"/>
public partial class PersonProxy : IPerson {

	private readonly IPerson _service;

	public string? FirstName {
		get {
			Interceptor();
			get_FirstNameInterceptor();
			return _service.FirstName;
		}

		set {
			Interceptor();
			set_FirstNameInterceptor(value);
			_service.FirstName = value;
		}

	}

	public string? LastName {
		get {
			Interceptor();
			get_LastNameInterceptor();
			return _service.LastName;
		}

		set {
			Interceptor();
			set_LastNameInterceptor(value);
			_service.LastName = value;
		}

	}

	public string Id {
		get {
			Interceptor();
			get_IdInterceptor();
			return _service.Id;
		}

		set {
			Interceptor();
			set_IdInterceptor(value);
			_service.Id = value;
		}

	}

	public string FullName() {
		Interceptor();
		FullNameInterceptor();
		return _service.FullName();
	}

	partial void Interceptor([CallerMemberName]string callerName = null);

	partial void get_FirstNameInterceptor();

	partial void set_FirstNameInterceptor(string? value);

	partial void get_LastNameInterceptor();

	partial void set_LastNameInterceptor(string? value);

	partial void FullNameInterceptor();

	partial void get_IdInterceptor();

	partial void set_IdInterceptor(string value);

}
""";
		
		const string expectedDud = """
using System.Runtime.CompilerServices;
using DudNet.Attributes;

namespace TestProject;

/// <inheritdoc cref="IPerson"/>
public partial class PersonDud : IPerson {

	public string? FirstName {
		get {
			return (string?) default;
		}

		set {
		}

	}

	public string? LastName {
		get {
			return (string?) default;
		}

		set {
		}

	}

	public string Id {
		get {
			return (string) default;
		}

		set {
		}

	}

	public string FullName() {
		return (string) default;
	}

}
""";

		await new GeneratorTest
		{
			ReferenceAssemblies = Reference,
			TestState =
			{
				Sources = { input },
				GeneratedSources =
				{
					(typeof(SourceGeneratorAdapter<ProxyServiceGenerator>), "PersonProxy.g.cs", expectedProxy),
					(typeof(SourceGeneratorAdapter<ProxyServiceGenerator>), "PersonDud.g.cs", expectedDud)
				},
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(ProxyServiceGenerator).Assembly.Location)
				}
			}
		}.RunAsync();
	}
}
