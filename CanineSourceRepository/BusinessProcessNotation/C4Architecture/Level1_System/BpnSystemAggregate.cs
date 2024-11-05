using EngineEvents;
using Marten.Events.Projections;
using System.ComponentModel.DataAnnotations;
using Environment = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component.Environment;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

public class BpnSystemAggregate
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


public class BpnSystemProjection : MultiStreamProjection<BpnSystemProjection.BpnSystem, Guid>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet($"BpnEngine/v1/System/All", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
    {
      var bpnContexts = await session.Query<BpnContextProjection.BpnContext>().ToListAsync(ct);
      return Results.Ok(bpnContexts);
    }).WithName("GetAllSystems")
      .Produces(StatusCodes.Status200OK, typeof(List<BpnContextProjection.BpnContext>))
      .WithTags("System");
  }
  public BpnSystemProjection()
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
  /*
  public static void Apply(BpnSystem view, IEvent<ContextCreated> @event)
  {
    view.Id = @event.Data.Id;
    view.Name = @event.Data.Name;
    view.CreatedTimestamp = @event.Timestamp;
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnSystem view, IEvent<DraftFeatureCreated> @event)
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
*//*
  public static void Apply(BpnSystem view, IEvent<BpnFeatureCompleted> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      var version = entry.Revisions.FirstOrDefault(ver => ver.Revision == @event.Data.FeatureRevision);
      if (version == null) return;

      version.Stats.InvocationCompletedCount++;
      version.Stats.InvocationsInProgressCount = version.Stats.InvocationCount - version.Stats.InvocationErrorCount - version.Stats.InvocationCompletedCount;
      version.Stats.MaxDurationMs = Math.Max(version.Stats.MaxDurationMs, @event.Data.DurationMs);
      version.Stats.MinDurationMs = version.Stats.MinDurationMs == 0 ? @event.Data.DurationMs : Math.Min(version.Stats.MinDurationMs, @event.Data.DurationMs);
      version.Stats.TotalDurationMs += (decimal)@event.Data.DurationMs;
      version.Stats.AvgDurationMs = (double)(version.Stats.TotalDurationMs / version.Stats.InvocationCompletedCount);
    }
  }*/

  public class ContextStats(long InvocationCount, long InvocationErrorCount, long InvocationCompletedCount, long InvocationsInProgressCount, decimal TotalDurationMs, double MaxDurationMs, double AvgDurationMs, double MinDurationMs, DateTimeOffset? LastUsed)
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
    [Required]
    public DateTimeOffset? LastUsed { get; set; } = LastUsed;
  }


  public class ContextVersion(string Name, long Version, Environment[] Environments, ContextStats Stats)
  {
    [Required]
    public string Name { get; set; } = Name;
    //[Required]
    //public long Revision { get; set; } = Revision;
    [Required]
    public Environment[] Environments { get; set; } = Environments;
    [Required]
    public ContextStats Stats { get; set; } = Stats;
  }
  public class ContextDetails(Guid Id, List<ContextVersion> revisions)
  {
    [Required]
    public Guid Id { get; set; } = Id;
    [Required]
    public List<ContextVersion> Revisions { get; set; } = revisions;
  }

  public class BpnSystem
  {
    [Required]
    public Guid Id { get; set; } = Guid.Empty;
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public DateTimeOffset LastUpdatedTimestamp { get; set; }
    [Required]
    public DateTimeOffset CreatedTimestamp { get; set; }
    [Required]
    public List<ContextDetails> Features { get; set; } = [];
    public BpnSystem() { }
  }
}