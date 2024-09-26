using CanineSourceRepository.BusinessProcessNotation;
using System.Reflection;

namespace CanineSourceRepositoryTest.BusinessProcessNotation;

public class GivenAValidHelloWorldCodeBlock
{
  private readonly CodeBlock block;
  private readonly Assembly assembly;
  public GivenAValidHelloWorldCodeBlock()
  {
    block = new CodeBlock("Generate hello world");
    block = (block.AddRecordType(new Bpn.RecordDefinition("Output", new Bpn.DataDefinition("Greeting", "string"))) as CodeBlock)!;
    block = (block.AddRecordType(new Bpn.RecordDefinition("Input", new Bpn.DataDefinition("Greet", "string"), new Bpn.DataDefinition("Name", "string"))) as CodeBlock)!;
    block = block with { Input = "Input", Output = "Output", Code = "return new Output(input.Greet + ' ' + input.Name);" };


    block.AddTestCase(new TestCase(
      Name: "Unit test",
      Input: new { Greet = "Hello", Name = "John" },
      new AssertDefinition("Greeting", AssertOperation.Equal, "Hello John")
      ));
    assembly = block.ToAssembly();
  }

  [Fact]
  public async Task WhenExecutingWithValidJson_ExpectSuccess()
  {
    //ARRANGE
    var input = new { Greet = "Hello", Name = "world" };

    //ACT
    var result = await block.Execute(input, null, assembly);

    //ASSERT
    Assert.Equal("Hello world", result?.Greeting);
  }

  [Fact]
  public async Task WhenExecutingWithInvalidJson_ExpectFailure()
  {
    //ARRANGE
    var input = new { Greet = "Hello", wrongField = "world" };

    //ACT
    var result = await block.Execute(input, null, assembly);

    //ASSERT
    Assert.NotEqual("Hello world", result?.Greeting);
  }

  [Fact]
  public void WhenVerifyCode_ExpectSuccess()
  {
    //ARRANGE
    //ACT
    var (_, success) = block.VerifyCode();

    //ASSERT
    Assert.True(success);
  }


  [Fact]
  public void WhenVerifyInputWithValidJson_ExpectSuccess()
  {
    //ARRANGE
    var input = new { Greet = "Hello", Name = "world" };

    //ACT
    var (IsOk, _) = block.VerifyInputData(input, assembly);

    //ASSERT
    Assert.True(IsOk);
  }

  [Fact]
  public void WhenVerifyInputWithInvalidJson_ExpectFailure()
  {
    //ARRANGE
    var input = new { Greet = "Hello", wrongField = "world" };


    //ACT
    var (IsOk, _) = block.VerifyInputData(input, assembly);

    //ASSERT
    Assert.False(IsOk);
  }

  [Fact]
  public async Task WhenRunningUnitTest_ExpectSuccess()
  {
    //ARRANGE
    //ACT
    var testResult = await block.RunTests(null, assembly);
    var anyUnsuccessful = testResult.Any(p => p.Success == false);

    //ASSERT
    Assert.False(anyUnsuccessful);

  }
}
