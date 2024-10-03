using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
using EngineEvents;

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

  //public static async Task RegisterFeatureStarted(Guid featureId, DateTimeOffset starTime, long featureVersion, Guid correlationId)
  //{
  //  var feature = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);
  //  if (feature == null) return;

  //  RegisterEvents


  //}

  //public record FeatureStarted(Guid ContextId, Guid FeatureId, DateTimeOffset StarTime, long FeatureVersion, Guid CorrelationId) : IEngineEvents;
  //public record FeatureError(Guid ContextId, Guid FeatureId, ErrorEvent Exception) : IEngineEvents;
  //public record BpnFeatureCompleted(Guid ContextId, Guid FeatureId, DateTimeOffset EndTime, double DurationMs) : IEngineEvents;
  //public record TaskInitialized(Guid ContextId, Guid FeatureId, Guid TaskId, string Input) : IEngineEvents;
  //public record TaskFailed(Guid ContextId, Guid FeatureId, Guid TaskId, ErrorEvent Exception, double ExecutionTimeMs) : IEngineEvents;
  //public record FailedTaskReInitialized(Guid ContextId, Guid FeatureId, string NewInput, double ExecutionTimeMs) : IEngineEvents;
  //public record TaskSucceeded(Guid ContextId, Guid FeatureId, Guid TaskId, double ExecutionTimeMs) : IEngineEvents;
  //public record TransitionUsed(Guid ContextId, Guid FeatureId, Guid FromBpn, Guid ToBpn) : IEngineEvents;
  //public record TransitionSkipped(Guid ContextId, Guid FeatureId, Guid FromBpn, Guid ToBpn) : IEngineEvents;
}

public class FeatureInvocationAggregate
{
  public Guid Id { get; internal set; } = Guid.Empty;
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
    public double DurationMs { get; set; } 
    public Guid FeatureId { get; set; }
    public long FeatureVersion { get; set; }
    public List<IEngineEvents> EventLog { get; set; } = new(); 
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
    public void Apply(FeatureInvocation projection, BpnTaskInitialized @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, BpnTaskFailed @event)
    {
      EventLog.Add(@event);
      projection.Status = FeatureStatus.Failed;
    }
    public void Apply(FeatureInvocation projection, BpnFailedTaskReInitialized @event)
    {
      EventLog.Add(@event);
      projection.Status = FeatureStatus.InProgress;
    }
    public void Apply(FeatureInvocation projection, BpnTaskSucceeded @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, BpnTransitionUsed @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, BpnTransitionSkipped @event) => EventLog.Add(@event);
    public void Apply(FeatureInvocation projection, BpnFeatureCompleted @event) 
    { 
      EventLog.Add(@event);
      projection.Status = FeatureStatus.Succeeded;
      projection.EndTime = @event.EndTime;
      projection.DurationMs = @event.DurationMs;
    }
  }
}

