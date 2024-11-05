namespace CanineSourceRepositoryTest.Snippets.AutoMapperGenerator;

public class GivenComplexInputAndOutputDefinition
{
  private readonly RecordDefinition inputDefinition;
  private readonly RecordDefinition outputDefinition;
  private readonly RecordDefinition outputDetailDefinition;
  private readonly RecordDefinition inputDetailDefinition;
  public GivenComplexInputAndOutputDefinition()
  {
    inputDefinition = new RecordDefinition("Input",
            new DataDefinition("FirstName", "string"),
            new DataDefinition("Age", "long"),
            new DataDefinition("Addresses", "string", IsCollection: true),
            new DataDefinition("Details", "InputDetail", IsCollection: true)
          );

    outputDefinition = new RecordDefinition("Output",
        new DataDefinition("First_Name", "string"),
        new DataDefinition("Age", "string"), 
        new DataDefinition("Addresses", "string", IsCollection: true), 
        new DataDefinition("Details", "OutputDetail", IsCollection:true)
    );

    inputDetailDefinition = new RecordDefinition("InputDetail",
        new DataDefinition("Street", "string"),
        new DataDefinition("Number", "string")
    );
    outputDetailDefinition = new RecordDefinition("OutputDetail",
        new DataDefinition("Street", "string"),
        new DataDefinition("Number", "long")
    );
  }


  [Fact]
  public void WhenGenerateMapping_ExpectAllFieldsToBeMapped()
  {
    //ARRANGE
    //ACT
    var generatedCode = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.Snippets.AutoMapperGenerator.GenerateMapping(inputDefinition, outputDefinition, [inputDetailDefinition, outputDetailDefinition, inputDefinition, outputDefinition]);

    //ASSERT
    Assert.Contains("First_Name = input.FirstName", generatedCode);
    Assert.Contains("Age = Convert.ToString(input.Age, CultureInfo.InvariantCulture)", generatedCode);
    Assert.Contains("Addresses = input.Addresses", generatedCode);
    Assert.Contains("Details = input.Details.Select(item => new OutputDetail() with", generatedCode);
    Assert.Contains("Street = item.Street", generatedCode);
    Assert.Contains("Number = Convert.ToInt64(item.Number, CultureInfo.InvariantCulture)", generatedCode);
  }
  [Fact]
  public void WhenGenerateReverseMapping_ExpectAllFieldsToBeMapped()
  {
    //ARRANGE
    //ACT
    var generatedCode = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.Snippets.AutoMapperGenerator.GenerateMapping(outputDefinition, inputDefinition, [inputDetailDefinition, outputDetailDefinition, inputDefinition, outputDefinition]);

    //ASSERT
    Assert.Contains("FirstName = input.First_Name", generatedCode);
    Assert.Contains("Age = Convert.ToInt64(input.Age, CultureInfo.InvariantCulture)", generatedCode);
    Assert.Contains("Addresses = input.Addresses", generatedCode);
    Assert.Contains("Details = input.Details.Select(item => new InputDetail() with", generatedCode);
    Assert.Contains("Street = item.Street", generatedCode);
    Assert.Contains("Number = Convert.ToString(item.Number, CultureInfo.InvariantCulture)", generatedCode);
  }

}