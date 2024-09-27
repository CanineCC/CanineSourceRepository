using CanineSourceRepository.BusinessProcessNotation;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;

namespace CanineSourceRepositoryTest.BpnDiagram;

public class GivenAComplexFeature
{
  private readonly BpnFeature feature;
  public GivenAComplexFeature()
  {
    var entryBlock = new ApiInputTask("Create user endpoint", ["Anonymous"]);
    entryBlock = (entryBlock.AddRecordType(new BpnTask.RecordDefinition("Api",
      new BpnTask.DataDefinition("Name", "string")
      )) as ApiInputTask)!;
    entryBlock = entryBlock with
    {
      Input = "Api",
    };

    var createUserBlock = new CodeTask("Create user logic");
    createUserBlock = (createUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Output", 
      new BpnTask.DataDefinition("Id", "Guid"),
      new BpnTask.DataDefinition("Name", "string"),
      new BpnTask.DataDefinition("Accessscope", "string")
      )) as CodeTask)!;
    createUserBlock = (createUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Input", 
      new BpnTask.DataDefinition("Name", "string"),
      new BpnTask.DataDefinition("Accessscope", "string")
      )) as CodeTask)!;
    createUserBlock = createUserBlock with
    {
      Input = "Input",
      Output = "Output",
      Code = @$"
    var userId = Guid.CreateVersion7();
    //TODO: Add the user to the user database
    return new Output(userId, input.Name, input.Accessscope);
    "
    };

    var logUserBlock = new CodeTask("Log user");
    logUserBlock = (logUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Input",
      new BpnTask.DataDefinition("Id", "Guid"),
      new BpnTask.DataDefinition("Name", "string")
      )) as CodeTask)!;
    logUserBlock = logUserBlock with
    {
      Input = "Input",
      Code = @$"
    Console.WriteLine(input.Id.ToString() + input.Name);
    "
    };


    var transition = new Transition(
      entryBlock.Id,
      createUserBlock.Id,
      "Call Accepted",
      "input.Name != string.Empty",
      new Map("input.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new Map("input.Name ?? \"Anonymous\"", "Accessscope")
      );
    var logTransition = new Transition(
      createUserBlock.Id,
      logUserBlock.Id,
      "Log info",
      "true",
      new Map("output.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new Map("output.Id", "Id")
      );

    feature = BpnFeature.CreateNew("Test diagram", [entryBlock, createUserBlock, logUserBlock], [transition, logTransition], [BpnFeature.Environment.Development, BpnFeature.Environment.Testing]);
    BpnFeatureRepository.Add(feature.NewRevision("me"));
  }


  //[Fact]
  //public void SaveDiagramAsBusinessProcessNotation()
  //{
  //  //ARRANGE
  //  //ACT
  //  BpnRepository.Save();
  //  //ASSERT
  //}
  [Fact]
  public void FeatureAsCode()
  {
    //ARRANGE
    //ACT
    var cSharp = feature.ToCode();

    //ASSERT
    Assert.NotNull(cSharp);
    Assert.NotEmpty(cSharp);
  }

  [Fact]
  public void Expect_FeatureIsValid()
  {
    //ARRANGE
    //ACT
    var result = feature.IsValid();

    //ASSERT
    Assert.True(result.Valid, result.Reason);
  }

}
