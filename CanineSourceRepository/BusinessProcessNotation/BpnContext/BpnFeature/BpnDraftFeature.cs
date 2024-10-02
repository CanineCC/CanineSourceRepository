using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnDraftFeatureProjection.BpnDraftFeature;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureDiagram;

namespace CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;


public class BpnDraftFeatureAggregate
{
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
    aggregate.Tasks = aggregate.Tasks.Remove(@event.Task);
    Diagram.BpnPositions.RemoveAll(p => p.Id == @event.Task.Id);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTransitionAdded @event)
  {
    aggregate.Transitions = aggregate.Transitions.Add(@event.Transition);
    Diagram.BpnConnectionWaypoints.Add(new ConnectionWaypoints(@event.Transition.FromBPN, @event.Transition.ToBPN, []));
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTransitionRemoved @event)
  {
    aggregate.Transitions = aggregate.Transitions.Remove(@event.Transition);
    Diagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.Transition.FromBPN && p.ToBPN == @event.Transition.ToBPN);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureReset @event)
  {
    aggregate.Tasks = @event.Tasks;
    aggregate.Transitions = @event.Transitions;
    aggregate.Diagram = @event.Diagram;
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, BpnFeatureDiagram.DraftFeatureDiagramPositionUpdated @event)
  {
    aggregate.Diagram.BpnPositions.RemoveAll(p => p.Id == @event.Position.Id);
    aggregate.Diagram.BpnPositions.Add(@event.Position);
  }
  public void Apply(BpnDraftFeatureAggregate aggregate, BpnFeatureDiagram.DraftFeatureDiagramWaypointUpdated @event)
  {
    aggregate.Diagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.Waypoint.FromBPN && p.ToBPN == @event.Waypoint.ToBPN);
    aggregate.Diagram.BpnConnectionWaypoints.Add(@event.Waypoint);
  }
}

public class BpnDraftFeatureProjection : SingleStreamProjection<BpnDraftFeatureProjection.BpnDraftFeature>
{
  public class BpnDraftFeature
  {
    public record DraftFeatureCreated(Guid ContextId, Guid FeatureId, string Name, string Objective, string FlowOverview);
    public record DraftFeaturePurposeChanged(Guid ContextId, Guid FeatureId, string Name, string Objective, string FlowOverview);
    public record DraftFeatureReset(ImmutableList<BpnTask> Tasks, ImmutableList<BpnTransition> Transitions, BpnFeatureDiagram  Diagram);
    public record DraftFeatureTaskAdded(BpnTask Task);
    public record DraftFeatureTaskRemoved(BpnTask Task);
    public record DraftFeatureTransitionAdded(BpnTransition Transition);
    public record DraftFeatureTransitionRemoved(BpnTransition Transition);
    public Guid Id { get; internal set; }
    public BpnFeatureDiagram Diagram { get; internal set; } = new BpnFeatureDiagram();
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

    public ImmutableList<BpnTask> Tasks { get; internal set; } = [];
    public ImmutableList<BpnTransition> Transitions { get; internal set; } = [];
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
      projection.Tasks = projection.Tasks.Remove(@event.Task);
      Diagram.BpnPositions.RemoveAll(p => p.Id == @event.Task.Id);
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTransitionAdded @event)
    {
      projection.Transitions = projection.Transitions.Add(@event.Transition);
      Diagram.BpnConnectionWaypoints.Add(new ConnectionWaypoints(@event.Transition.FromBPN, @event.Transition.ToBPN, []));
    }
    public void Apply(BpnDraftFeature projection, DraftFeatureTransitionRemoved @event)
    {
      projection.Transitions = projection.Transitions.Remove(@event.Transition);
      Diagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.Transition.FromBPN && p.ToBPN == @event.Transition.ToBPN);
    }
    public static void Apply(BpnDraftFeature projection, IEvent<DraftFeatureReset> @event)
    {
      projection.Tasks = @event.Data.Tasks;
      projection.Transitions = @event.Data.Transitions;
      projection.Diagram = @event.Data.Diagram;

    }
    public void Apply(BpnDraftFeature projection, BpnFeatureDiagram.DraftFeatureDiagramPositionUpdated @event)
    {
      projection.Diagram.BpnPositions.RemoveAll(p => p.Id == @event.Position.Id);
      projection.Diagram.BpnPositions.Add(@event.Position);
    }
    public void Apply(BpnDraftFeature projection, BpnFeatureDiagram.DraftFeatureDiagramWaypointUpdated @event)
    {
      projection.Diagram.BpnConnectionWaypoints.RemoveAll(p => p.FromBPN == @event.Waypoint.FromBPN && p.ToBPN == @event.Waypoint.ToBPN);
      projection.Diagram.BpnConnectionWaypoints.Add(@event.Waypoint);
    }
  }
}