namespace CanineSourceRepository.BusinessProcessNotation.Context;

public class BpnContextAggregate
{
  public Guid Id { get; internal set; }

  public void Apply(
     BpnContextAggregate aggregate,
     BpnContextProjection.BpnContext.ContextCreated @event
  )
  {
    aggregate.Id = @event.Id;
  }
}

public class BpnContextProjection : SingleStreamProjection<BpnContextProjection.BpnContext>
{
  public class BpnContext
  {
    public record ContextCreated(Guid Id, string Name);
    public record FeatureAddedToContext(Guid FeatureId);
   // public record FeatureRemoved(Guid FeatureId);

    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = "";
    public DateTimeOffset LastUpdatedTimestamp { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public List<Guid> FeatureIds { get; set; } = [];
    public BpnContext() { }

    public void Apply(BpnContext projection, IEvent<ContextCreated> @event)
    {
      projection.Id = @event.Data.Id;
      projection.Name = @event.Data.Name;
      projection.LastUpdatedTimestamp = @event.Timestamp;
      projection.CreatedTimestamp = @event.Timestamp;
    }

    public void Apply(BpnContext projection, IEvent<FeatureAddedToContext> @event)
    {
      projection.LastUpdatedTimestamp = @event.Timestamp;
      projection.FeatureIds.Add( @event.Data.FeatureId );
    }
    //public void Apply(BpnContext projection, IEvent<FeatureRemoved> @event)
    //{
    //  projection.LastUpdatedTimestamp = @event.Timestamp;
    //  projection.FeatureIds.Remove(@event.Data.FeatureId);
    //}
  }
}