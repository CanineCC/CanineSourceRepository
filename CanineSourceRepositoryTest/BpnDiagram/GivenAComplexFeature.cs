﻿using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnDraftFeatureProjection;

namespace CanineSourceRepositoryTest.BpnDiagram;

public class GivenAComplexFeature
{
  private readonly BpnDraftFeatureAggregate feature;
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


    var transition = new BpnTransition(
      entryBlock.Id,
      createUserBlock.Id,
      "Call Accepted",
      "input.Name != string.Empty",
      new Map("input.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new Map("input.Name ?? \"Anonymous\"", "Accessscope")
      );
    var logTransition = new BpnTransition(
      createUserBlock.Id,
      logUserBlock.Id,
      "Log info",
      "true",
      new Map("output.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new Map("output.Id", "Id")
      );

    feature = new BpnDraftFeatureAggregate();
    feature.Apply(feature, new BpnDraftFeature.DraftFeatureCreated(Guid.Empty, Guid.CreateVersion7(), "Test diagram", "An objective", "An overview"));
    feature.Apply(feature, new BpnDraftFeature.DraftFeatureTaskAdded(entryBlock));
    feature.Apply(feature, new BpnDraftFeature.DraftFeatureTaskAdded(createUserBlock));
    feature.Apply(feature, new BpnDraftFeature.DraftFeatureTaskAdded(logUserBlock));

    feature.Apply(feature, new BpnDraftFeature.DraftFeatureTransitionAdded(transition));
    feature.Apply(feature, new BpnDraftFeature.DraftFeatureTransitionAdded(logTransition));

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
    var result = feature.IsValid();

    //ASSERT
    Assert.True(result.IsValid, result.InvalidReason);
  }

}
