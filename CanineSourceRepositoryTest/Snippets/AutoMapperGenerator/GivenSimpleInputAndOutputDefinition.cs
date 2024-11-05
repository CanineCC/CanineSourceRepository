namespace CanineSourceRepositoryTest.Snippets.AutoMapperGenerator;

public class GivenSimpleInputAndOutputDefinition
{
  private readonly RecordDefinition inputDefinition;
  private readonly RecordDefinition outputDefinition;
  public GivenSimpleInputAndOutputDefinition()
  {
    inputDefinition = new RecordDefinition("Input",
              new DataDefinition("FirstName", "string"),
              new DataDefinition("Age", "long"),
              new DataDefinition("Salary", "decimal"),
              new DataDefinition("Birthdate", "DateTimeOffset"),
              new DataDefinition("UniqueID", "Guid")
          );

    outputDefinition = new RecordDefinition("Output",
        new DataDefinition("First_Name", "string"),
        new DataDefinition("Age", "string"), // Different type for conversion
        new DataDefinition("Salary", "decimal"),
        new DataDefinition("Birthdate", "string"), // Different type for conversion
        new DataDefinition("UniqueID", "Guid")
    );
  }


  [Fact]
  public void WhenGenerateMapping_ExpectAllFieldsToBeMapped()
  {
    //ARRANGE
    //ACT
    var generatedCode = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.Snippets.AutoMapperGenerator.GenerateMapping(inputDefinition, outputDefinition, [inputDefinition, outputDefinition]);

    //ASSERT
    Assert.Contains("First_Name = input.FirstName", generatedCode);
    Assert.Contains("Age = Convert.ToString(input.Age, CultureInfo.InvariantCulture)", generatedCode);
    Assert.Contains("Salary = input.Salary", generatedCode);
    Assert.Contains("Birthdate = Convert.ToString(input.Birthdate, CultureInfo.InvariantCulture)", generatedCode);
    Assert.Contains("UniqueID = input.UniqueID", generatedCode);
  }
  [Fact]
  public void WhenGenerateReverseMapping_ExpectAllFieldsToBeMapped()
  {
    //ARRANGE
    //ACT
    var generatedCode = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.Snippets.AutoMapperGenerator.GenerateMapping(outputDefinition, inputDefinition, [inputDefinition, outputDefinition]);

    //ASSERT
    Assert.Contains("FirstName = input.First_Name", generatedCode);
    Assert.Contains("Age = Convert.ToInt64(input.Age, CultureInfo.InvariantCulture)", generatedCode);
    Assert.Contains("Salary = input.Salary", generatedCode);
    Assert.Contains("Birthdate = DateTimeOffset.Parse(input.Birthdate, CultureInfo.InvariantCulture)", generatedCode);
    Assert.Contains("UniqueID = input.UniqueID", generatedCode);
  }

}