namespace CanineSourceRepositoryTest.BpnDiagram;

public class GivenAComplexFeature
{
  private readonly DraftFeatureComponentAggregate _featureComponent;
  public GivenAComplexFeature()
  {
    var entryBlock = new ApiInputTask("Create user endpoint", ["Anonymous"]);
    entryBlock = (entryBlock.AddRecordType(new BpnTask.RecordDefinition("Api",
      new BpnTask.DataDefinition("Name", "string")
      )) as ApiInputTask)!;
    entryBlock.Input = "Api";

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
    createUserBlock.Input = "Input";
    createUserBlock.Output = "Output";
    createUserBlock.Code = @$"
    var userId = Guid.CreateVersion7();
    //TODO: Add the user to the user database
    return new Output(userId, input.Name, input.Accessscope);
    ";

    var logUserBlock = new CodeTask("Log user");
    logUserBlock = (logUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Input",
      new BpnTask.DataDefinition("Id", "Guid"),
      new BpnTask.DataDefinition("Name", "string")
      )) as CodeTask)!;
    logUserBlock.Input = "Input";
    logUserBlock.Code = @$"
    Console.WriteLine(input.Id.ToString() + input.Name);
    ";


    var transition = new BpnTransition(
      entryBlock.Id,
      createUserBlock.Id,
      "Call Accepted",
      "input.Name != string.Empty",
      new MapField("input.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new MapField("input.Name ?? \"Anonymous\"", "Accessscope")
      );
    var logTransition = new BpnTransition(
      createUserBlock.Id,
      logUserBlock.Id,
      "Log info",
      "true",
      new MapField("output.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new MapField("output.Id", "Id")
      );

    _featureComponent = new DraftFeatureComponentAggregate();
    _featureComponent.Apply(_featureComponent, new DraftFeatureCreated(Guid.Empty, Guid.CreateVersion7(), "Test diagram", "An objective", "An overview"));
    _featureComponent.Apply(_featureComponent, new DraftFeatureTaskAdded(entryBlock));
    _featureComponent.Apply(_featureComponent, new DraftFeatureTaskAdded(createUserBlock));
    _featureComponent.Apply(_featureComponent, new DraftFeatureTaskAdded(logUserBlock));

    _featureComponent.Apply(_featureComponent, new DraftFeatureTransitionAdded(transition));
    _featureComponent.Apply(_featureComponent, new DraftFeatureTransitionAdded(logTransition));

    //BpnFeatureAggregate.Apply(feature, new BpnFeature.EnvironmentsUpdated(feature.Id, [BpnFeature.Environment.Testing]));

    //BpnFeatureRepository.Add(feature.NewRevision("me"));
  }


  //[Fact]
  //public void SaveDiagramAsBusinessProcessNotation()
  //{
  //  //ARRANGE
  //  //ACT
  //  BpnRepository.Save();
  //  //ASSERT
  //}
  //[Fact]
  //public void FeatureAsCode()
  //{
  //  //ARRANGE
  //  //ACT
  //  var cSharp = feature.ToCode();

  //  //ASSERT
  //  Assert.NotNull(cSharp);
  //  Assert.NotEmpty(cSharp);
  //}

  [Fact]
  public void Expect_FeatureIsValid()
  {
    //ARRANGE
    //ACT
    var result = _featureComponent.IsValid();

    //ASSERT
    Assert.True(result.IsValid, result.InvalidReason);
  }

}
