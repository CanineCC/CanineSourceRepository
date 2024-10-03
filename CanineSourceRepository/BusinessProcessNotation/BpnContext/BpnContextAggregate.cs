using EngineEvents;
using Marten.Events.Projections;


namespace CanineSourceRepository.BusinessProcessNotation.BpnContext;

public class BpnContextAggregate
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
  }

  public Guid Id { get; internal set; }

  public void Apply(
     BpnContextAggregate aggregate,
     ContextCreated @event
  )
  {
    aggregate.Id = @event.Id;
  }
}


public class BpnContextProjection : MultiStreamProjection<BpnContextProjection.BpnContext, Guid>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet($"BpnEngine/v1/Context/All", async (HttpContext context, [FromServices] IDocumentSession session, CancellationToken ct) =>
    {
      var bpnContexts = await session.Query<BpnContextProjection.BpnContext>().ToListAsync(ct);
      return Results.Ok(bpnContexts);
    }).WithName("GetAllContexts")
      .Produces(StatusCodes.Status200OK, typeof(BpnContextProjection.BpnContext))
      .WithTags("Context");
  }
  public BpnContextProjection()
  {
    Identity<ContextCreated>(x => x.Id);
    Identity<DraftFeatureCreated>(x => x.ContextId);
    Identity<FeatureReleased>(x => x.ContextId);
    Identity<EnvironmentsUpdated>(x => x.ContextId);
    Identity<DraftFeaturePurposeChanged>(x => x.ContextId);
    Identity<BpnFeatureStarted>(x => x.ContextId);
    Identity<BpnFeatureError>(x => x.ContextId);
    Identity<BpnFeatureCompleted>(x => x.ContextId);
  }
  public static void Apply(BpnContext view, IEvent<ContextCreated> @event)
  {
    view.Id = @event.Data.Id;
    view.Name = @event.Data.Name;
    view.CreatedTimestamp = @event.Timestamp;
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnContext view, IEvent<DraftFeatureCreated> @event)
  {
    view.Features.Add(new FeatureDetails(
      Id: @event.Data.FeatureId,
      Versions: new List<FeatureVersion>() {
        new FeatureVersion(
            Name: @event.Data.Name,
            Version: -1,
            Environments: [],
            Stats : new FeatureStats(
              InvocationCount: 0,
              InvocationErrorCount: 0,
              InvocationCompletedCount: 0,
              InvocationsInProgressCount: 0,
              TotalDurationMs: 0,
              MaxDurationMs: 0,
              MinDurationMs: 0, 
              AvgDurationMs: 0,
              LastUsed: null)
            )
      }
    ));
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnContext view, IEvent<FeatureReleased> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      view.Features.Remove(entry);
      entry.Versions.Add(new FeatureVersion(
        Name: @event.Data.Name, 
        Version: @event.Data.Version, 
        Environments: [],
        Stats: new FeatureStats(
              InvocationCount: 0,
              InvocationErrorCount: 0,
              InvocationCompletedCount: 0,
              InvocationsInProgressCount: 0,
              TotalDurationMs: 0,
              MaxDurationMs: 0,
              MinDurationMs: 0,
              AvgDurationMs: 0,
              LastUsed: null)));
      view.Features.Add(entry);
    }
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnContext view, IEvent<EnvironmentsUpdated> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var version = entry.Versions.FirstOrDefault(ver => ver.Version == @event.Data.FeatureVersion);
      if (version == null) return;
      version.Environments = @event.Data.Environment;
    }
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnContext view, IEvent<DraftFeaturePurposeChanged> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var version = entry.Versions.FirstOrDefault(ver => ver.Version == -1);
      if (version == null) return;

      version.Name = @event.Data.Name;
    }
    view.LastUpdatedTimestamp = @event.Timestamp;
  }


  public static void Apply(BpnContext view, IEvent<BpnFeatureStarted> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var version = entry.Versions.FirstOrDefault(ver => ver.Version == @event.Data.FeatureVersion);
      if (version == null) return;

      version.Stats.InvocationCount++;
      version.Stats.LastUsed = @event.Timestamp;
      version.Stats.InvocationsInProgressCount = version.Stats.InvocationCount - version.Stats.InvocationErrorCount - version.Stats.InvocationCompletedCount;
    }
  }
  public static void Apply(BpnContext view, IEvent<BpnFeatureError> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var version = entry.Versions.FirstOrDefault(ver => ver.Version == @event.Data.FeatureVersion);
      if (version == null) return;

      version.Stats.InvocationErrorCount++;
      version.Stats.InvocationsInProgressCount = version.Stats.InvocationCount - version.Stats.InvocationErrorCount - version.Stats.InvocationCompletedCount;
    }
  }
  public static void Apply(BpnContext view, IEvent<BpnFeatureCompleted> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var version = entry.Versions.FirstOrDefault(ver => ver.Version == @event.Data.FeatureVersion);
      if (version == null) return;

      version.Stats.InvocationCompletedCount++;
      version.Stats.InvocationsInProgressCount = version.Stats.InvocationCount - version.Stats.InvocationErrorCount - version.Stats.InvocationCompletedCount;
      version.Stats.MaxDurationMs = Math.Max(version.Stats.MaxDurationMs, @event.Data.DurationMs);
      version.Stats.MinDurationMs = version.Stats.MinDurationMs == 0 ? @event.Data.DurationMs : Math.Min(version.Stats.MinDurationMs, @event.Data.DurationMs);
      version.Stats.TotalDurationMs += (decimal)@event.Data.DurationMs;
      version.Stats.AvgDurationMs = (double)(version.Stats.TotalDurationMs / version.Stats.InvocationCompletedCount);
    }
  }

  public class FeatureStats(long InvocationCount, long InvocationErrorCount, long InvocationCompletedCount, long InvocationsInProgressCount, decimal TotalDurationMs, double MaxDurationMs, double AvgDurationMs, double MinDurationMs, DateTimeOffset? LastUsed)
  {
    public long InvocationCount { get; set; } = InvocationCount;
    public long InvocationErrorCount { get; set; } = InvocationErrorCount;
    public long InvocationCompletedCount { get; set; } = InvocationCompletedCount;
    public long InvocationsInProgressCount { get; set; } = InvocationsInProgressCount;
    public decimal TotalDurationMs { get; set; } = TotalDurationMs;
    public double MaxDurationMs { get; set; } = MaxDurationMs;
    public double AvgDurationMs { get; set;  } = AvgDurationMs;
    public double MinDurationMs { get; set; } = MinDurationMs;
    public DateTimeOffset? LastUsed { get; set; } = LastUsed;
  }

  public class FeatureVersion(string Name, long Version, BpnFeature.Environment[] Environments, FeatureStats Stats)
  {
    public string Name { get; set; } = Name;
    public long Version { get; set; } = Version;
    public BpnFeature.Environment[] Environments { get; set; } = Environments;
    public FeatureStats Stats { get; set; } = Stats;
  }

  public class FeatureDetails(Guid Id, List<FeatureVersion> Versions)
  {
    public Guid Id { get; set; } = Id;
    public List<FeatureVersion> Versions { get; set; } = Versions;
  }

  public class BpnContext
  {
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = "";
    public DateTimeOffset LastUpdatedTimestamp { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public List<FeatureDetails> Features { get; set; } = [];
    public BpnContext() { }



  }
}