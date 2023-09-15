using DudNet.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace DudNet.Tests.Generation;

using GeneratorTest =
	Microsoft.CodeAnalysis.CSharp.Testing.CSharpSourceGeneratorTest<SourceGeneratorAdapter<ProxyServiceGenerator>,
		Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

public class ProxyServiceGeneratorTests
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
using DudNet.Attributes;

namespace TestProject;

public class ExampleServiceDud : IExampleService {

	public void ExampleFunction() {}

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
	
}
