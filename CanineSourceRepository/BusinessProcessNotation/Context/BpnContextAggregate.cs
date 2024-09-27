namespace CanineSourceRepository.BusinessProcessNotation.Context;


public static class BpnContextEventStore
{
  public static async Task RegisterEvents(this IDocumentSession session, CancellationToken ct, Guid id, Guid causationId, params IEvent[] @events)
  {
    session.CorrelationId = id.ToString("N");
    session.CausationId = causationId.ToString("N");

    await session.Events.WriteToAggregate<BpnContextAggregate>(
            id,
            stream => stream.AppendMany(@events),
            ct);
  }
}

public class BpnContextAggregate
{
  public Guid Id { get; internal set; }

  public void Apply(
     BpnContextAggregate aggregate,
     BpnContextCreated @event
  )
  {
    aggregate.Id = @event.Id;
  }
}

public class BpnContextProjection : SingleStreamProjection<BpnContextProjection.BpnContext>
{
  public class BpnContext
  {
    public Guid Id { get; set; } = Guid.Empty;
    public DateTimeOffset LastUpdatedTimestamp { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public List<Guid> FeatureIds { get; set; } = [];
    public BpnContext() { }

    public void Apply(BpnContext projection, IEvent<BpnContextCreated> @event)
    {
      projection.Id = @event.Data.Id;
      projection.LastUpdatedTimestamp = @event.Timestamp;
      projection.CreatedTimestamp = @event.Timestamp;
    }

    public void Apply(BpnContext projection, IEvent<FeatureAddedToBpnContext> @event)
    {
      projection.LastUpdatedTimestamp = @event.Timestamp;
      projection.FeatureIds.Add( @event.Data.FeatureId );
    }
    public void Apply(BpnContext projection, IEvent<FeatureRemovedFromBpnContext> @event)
    {
      projection.LastUpdatedTimestamp = @event.Timestamp;
      projection.FeatureIds.Remove(@event.Data.FeatureId);
    }
  }
}

public record BpnContextCreated(Guid Id, string Name);
public record FeatureAddedToBpnContext(Guid BpnContextId, Guid FeatureId);
public record FeatureRemovedFromBpnContext(Guid BpnContextId, Guid FeatureId);
