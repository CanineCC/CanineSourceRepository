using CanineSourceRepository.BusinessProcessNotation;
using System.Reflection;

namespace CanineSourceRepositoryTest.BusinessProcessNotation;

public class GivenACodeBlockUsingAllBasicTypes
{
  private readonly Assembly assembly;
  private readonly CodeBlock block;
  public GivenACodeBlockUsingAllBasicTypes()
  {
    block = new CodeBlock("Check Types");
    block = (block.AddRecordType(new Bpn.RecordDefinition("Output", new Bpn.DataDefinition("Success", "bool"))) as CodeBlock)!;
    block = (block.AddRecordType(new Bpn.RecordDefinition("Input",
        new Bpn.DataDefinition("Text", "string"),
        new Bpn.DataDefinition("YesNo", "bool"),
        new Bpn.DataDefinition("Number", "long"),
        new Bpn.DataDefinition("Fraction", "decimal"),
        new Bpn.DataDefinition("StartDateTime", "DateTimeOffset"),
        new Bpn.DataDefinition("StartDate", "DateOnly"),
        new Bpn.DataDefinition("StartTime", "TimeOnly")
        //        new BPN.DataDefinition("Blob", "byte[]") -- base64 encoded... helper functions needed
        )) as CodeBlock)!;
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
    var result = await block.Execute(jsonInput, null, assembly);

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
