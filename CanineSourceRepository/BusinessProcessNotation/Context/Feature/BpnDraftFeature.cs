using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnDraftFeatureProjection.BpnDraftFeature;

namespace CanineSourceRepository.BusinessProcessNotation.Context.Feature;


public class BpnDraftFeatureAggregate
{
  public Guid Id { get; internal set; }
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
  public (bool Valid, string Reason) IsValid()
  {
    if (Tasks.Count == 0) return (false, "No nodes");
    if (Tasks.First().GetType() != typeof(ApiInputTask)) return (false, $"First node can not be {Tasks.First().GetType()}");
    if (Transitions.Count == 0) return (false, "No connections");
    if (OrphanElements().Count > 0) return (false, "Orphan elements: " + string.Join(',', OrphanElements().Select(p => p.Name)));

    var compileResult = DynamicCompiler.CompileCode(BpnFeatureProjection.BpnFeature.ToCode(Tasks, Transitions));
    if (compileResult.errors.Any()) return (false, "Compiler errors: " + string.Join("\r\n", compileResult.errors.Select(err => $"Line:{err.LineNumber}, Column:{err.ColumnNumber}, Message:{err.ErrorMessage}")));
    return (true, "");
  }

  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureCreated @event)
  {
    aggregate.Id = @event.FeatureId;
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

  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTaskAdded @event) => aggregate.Tasks = aggregate.Tasks.Add(@event.Task);
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTaskRemoved @event) => aggregate.Tasks = aggregate.Tasks.Remove(@event.Task);
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTransitionAdded @event) => aggregate.Transitions = aggregate.Transitions.Add(@event.Transition);
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureTransitionRemoved @event) => aggregate.Transitions = aggregate.Transitions.Remove(@event.Transition);
  public void Apply(BpnDraftFeatureAggregate aggregate, DraftFeatureReset @event)
  {
    aggregate.Tasks = @event.Tasks;
    aggregate.Transitions = @event.Transitions;
  }

}

public class BpnDraftFeatureProjection : SingleStreamProjection<BpnDraftFeatureProjection.BpnDraftFeature>
{
  public class BpnDraftFeature
  {
    public record DraftFeatureCreated(Guid FeatureId, string Name, string Objective, string FlowOverview);
    public record DraftFeaturePurposeChanged(string Name, string Objective, string FlowOverview);
    public record DraftFeatureReset(ImmutableList<BpnTask> Tasks, ImmutableList<BpnTransition> Transitions);
    public record DraftFeatureTaskAdded(BpnTask Task);
    public record DraftFeatureTaskRemoved(BpnTask Task);
    public record DraftFeatureTransitionAdded(BpnTransition Transition);
    public record DraftFeatureTransitionRemoved(BpnTransition Transition);
    public Guid Id { get; internal set; }
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
    public void Apply(BpnDraftFeature projection, DraftFeatureTaskAdded @event) => projection.Tasks = projection.Tasks.Add(@event.Task);
    public void Apply(BpnDraftFeature projection, DraftFeatureTaskRemoved @event) => projection.Tasks = projection.Tasks.Remove(@event.Task);
    public void Apply(BpnDraftFeature projection, DraftFeatureTransitionAdded @event) => projection.Transitions = projection.Transitions.Add(@event.Transition);
    public void Apply(BpnDraftFeature projection, DraftFeatureTransitionRemoved @event) => projection.Transitions = projection.Transitions.Remove(@event.Transition);
    public static void Apply(BpnDraftFeature projection, IEvent<DraftFeatureReset> @event)
    {
      projection.Tasks = @event.Data.Tasks;
      projection.Transitions = @event.Data.Transitions;
    }
  }
}