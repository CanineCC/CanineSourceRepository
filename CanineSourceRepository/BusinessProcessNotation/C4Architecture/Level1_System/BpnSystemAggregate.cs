using Marten.Events.Projections;
using System.ComponentModel.DataAnnotations;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSystem;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

public class BpnSystemAggregate
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
  }

  public Guid Id { get; internal set; }

  public void Apply(
    BpnSystemAggregate aggregate,
     CreateSystemFeature.SystemCreated @event
  )
  {
    aggregate.Id = @event.Id;
  }
  //WebApiContainerCreated
}


public class BpnSystemProjection : MultiStreamProjection<BpnSystemProjection.BpnSystem, Guid>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet($"BpnEngine/v1/System/All", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
    {
      var bpnContexts = await session.Query<BpnSystem>().ToListAsync(ct);
      return Results.Ok(bpnContexts);
    }).WithName("GetAllSystems")
      .Produces(StatusCodes.Status200OK, typeof(List<BpnSystem>))
      .WithTags("System");
    
    app.MapGet($"BpnEngine/v1/System/DiagramSvg", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
      {
        var bpnContexts = await session.Query<BpnSystem>().ToListAsync(ct);
        var diagram = new C4SystemDiagram("TODO", bpnContexts.ToArray());
        var svg = C4DiagramHelper.GenerateC4(diagram);

        context.Response.ContentType = "image/svg+xml"; 
        await context.Response.WriteAsync(svg, ct);
      }).WithName("GetC4_Level1DiagramSvg")
      .Produces(StatusCodes.Status200OK, typeof(string))
      .WithTags("System");
  }
  public BpnSystemProjection()
  {
    Identity<CreateSystemFeature.SystemCreated>(x => x.Id);
    Identity<WebApiContainerCreated>(x => x.SystemId);
  }
  public static void Apply(BpnSystem view, IEvent<CreateSystemFeature.SystemCreated> @event)
  {
    view.Id = @event.Data.Id;
    view.Name = @event.Data.Name;
    view.CreatedTimestamp = @event.Timestamp;
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnSystem view, IEvent<WebApiContainerCreated> @event)
  {
    view.Contexts.Add(new ContextDetails(@event.Data.Id, @event.Data.Name));
  }
  /*
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


  
  public class ContextDetails(Guid Id, string Name)
  {
    [Required]
    public Guid Id { get; set; } = Id;
    [Required]
    public string Name { get; set; } = Name;
  }

  public class BpnSystem
  {//versioning?/timestamp?
    [Required]
    public Guid Id { get; set; } = Guid.Empty;
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Description { get; set; } = "";
    
    [Required]
    public DateTimeOffset CreatedTimestamp { get; set; }
    [Required]
    public DateTimeOffset LastUpdatedTimestamp { get; set; }
    [Required]
    public List<ContextDetails> Contexts { get; set; } = [];
    //Relations to other systems? (maybe determin by: messages sendt and received? + services called)
    //Person/Customer for documentation (including relation to system) - https://c4model.com/diagrams/system-context
    public BpnSystem() { }
  }
}