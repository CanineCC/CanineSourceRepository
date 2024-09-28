﻿using EngineEvents;

namespace CanineSourceRepository.BusinessProcessNotation.Engine;
public static class FeatureInovationEventStore
{
  public static async Task RegisterEvents(this IDocumentSession session, CancellationToken ct, Guid id, Guid causationId, params IEngineEvents[] @events)
  {
    session.CorrelationId = id.ToString("N");
    session.CausationId = causationId.ToString("N");

    await session.Events.WriteToAggregate<FeatureInvocationAggregate>(
            id,
            stream => stream .AppendMany(@events),
            ct);
    await session.SaveChangesAsync();  
  }
}

public class FeatureInvocationAggregate
{
  public Guid Id { get; internal set; } = Guid.Empty;
  public void Apply(
       FeatureInvocationAggregate aggregate,
       FeatureStarted @event
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
    public double DurationMs { get; set; } 
    public Guid FeatureId { get; set; }
    public long FeatureVersion { get; set; }
    public List<IEngineEvents> EventLog { get; set; } = new(); //TODO: Consider NOT using events, instead keep the states "i.e. 'createUserTask, status=success'", instead of an initialized+success event
    public FeatureStatus Status { get; set; } = FeatureStatus.Undefined;
    public FeatureInvocation() { }

    public void Apply(FeatureInvocation projection, FeatureStarted @event)
    {
      projection.Id = @event.CorrelationId;
      projection.StarTime = @event.StarTime;
      projection.FeatureVersion = @event.FeatureVersion;
      projection.FeatureId = @event.FeatureId;
      projection.Status = FeatureStatus.InProgress;
      EventLog.Add(@event);
    }
    public void Apply(FeatureInvocation projection, FeatureError @event)
    {
      EventLog.Add(@event);
      projection.Status = FeatureStatus.Failed;
    }
    public void Apply(FeatureInvocation projection, TaskInitialized @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, TaskFailed @event)
    {
      EventLog.Add(@event);
      projection.Status = FeatureStatus.Failed;
    }
    public void Apply(FeatureInvocation projection, FailedTaskReInitialized @event)
    {
      EventLog.Add(@event);
      projection.Status = FeatureStatus.InProgress;
    }
    public void Apply(FeatureInvocation projection, TaskSucceeded @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, TransitionUsed @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, TransitionSkipped @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, BpnFeatureCompleted @event) 
    { 
      EventLog.Add(@event);
      projection.Status = FeatureStatus.Succeeded;
      projection.EndTime = @event.EndTime;
      projection.DurationMs = @event.DurationMs;
    }
  }
}
