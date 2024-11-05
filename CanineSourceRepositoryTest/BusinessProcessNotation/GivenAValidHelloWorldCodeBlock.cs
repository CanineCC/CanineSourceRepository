using CanineSourceRepository.BusinessProcessNotation.Engine;

namespace CanineSourceRepositoryTest.BusinessProcessNotation;

public class GivenAValidHelloWorldCodeBlock
{
  private readonly CodeTask block;
  private readonly Assembly assembly;
  public GivenAValidHelloWorldCodeBlock()
  {
    block = new CodeTask("Generate hello world");
    block = (block.AddRecordType(new BpnTask.RecordDefinition("Output", new BpnTask.DataDefinition("Greeting", "string"))) as CodeTask)!;
    block = (block.AddRecordType(new BpnTask.RecordDefinition("Input", new BpnTask.DataDefinition("Greet", "string"), new BpnTask.DataDefinition("Name", "string"))) as CodeTask)!;
    block.Input = "Input";
    block.Output = "Output";
    block.Code = "return new Output(input.Greet + ' ' + input.Name);";


    block.UpsertTestCase(new TestCase(){
      Id = Guid.CreateVersion7(),
      Name = "Unit test",
      Input = """{ Greet = "Hello", Name = "John" }""",
      Asserts = [new AssertDefinition()
      {
        Field = "Greeting",
        Operation =  AssertOperation.Equal,
        ExpectedValue = "Hello John"
      }]}
      );
    assembly = block.ToAssembly();
  }

  [Fact]
  public async Task WhenExecutingWithValidJson_ExpectSuccess()
  {
    //ARRANGE
    var input = new { Greet = "Hello", Name = "world" };

    //ACT
    var result = await block.Execute(input, new NoService(), assembly);

    //ASSERT
    Assert.Equal("Hello world", result?.Greeting);
  }

  [Fact]
  public async Task WhenExecutingWithInvalidJson_ExpectFailure()
  {
    //ARRANGE
    var input = new { Greet = "Hello", wrongField = "world" };

    //ACT
    var result = await block.Execute(input, new NoService(), assembly);

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
    var testResult = await block.RunTests(new NoService(), assembly);
    var anyUnsuccessful = testResult.Any(p => p.Success == false);

    //ASSERT
    Assert.False(anyUnsuccessful);

  }
}
