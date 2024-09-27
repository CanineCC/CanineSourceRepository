using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.Engine;
using System.Reflection;

namespace CanineSourceRepositoryTest.BusinessProcessNotation;

public class GivenACodeBlockUsingAllBasicTypes
{
  private readonly Assembly assembly;
  private readonly CodeTask block;
  public GivenACodeBlockUsingAllBasicTypes()
  {
    block = new CodeTask("Check Types");
    block = (block.AddRecordType(new BpnTask.RecordDefinition("Output", new BpnTask.DataDefinition("Success", "bool"))) as CodeTask)!;
    block = (block.AddRecordType(new BpnTask.RecordDefinition("Input",
        new BpnTask.DataDefinition("Text", "string"),
        new BpnTask.DataDefinition("YesNo", "bool"),
        new BpnTask.DataDefinition("Number", "long"),
        new BpnTask.DataDefinition("Fraction", "decimal"),
        new BpnTask.DataDefinition("StartDateTime", "DateTimeOffset"),
        new BpnTask.DataDefinition("StartDate", "DateOnly"),
        new BpnTask.DataDefinition("StartTime", "TimeOnly")
        //        new BPN.DataDefinition("Blob", "byte[]") -- base64 encoded... helper functions needed
        )) as CodeTask)!;
    block = block with { Input = "Input", Output = "Output", Code = "return new Output(true);" };
    assembly = block.ToAssembly();
  }


  [Fact]
  public async Task WhenExecutingWithValidJson_ExpectSuccess()
  {
    //ARRANGE
    var jsonInput =
"""
{
    "Text": "Hello",
    "YesNo": true,
    "Number": 120,
    "Fraction": 120.2,
    "StartDateTime": "2024-07-14T13:52:02+02:00",
    "StartDate": "2024-07-14",
    "StartTime": "13:52:02"
}
""";

    //ACT
    var result = await block.Execute(jsonInput, new NoService(), assembly);

    //ASSERT
    Assert.NotNull(result);
    Assert.True(result!.Success);
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
    var jsonInput =
"""
{
    "Text": "Hello",
    "YesNo": true,
    "Number": 120,
    "Fraction": 120.2,
    "StartDateTime": "2024-07-14T13:52:02+02:00",
    "StartDate": "2024-07-14",
    "StartTime": "13:52:02"
}
""";

    //ACT
    var (IsOk, _) = block.VerifyInputData(jsonInput, assembly);

    //ASSERT
    Assert.True(IsOk);
  }

  [Fact]
  public void WhenVerifyInputWithInvalidJson_ExpectFailure()
  {
    //ARRANGE
    var jsonInput =
"""
{
    "greet": "Hello",
    "notCorrectName": "world"
}
""";

    //ACT
    var (IsOk, _) = block.VerifyInputData(jsonInput, assembly);

    //ASSERT
    Assert.False(IsOk);
  }
}
