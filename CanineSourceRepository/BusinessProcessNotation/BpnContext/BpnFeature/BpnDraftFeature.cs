using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task.Snippets;
using System.ComponentModel.DataAnnotations;
using static CanineSourceRepository.DynamicCompiler;

namespace CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;


public class BpnDraftFeatureAggregate
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/Task/VerifyCodeBlock", (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CodeTask codeTask, CancellationToken ct) =>
    {
      var res = codeTask.VerifyCode();
      if (res.success)
        return Results.Accepted();

      return Results.BadRequest(res.errors);
    }).WithName("VerifyCodeBlock")
      .Produces(StatusCodes.Status202Accepted)
      .Produces(StatusCodes.Status400BadRequest, typeof(CompileError))
      .WithTags("DraftFeature");


    app.MapPost($"BpnEngine/v1/Task/GetSnippetsForCodeBlock", (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CodeTask codeTask, CancellationToken ct) =>
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
  public Guid ContextId { get; internal set; }
  public BpnFeatureDiagram Diagram { get; internal set; } = new BpnFeatureDiagram();
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
    if (Tasks.First().GetType() != typeof(ApiInputTask)) return new ValidationResponse(false, $"First node can not be {Tasks.First().GetType()}", ResultCode.BadRequest);
    if (Transitions.Count == 0) return new ValidationResponse(false, "No connections", ResultCode.BadRequest);
    if (OrphanElements().Count > 0) return new ValidationResponse(false, "Orphan elements: " + string.Join(',', OrphanElements().Select(p => p.Name)), ResultCode.BadRequest);

    var compileResult = DynamicCompiler.CompileCode(BpnFeatureProjection.BpnFeature.ToCode(Tasks, Transitions));
    if (compileResult.errors.Any()) return new ValidationResponse(false, "Compiler errors: " + string.Join("\r\n", compileResult.errors.Select(err => $"Line:{err.LineNumber}, Column:{err.ColumnNumber}, Message:{err.ErrorMessage}")), ResultCode.BadRequest);
    return new ValidationResponse(true, "", ResultCode.NoContent);
  }

  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureCreated @event)
  {
    aggregate.Id = @event.FeatureId;
    aggregate.ContextId = @event.ContextId;
    aggregate.Name = @event.Name;
    aggregate.Objective = @event.Objective;
    aggregate.FlowOverview = @event.FlowOverview;
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeaturePurposeChanged @event)
  {
    aggregate.Name = @event.Name;
    aggregate.Objective = @event.Objective;
    aggregate.FlowOverview = @event.FlowOverview;
  }

  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTaskAdded @event)
  {
    aggregate.Tasks = aggregate.Tasks.Add(@event.Task);
    Diagram.BpnPositions.Add(new BpnPosition(@event.Task.Id, new Position(0, 0)));
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTaskRemoved @event)
  {
    aggregate.Tasks = aggregate.Tasks.RemoveAll(task => task.Id == @event.TaskId);
    Diagram.BpnPositions.RemoveAll(p => p.Id == @event.TaskId);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTransitionAdded @event)
  {
    aggregate.Transitions = aggregate.Transitions.Add(@event.Transition);
    Diagram.BpnConnectionWaypoints.Add(new ConnectionWaypoints(@event.Transition.FromBPN, @event.Transition.ToBPN, []));
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTransitionRemoved @event)
  {
    aggregate.Transitions = aggregate.Transitions.RemoveAll(p => p.FromBPN == @event.FromBpn && p.ToBPN == @event.ToBpn);
    Diagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.FromBpn && p.ToBPN == @event.ToBpn);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureReset @event)
  {
    aggregate.Tasks = @event.Tasks;
    aggregate.Transitions = @event.Transitions;
    aggregate.Diagram = @event.Diagram;
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureDiagramPositionUpdated @event)
  {
    aggregate.Diagram.BpnPositions.RemoveAll(p => p.Id == @event.Position.Id);
    aggregate.Diagram.BpnPositions.Add(@event.Position);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureDiagramWaypointUpdated @event)
  {
    aggregate.Diagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.Waypoint.FromBPN && p.ToBPN == @event.Waypoint.ToBPN);
    aggregate.Diagram.BpnConnectionWaypoints.Add(@event.Waypoint);
  }

  public void Apply(BpnDraftFeatureAggregate aggregate, TaskPurposeUpdated @event)
  {
    var task = aggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.Name = @event.Name;
    task.BehavioralGoal = @event.BehavioralGoal;
    task.BusinessPurpose = @event.BusinessPurpose;
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, RecordAddedToTask @event)
  {
    var task = aggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.RecordTypes = task.RecordTypes.Add(@event.RecordDefinition);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, RecordUpdatedOnTask @event)
  {
    var task = aggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.RecordTypes = task.RecordTypes.RemoveAt(@event.RecordIndex).Add(@event.RecordDefinition);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, RecordDeletedOnTask @event)
  {
    var task = aggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.RecordTypes = task.RecordTypes.RemoveAll(record => record.Name == @event.Name);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, CodeUpdatedOnTask @event)
  {
    var task = (CodeTask)aggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.Code = @event.Code;
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, ServiceDependencyUpdated @event)
  {
    var task = aggregate.Tasks.First(p => p.Id == @event.TaskId);
    task.ServiceDependency = @event.ServiceDependency;
    task.NamedConfiguration = @event.NamedConfiguration;
  }
}

public class BpnDraftFeatureProjection : SingleStreamProjection<BpnDraftFeatureProjection.BpnDraftFeature>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet("BpnEngine/v1/DraftFeature/{featureId}", async (HttpContext context, [FromServices] IQuerySession session, Guid featureId, CancellationToken ct) =>
    {
      var bpnFeature = await session.Query<BpnDraftFeatureProjection.BpnDraftFeature>().Where(p => p.Id == featureId).SingleOrDefaultAsync();
      if (bpnFeature == null) return Results.NotFound();

      return Results.Ok(bpnFeature);
    }).WithName("GetDraftFeature")
      .Produces(StatusCodes.Status200OK, typeof(BpnDraftFeatureProjection.BpnDraftFeature))
      .WithTags("DraftFeature");
  }
  public class BpnDraftFeature
  {
    [Required]
    public Guid Id { get; set; }
    [Required]
    public BpnFeatureDiagram Diagram { get; set; } = new BpnFeatureDiagram();
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
    public Assembly ToAssembly() => DynamicCompiler.PrecompileCode(BpnFeatureProjection.BpnFeature.ToCode(Tasks, Transitions));
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
      projection.Tasks = projection.Tasks.Add(@event.Task);
      Diagram.BpnPositions.Add(new BpnPosition(@event.Task.Id, new Position(0, 0)));
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTaskRemoved @event)
    {
      projection.Tasks = projection.Tasks.RemoveAll(task => task.Id == @event.TaskId);
      Diagram.BpnPositions.RemoveAll(p => p.Id == @event.TaskId);
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTransitionAdded @event)
    {
      projection.Transitions = projection.Transitions.Add(@event.Transition);
      Diagram.BpnConnectionWaypoints.Add(new ConnectionWaypoints(@event.Transition.FromBPN, @event.Transition.ToBPN, []));
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTransitionRemoved @event)
    {
      projection.Transitions = projection.Transitions.RemoveAll(p => p.FromBPN == @event.FromBpn && p.ToBPN == @event.ToBpn);
      Diagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.FromBpn && p.ToBPN == @event.ToBpn);
    }
    public static void Apply(BpnDraftFeature projection, IEvent<DraftFeatureReset> @event)
    {
      projection.Tasks = @event.Data.Tasks;
      projection.Transitions = @event.Data.Transitions;
      projection.Diagram = @event.Data.Diagram;

    }
    public void Apply(BpnDraftFeature projection, DraftFeatureDiagramPositionUpdated @event)
    {
      projection.Diagram.BpnPositions.RemoveAll(p => p.Id == @event.Position.Id);
      projection.Diagram.BpnPositions.Add(@event.Position);
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureDiagramWaypointUpdated @event)
    {
      projection.Diagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.Waypoint.FromBPN && p.ToBPN == @event.Waypoint.ToBPN);
      projection.Diagram.BpnConnectionWaypoints.Add(@event.Waypoint);
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
      var task = (CodeTask)projection.Tasks.First(p => p.Id == @event.TaskId);
      task.Code = @event.Code;
    }
    public void Apply(BpnDraftFeature projection, ServiceDependencyUpdated @event)
    {
      var task = projection.Tasks.First(p => p.Id == @event.TaskId);
      task.ServiceDependency = @event.ServiceDependency;
      task.NamedConfiguration = @event.NamedConfiguration;
    }
  }
}