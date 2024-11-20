using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.TaskFeatures.AddTestCaseToTaskFeature;
using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.TaskFeatures.RemoveTestCaseFromTaskFeature;
using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.TaskFeatures.UpdateTestCaseOnTaskFeature;
using static CanineSourceRepository.DynamicCompiler;
using BpnTask = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.BpnTask;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component;


public class DraftFeatureComponentAggregate
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    //MOVE TO FEATURE!
    app.MapPost($"BpnEngine/v1/Task/VerifyCodeBlock", (HttpContext context, [FromServices] IDocumentSession session, [FromBody] BpnTask codeTask, CancellationToken ct) =>
    {
      var res = codeTask.VerifyCode();
      if (res.success)
        return Results.Accepted();

      return Results.BadRequest(res.errors);
    }).WithName("VerifyCodeBlock")
      .Produces(StatusCodes.Status202Accepted)
      .Produces(StatusCodes.Status400BadRequest, typeof(CompileError))
      .WithTags("DraftFeature");


    //MOVE TO FEATURE!
    app.MapPost($"BpnEngine/v1/Task/GetSnippetsForCodeBlock", (HttpContext context, [FromServices] IDocumentSession session, [FromBody] BpnTask codeTask, CancellationToken ct) =>
    {
      var input = codeTask.RecordTypes.FirstOrDefault(p => p.Name == codeTask.Input);
      var output = codeTask.RecordTypes.FirstOrDefault(p => p.Name == codeTask.Output);

      var snippets = new List<CodeSnippet>();
      if (input != null && output != null)
      {
        snippets.AddRange([
          new CodeSnippet("Auto construct output", AutoConstructorGenerator.GenerateMapping(input, output, codeTask.RecordTypes.ToArray())),
          new CodeSnippet("Auto mapper", AutoMapperGenerator.GenerateMapping(input, output, codeTask.RecordTypes.ToArray()))
         
        ]);
      }
    }).WithName("GetSnippetsForCodeBlock")
      .Produces(StatusCodes.Status200OK, typeof(List<CodeSnippet>))
      .WithTags("DraftFeature");
  }

  public Guid Id { get; internal set; }
  public Guid ContainerId { get; internal set; }
  public FeatureComponentDiagram ComponentDiagram { get; internal set; } = new FeatureComponentDiagram();
  public ImmutableList<BpnTask> Tasks { get; internal set; } = [];
  public ImmutableList<BpnTransition> Transitions { get; internal set; } = [];
  public string Name { get; internal set; } = string.Empty;
  /// <summary>
  /// Describe the business purpose of the entire feature in business terms, not technical ones.
  /// </summary>
  /// <example>
  /// Enable users to register, validate their email, and gain access to premium content.
  /// </example>
  public string Objective { get; internal set; } = string.Empty;
  /// <summary>
  /// A high-level description of the business process from start to finish. 
  /// </summary>
  /// <example>
  /// The user enters their registration details, verifies their email, and is granted access to restricted areas.
  /// </example>
  public string FlowOverview { get; internal set; } = string.Empty;

  private List<BpnTask> OrphanElements()
  {
    return Tasks.Where(node => Transitions.Where(c => c.FromBPN == node.Id || c.ToBPN == node.Id).Any() == false).ToList();
  }
  public ValidationResponse IsValid()
  {
    if (Tasks.Count == 0) return new ValidationResponse(false, "No nodes", ResultCode.BadRequest);
   // if (Tasks.First().GetType() != typeof(ApiInputTask)) return new ValidationResponse(false, $"First node can not be {Tasks.First().GetType()}", ResultCode.BadRequest);
    if (Transitions.Count == 0) return new ValidationResponse(false, "No connections", ResultCode.BadRequest);
    if (OrphanElements().Count > 0) return new ValidationResponse(false, "Orphan elements: " + string.Join(',', OrphanElements().Select(p => p.Name)), ResultCode.BadRequest);

    var compileResult = DynamicCompiler.CompileCode(FeatureComponentProjection.BpnFeature.ToCode(Tasks, Transitions));
    if (compileResult.errors.Any()) return new ValidationResponse(false, "Compiler errors: " + string.Join("\r\n", compileResult.errors.Select(err => $"Line:{err.LineNumber}, Column:{err.ColumnNumber}, Message:{err.ErrorMessage}")), ResultCode.BadRequest);
    return new ValidationResponse(true, "", ResultCode.NoContent);
  }

  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeatureCreated @event)
  {
    componentAggregate.Id = @event.FeatureId;
    componentAggregate.ContainerId = @event.ContainerId;
    componentAggregate.Name = @event.Name;
    componentAggregate.Objective = @event.Objective;
    componentAggregate.FlowOverview = @event.FlowOverview;
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeaturePurposeChanged @event)
  {
    componentAggregate.Name = @event.Name;
    componentAggregate.Objective = @event.Objective;
    componentAggregate.FlowOverview = @event.FlowOverview;
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeatureTaskAdded @event)
  {
    var serviceType = ServiceType.ServiceTypes.First(p => p.Id == @event.ServiceTypeId);
    componentAggregate.Tasks = componentAggregate.Tasks.Add(
      new BpnTask(@event.Name)
      {
        Input = @event.Input, 
        Output = @event.Output, 
        BehavioralGoal = @event.BehavioralGoal, 
        BusinessPurpose = @event.BusinessPurpose, 
        Code = @event.Code, 
        Id=@event.TaskId,
        NamedConfiguration = @event.NamedConfigurationName,
        NamedConfigurationId = @event.NamedConfigurationId,
        ServiceDependency = serviceType.InjectedComponent.Name,
        ServiceDependencyId = serviceType.Id,
        TestCases = [],
        RecordTypes = @event.RecordTypes.Select(rt => 
          new RecordDefinition(rt.Name, rt.Fields.Select(rf=> new DataDefinition(rf.Name, rf.Type, rf.IsCollection, rf.IsMandatory)).ToArray()
          )
        ).ToImmutableList()
      });
    ComponentDiagram.BpnPositions.Add(new BpnPosition(@event.TaskId, new Position(0, 0)));
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeatureTaskRemoved @event)
  {
    componentAggregate.Tasks = componentAggregate.Tasks.RemoveAll(task => task.Id == @event.TaskId);
    ComponentDiagram.BpnPositions.RemoveAll(p => p.Id == @event.TaskId);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeatureTransitionAdded @event)
  {
    componentAggregate.Transitions = componentAggregate.Transitions.Add(@event.Transition);
    ComponentDiagram.BpnConnectionWaypoints.Add(new ConnectionWaypoints(@event.Transition.FromBPN, @event.Transition.ToBPN, []));
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeatureTransitionRemoved @event)
  {
    componentAggregate.Transitions = componentAggregate.Transitions.RemoveAll(p => p.FromBPN == @event.FromBpn && p.ToBPN == @event.ToBpn);
    ComponentDiagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.FromBpn && p.ToBPN == @event.ToBpn);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeatureReset @event)
  {
    componentAggregate.Tasks = @event.Tasks;
    componentAggregate.Transitions = @event.Transitions;
    componentAggregate.ComponentDiagram = @event.ComponentDiagram;
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeatureDiagramPositionUpdated @event)
  {
    componentAggregate.ComponentDiagram.BpnPositions.RemoveAll(p => p.Id == @event.Position.Id);
    componentAggregate.ComponentDiagram.BpnPositions.Add(@event.Position);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, DraftFeatureDiagramWaypointUpdated @event)
  {
    componentAggregate.ComponentDiagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.Waypoint.FromBPN && p.ToBPN == @event.Waypoint.ToBPN);
    componentAggregate.ComponentDiagram.BpnConnectionWaypoints.Add(@event.Waypoint);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, TaskPurposeUpdated @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.Name = @event.Name;
    task.BehavioralGoal = @event.BehavioralGoal;
    task.BusinessPurpose = @event.BusinessPurpose;
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, RecordAddedToTask @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.RecordTypes = task.RecordTypes.Add(@event.RecordDefinition);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, RecordUpdatedOnTask @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.RecordTypes = task.RecordTypes.RemoveAt(@event.RecordIndex).Add(@event.RecordDefinition);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, RecordDeletedOnTask @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.RecordTypes = task.RecordTypes.RemoveAll(record => record.Name == @event.Name);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, CodeUpdatedOnTask @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.Code = @event.Code;
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, ServiceDependencyUpdated @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.ServiceDependency = @event.ServiceDependency;
    task.NamedConfiguration = @event.NamedConfiguration;
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, TestCaseAddedToTaskBody @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.UpsertTestCase(@event.TestCase);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, TestCaseRemovedFromTask @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.RemoveTestCase(@event.AssertionId);
  }
  public void Apply(DraftFeatureComponentAggregate componentAggregate, TestCaseUpdatedOnTaskBody @event)
  {
    var task = componentAggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.UpsertTestCase(@event.TestCase);
  }

}

public class DraftFeatureComponentProjection : SingleStreamProjection<DraftFeatureComponentProjection.BpnDraftFeature>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet("BpnEngine/v1/DraftFeature/{featureId}", async (HttpContext context, [FromServices] IQuerySession session, Guid featureId, CancellationToken ct) =>
    {
      var bpnFeature = await session.Query<DraftFeatureComponentProjection.BpnDraftFeature>().Where(p => p.Id == featureId).SingleOrDefaultAsync();
      if (bpnFeature == null) return Results.NotFound();
      return Results.Ok(bpnFeature);
    }).WithName("GetDraftFeature")
      .Produces(StatusCodes.Status200OK, typeof(DraftFeatureComponentProjection.BpnDraftFeature))
      .WithTags("DraftFeature");
  }
  public class BpnDraftFeature
  {
    [Required]
    public Guid Id { get; set; }
    [Required]
    public FeatureComponentDiagram ComponentDiagram { get; set; } = new FeatureComponentDiagram();
    [Required]
    public string Name { get;  set; } = string.Empty;
    /// <summary>
    /// Describe the business purpose of the entire feature in business terms, not technical ones.
    /// </summary>
    /// <example>
    /// Enable users to register, validate their email, and gain access to premium content.
    /// </example>
    [Required]
    public string Objective { get;  set; } = string.Empty;
    /// <summary>
    /// A high-level description of the business process from start to finish. 
    /// </summary>
    /// <example>
    /// The user enters their registration details, verifies their email, and is granted access to restricted areas.
    /// </example>
    [Required]
    public string FlowOverview { get;  set; } = string.Empty;

    [Required]
    public ImmutableList<BpnTask> Tasks { get;  set; } = [];
    [Required]
    public ImmutableList<BpnTransition> Transitions { get;  set; } = [];
    public BpnDraftFeature() { }
    public Assembly ToAssembly() => DynamicCompiler.PrecompileCode(FeatureComponentProjection.BpnFeature.ToCode(Tasks, Transitions));
    public void Apply(BpnDraftFeature projection, DraftFeatureCreated @event)
    {
      projection.Id = @event.FeatureId;
      projection.Name = @event.Name;
      projection.Objective = @event.Objective;
      projection.FlowOverview = @event.FlowOverview;
    }
    public void Apply(BpnDraftFeature projection, DraftFeaturePurposeChanged @event)
    {
      projection.Name = @event.Name;
      projection.Objective = @event.Objective;
      projection.FlowOverview = @event.FlowOverview;
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTaskAdded @event)
    {
      var serviceType = ServiceType.ServiceTypes.First(p => p.Id == @event.ServiceTypeId);
      
      projection.Tasks = projection.Tasks.Add(
      new BpnTask(@event.Name)
      {
        Input = @event.Input, 
        Output = @event.Output, 
        BehavioralGoal = @event.BehavioralGoal, 
        BusinessPurpose = @event.BusinessPurpose, 
        Code = @event.Code, 
        Id=@event.TaskId,
        NamedConfiguration = @event.NamedConfigurationName,
        NamedConfigurationId = @event.NamedConfigurationId,
        ServiceDependency = serviceType.InjectedComponent.Name,
        ServiceDependencyId = serviceType.Id,
        TestCases = [],
        RecordTypes = @event.RecordTypes.Select(rt => 
          new RecordDefinition(rt.Name, rt.Fields.Select(rf=> new DataDefinition(rf.Name, rf.Type, rf.IsCollection, rf.IsMandatory)).ToArray()
          )
        ).ToImmutableList()
      });      
      ComponentDiagram.BpnPositions.Add(new BpnPosition(@event.TaskId, new Position(0, 0)));
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTaskRemoved @event)
    {
      projection.Tasks = projection.Tasks.RemoveAll(task => task.Id == @event.TaskId);
      ComponentDiagram.BpnPositions.RemoveAll(p => p.Id == @event.TaskId);
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTransitionAdded @event)
    {
      projection.Transitions = projection.Transitions.Add(@event.Transition);
      ComponentDiagram.BpnConnectionWaypoints.Add(new ConnectionWaypoints(@event.Transition.FromBPN, @event.Transition.ToBPN, []));
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTransitionRemoved @event)
    {
      projection.Transitions = projection.Transitions.RemoveAll(p => p.FromBPN == @event.FromBpn && p.ToBPN == @event.ToBpn);
      ComponentDiagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.FromBpn && p.ToBPN == @event.ToBpn);
    }
    public static void Apply(BpnDraftFeature projection, IEvent<DraftFeatureReset> @event)
    {
      projection.Tasks = @event.Data.Tasks;
      projection.Transitions = @event.Data.Transitions;
      projection.ComponentDiagram = @event.Data.ComponentDiagram;

    }
    public void Apply(BpnDraftFeature projection, DraftFeatureDiagramPositionUpdated @event)
    {
      projection.ComponentDiagram.BpnPositions.RemoveAll(p => p.Id == @event.Position.Id);
      projection.ComponentDiagram.BpnPositions.Add(@event.Position);
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureDiagramWaypointUpdated @event)
    {
      projection.ComponentDiagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.Waypoint.FromBPN && p.ToBPN == @event.Waypoint.ToBPN);
      projection.ComponentDiagram.BpnConnectionWaypoints.Add(@event.Waypoint);
    }


    public void Apply(BpnDraftFeature projection, TaskPurposeUpdated @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.Name = @event.Name;
      task.BehavioralGoal = @event.BehavioralGoal;
      task.BusinessPurpose = @event.BusinessPurpose;
    }
    public void Apply(BpnDraftFeature projection, RecordAddedToTask @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.RecordTypes = task.RecordTypes.Add(@event.RecordDefinition).OrderBy(p => p.Name).ToImmutableList();
    }
    public void Apply(BpnDraftFeature projection, RecordUpdatedOnTask @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.RecordTypes = task.RecordTypes.RemoveAt(@event.RecordIndex).Add(@event.RecordDefinition).OrderBy(p=>p.Name).ToImmutableList();
    }
    public void Apply(BpnDraftFeature projection, RecordDeletedOnTask @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.RecordTypes = task.RecordTypes.RemoveAll(record => record.Name == @event.Name);
    }
    public void Apply(BpnDraftFeature projection, CodeUpdatedOnTask @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.Code = @event.Code;
    }
    public void Apply(BpnDraftFeature projection, ServiceDependencyUpdated @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.ServiceDependency = @event.ServiceDependency;
      task.NamedConfiguration = @event.NamedConfiguration;
    }
    public void Apply(BpnDraftFeature projection, TestCaseAddedToTaskBody @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.UpsertTestCase(@event.TestCase);
    }
    public void Apply(BpnDraftFeature projection, TestCaseRemovedFromTask @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.RemoveTestCase(@event.AssertionId);
    }
    public void Apply(BpnDraftFeature projection, TestCaseUpdatedOnTaskBody @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.UpsertTestCase(@event.TestCase);
    }

  }
}