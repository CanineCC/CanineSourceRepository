using EngineEvents;
using Marten.Events.Projections;
using System.ComponentModel.DataAnnotations;
using Environment = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component.Environment;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level2_Container;

public class BpnContextAggregate
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    //control endpoints/api (GPRC, RestApi)
    //Frontend is defined here as well
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
    app.MapGet($"BpnEngine/v1/Context/All", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
    {
      var bpnContexts = await session.Query<BpnContextProjection.BpnContext>().ToListAsync(ct);
      return Results.Ok(bpnContexts);
    }).WithName("GetAllContexts")
      .Produces(StatusCodes.Status200OK, typeof(List<BpnContextProjection.BpnContext>))
      .WithTags("Container");
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
      Revisions: new List<FeatureRevisions>() {
        new FeatureRevisions(
            Name: @event.Data.Name,
            Revision: -1,
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
              LastUsed: null,
              Published: @event.Timestamp)
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
      DateTimeOffset? dateTimeOffset = null;
      entry.Revisions.Add(new FeatureRevisions(
        Name: @event.Data.Name, 
        Revision: @event.Data.Revision, 
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
              LastUsed: dateTimeOffset,
              Published: @event.Timestamp)));
      view.Features.Add(entry);
    }
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnContext view, IEvent<EnvironmentsUpdated> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var revision = entry.Revisions.FirstOrDefault(ver => ver.Revision == @event.Data.FeatureRevision);
      if (revision == null) return;
      revision.Environments = @event.Data.Environment;
    }
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnContext view, IEvent<DraftFeaturePurposeChanged> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var revision = entry.Revisions.FirstOrDefault(ver => ver.Revision == -1);
      if (revision == null) return;

      revision.Name = @event.Data.Name;
    }
    view.LastUpdatedTimestamp = @event.Timestamp;
  }


  public static void Apply(BpnContext view, IEvent<BpnFeatureStarted> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var revision = entry.Revisions.FirstOrDefault(ver => ver.Revision == @event.Data.FeatureRevision);
      if (revision == null) return;

      revision.Stats.InvocationCount++;
      revision.Stats.LastUsed = @event.Timestamp;
      revision.Stats.InvocationsInProgressCount = revision.Stats.InvocationCount - revision.Stats.InvocationErrorCount - revision.Stats.InvocationCompletedCount;
    }
  }
  public static void Apply(BpnContext view, IEvent<BpnFeatureError> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var revision = entry.Revisions.FirstOrDefault(ver => ver.Revision == @event.Data.FeatureRevision);
      if (revision == null) return;

      revision.Stats.InvocationErrorCount++;
      revision.Stats.InvocationsInProgressCount = revision.Stats.InvocationCount - revision.Stats.InvocationErrorCount - revision.Stats.InvocationCompletedCount;
    }
  }
  public static void Apply(BpnContext view, IEvent<BpnFeatureCompleted> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var revision = entry.Revisions.FirstOrDefault(ver => ver.Revision == @event.Data.FeatureRevision);
      if (revision == null) return;

      revision.Stats.InvocationCompletedCount++;
      revision.Stats.InvocationsInProgressCount = revision.Stats.InvocationCount - revision.Stats.InvocationErrorCount - revision.Stats.InvocationCompletedCount;
      revision.Stats.MaxDurationMs = Math.Max(revision.Stats.MaxDurationMs, @event.Data.DurationMs);
      revision.Stats.MinDurationMs = revision.Stats.MinDurationMs == 0 ? @event.Data.DurationMs : Math.Min(revision.Stats.MinDurationMs, @event.Data.DurationMs);
      revision.Stats.TotalDurationMs += (decimal)@event.Data.DurationMs;
      revision.Stats.AvgDurationMs = (double)(revision.Stats.TotalDurationMs / revision.Stats.InvocationCompletedCount);
    }
  }

  public class FeatureStats(long InvocationCount, long InvocationErrorCount, long InvocationCompletedCount, long InvocationsInProgressCount, decimal TotalDurationMs, double MaxDurationMs, double AvgDurationMs, double MinDurationMs, DateTimeOffset? LastUsed, DateTimeOffset Published)
  {
    [Required]
    public long InvocationCount { get; set; } = InvocationCount;
    [Required]
    public long InvocationErrorCount { get; set; } = InvocationErrorCount;
    [Required]
    public long InvocationCompletedCount { get; set; } = InvocationCompletedCount;
    [Required]
    public long InvocationsInProgressCount { get; set; } = InvocationsInProgressCount;
    [Required]
    public decimal TotalDurationMs { get; set; } = TotalDurationMs;
    [Required]
    public double MaxDurationMs { get; set; } = MaxDurationMs;
    [Required]
    public double AvgDurationMs { get; set;  } = AvgDurationMs;
    [Required]
    public double MinDurationMs { get; set; } = MinDurationMs;
    public DateTimeOffset? LastUsed { get; set; } = LastUsed;
    [Required]
    public DateTimeOffset Published { get; set; } = Published;
  }

  public class FeatureRevisions(string Name, long Revision, Environment[] Environments, FeatureStats Stats)
  {
    [Required]
    public string Name { get; set; } = Name;
    [Required]
    public long Revision { get; set; } = Revision;
    [Required]
    public Environment[] Environments { get; set; } = Environments;
    [Required]
    public FeatureStats Stats { get; set; } = Stats;
  }

  public class FeatureDetails(Guid Id, List<FeatureRevisions> Revisions)
  {
    [Required]
    public Guid Id { get; set; } = Id;
    [Required]
    public List<FeatureRevisions> Revisions { get; set; } = Revisions;
  }

  public class BpnContext
  {
    [Required]
    public Guid Id { get; set; } = Guid.Empty;
    [Required]
    public string Name { get; set; } = "";

   // [Required]
    //public long Revision { get; internal set; } = 0;

    [Required]
    public DateTimeOffset LastUpdatedTimestamp { get; set; }
    [Required]
    public DateTimeOffset CreatedTimestamp { get; set; }
    [Required]
    public List<FeatureDetails> Features { get; set; } = [];
    public BpnContext() { }



  }
}