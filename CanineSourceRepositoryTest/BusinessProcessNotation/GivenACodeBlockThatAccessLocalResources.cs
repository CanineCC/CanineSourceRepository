using CanineSourceRepository.BusinessProcessNotation;
using System.Reflection;

namespace CanineSourceRepositoryTest.BusinessProcessNotation;

public class GivenACodeBlockThatAccessLocalResources
{
  private readonly CodeBlock block;
  public GivenACodeBlockThatAccessLocalResources()
  {
    block = new CodeBlock("Use FileIO");
    block.AddRecordType(new Bpn.RecordDefinition("Output", new Bpn.DataDefinition("Text", "string")));
    block.AddRecordType(new Bpn.RecordDefinition("Input"));
    block = block with
    {
      Input = "Input",
      Output = "Output",
      Code = "return new Output(System.IO.File.ReadAllText(\"test.txt\"));"
    };
  }


  [Fact]
  public void WhenExecutingWithValidJson_ExpectFailure()
  {
    //ARRANGE
    //ACT
    //ASSERT
    Assert.Throws<InvalidOperationException>(() => block.ToAssembly());
  }


  [Fact]
  public void WhenVerifyCode_ExpectFailure()
  {
    //ARRANGE
    //ACT
    var (_, success) = block.VerifyCode();

    //ASSERT
    Assert.False(success);
  }


  [Fact]
  public void WhenVerifyInputWithValidJson_ExpectFailure()
  {
    //ARRANGE
    var jsonInput = "{}";

    //ACT
    //ASSERT
    Assert.Throws<InvalidOperationException>(() => block.VerifyInputData(jsonInput, block.ToAssembly()));
  }
}