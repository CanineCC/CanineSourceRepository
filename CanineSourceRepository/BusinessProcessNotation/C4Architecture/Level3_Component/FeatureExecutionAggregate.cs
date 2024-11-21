using EngineEvents;
using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component;
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
       BpnFeatureStarted @event
   )
  {
    aggregate.Id = @event.CorrelationId;
  }
}