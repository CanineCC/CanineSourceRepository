using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using System.Reflection;

namespace CanineSourceRepositoryTest.BusinessProcessNotation;

public class GivenACodeBlockThatAccessLocalResources
{
  private readonly CodeTask block;
  public GivenACodeBlockThatAccessLocalResources()
  {
    block = new CodeTask("Use FileIO");
    block.AddRecordType(new BpnTask.RecordDefinition("Output", new BpnTask.DataDefinition("Text", "string")));
    block.AddRecordType(new BpnTask.RecordDefinition("Input"));
    block.Input = "Input";
    block.Output = "Output";
    block.Code = "return new Output(System.IO.File.ReadAllText(\"test.txt\"));";
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