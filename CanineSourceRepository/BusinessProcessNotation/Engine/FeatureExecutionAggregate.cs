using EngineEvents;
using Marten;
using Marten.Events.Aggregation;

namespace CanineSourceRepository.BusinessProcessNotation.Engine;


public static class FeatureInovationFeatures
{
  public static async Task RegisterEvent(this IDocumentSession session, Guid id, IEvent @event, CancellationToken ct)
  {
    await session.Events.WriteToAggregate<FeatureInvocationAggregate>(
            id,
            stream => stream.AppendOne(@event),
            ct);
  }
  public static async Task RegisterEvents(this IDocumentSession session, CancellationToken ct, Guid id, params IEvent[] @events)
  {
    await session.Events.WriteToAggregate<FeatureInvocationAggregate>(
            id,
            stream => stream .AppendMany(@events),
            ct);
  }
}

public class FeatureInvocationAggregate
{
  public Guid Id { get; set; } = Guid.Empty;
  public int Version { get; set; } = 0;
  public void Apply(
       FeatureInvocationAggregate aggregate,
       BpnFeatureStarted @event
   )
  {
    aggregate.Id = @event.CorrelationId;
  }
}
public class FeatureInvocationProjection : SingleStreamProjection<FeatureInvocationProjection.FeatureInvocation>
{
  public class FeatureInvocation
  {
    public enum FeatureStatus { Undefined, InProgress, Succeeded, Failed }
    public Guid Id { get; set; } = Guid.Empty;
    public DateTimeOffset StarTime { get; set; }
    public DateTimeOffset? EndTime { get; set; } = null;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public Guid FeatureId { get; set; }
    public long FeatureVersion { get; set; }
    public List<IEvent> EventLog { get; set; } = new();
    public FeatureStatus Status { get; set; } = FeatureStatus.Undefined;
    public FeatureInvocation() { }

    public void Apply(FeatureInvocation projection, BpnFeatureStarted @event)
    {
      projection.Id = @event.CorrelationId;
      projection.StarTime = @event.StarTime;
      projection.FeatureVersion = @event.FeatureVersion;
      projection.FeatureId = @event.FeatureId;
      projection.Status = FeatureStatus.InProgress;
      EventLog.Add(@event);
    }
    public void Apply(FeatureInvocation projection, BpnFeatureError @event)
    {
      EventLog.Add(@event);
      projection.Status = FeatureStatus.Failed;
    }
    public void Apply(FeatureInvocation projection, NodeInitialized @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, NodeFailed @event)
    {
      EventLog.Add(@event);
      projection.Status = FeatureStatus.Failed;
    }
    public void Apply(FeatureInvocation projection, FailedNodeReInitialized @event)
    {
      EventLog.Add(@event);
      projection.Status = FeatureStatus.InProgress;
    }
    public void Apply(FeatureInvocation projection, NodeSucceeded @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, ConnectionUsed @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, ConnectionSkipped @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, BpnFeatureCompleted @event) 
    { 
      EventLog.Add(@event);
      projection.Status = FeatureStatus.Succeeded;
      projection.EndTime = @event.EndTime;
      projection.Duration = @event.Duration;
    }
  }
}

