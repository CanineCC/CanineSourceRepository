﻿using EngineEvents;
using Marten.Events.Projections;
using System.ComponentModel.DataAnnotations;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;
using Environment = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component.Environment;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level2_Container;

public class BpnBpnWebApiContainerAggregate
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    //control endpoints/api (GPRC, RestApi)
    //Frontend is defined here as well
  }

  public Guid Id { get; internal set; }

  public void Apply(
     BpnBpnWebApiContainerAggregate aggregate,
     WebApiContainerCreated @event
  )
  {
    aggregate.Id = @event.Id;
  }
}


public class BpnBpnWebApiContainerProjection : MultiStreamProjection<BpnBpnWebApiContainerProjection.BpnWebApiContainer, Guid>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet("BpnEngine/v1/Container/All", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
    {
      var bpnContexts = await session.Query<BpnBpnWebApiContainerProjection.BpnWebApiContainer>().ToListAsync(ct);
      return Results.Ok(bpnContexts);
    }).WithName("GetAllContainers")
      .Produces(StatusCodes.Status200OK, typeof(List<BpnBpnWebApiContainerProjection.BpnWebApiContainer>))
      .WithTags("Container");

    app.MapGet("BpnEngine/v1/Container/BySystem/{systemId}", async (HttpContext context, [FromServices] IQuerySession session, Guid systemId, CancellationToken ct) =>
      {
        var bpnContexts = await session.Query<BpnBpnWebApiContainerProjection.BpnWebApiContainer>().Where(p=>p.SystemId == systemId).ToListAsync(ct);
        return Results.Ok(bpnContexts);
      }).WithName("GetAllContainersBySystem")
      .Produces(StatusCodes.Status200OK, typeof(List<BpnBpnWebApiContainerProjection.BpnWebApiContainer>))
      .WithTags("Container");
    

    app.MapGet("BpnEngine/v1/Feature/DiagramSvg/{containerId}", async (HttpContext context, [FromServices] IQuerySession session,  Guid containerId, CancellationToken ct) =>
      {
        //TODO: /{revision} <-- kræver at vi får revision på container? eller ihvertfald at publish/versionering sker på tværs af features i en container!
        //TODO: Generate all C4 documentation "on events", so this will purely be a fetch from db instead of a rendering
        var bpnContext = await session.Query<BpnBpnWebApiContainerProjection.BpnWebApiContainer>().Where(p=>p.Id == containerId).FirstAsync(ct);
        context.Response.ContentType = "image/svg+xml"; 
        await context.Response.WriteAsync(bpnContext.C4ComponentDiagramSvg, ct);
      }).WithName("GetC4_level3DiagramSvg")
      .Produces(StatusCodes.Status200OK, typeof(string))
      .WithTags("Feature.Diagram");    
  }
  public BpnBpnWebApiContainerProjection()
  {
    Identity<WebApiContainerCreated>(x => x.Id);
    Identity<DraftFeatureCreated>(x => x.ContextId);
    Identity<FeatureReleased>(x => x.ContextId);
    Identity<EnvironmentsUpdated>(x => x.ContextId);
    Identity<DraftFeaturePurposeChanged>(x => x.ContextId);
    Identity<BpnFeatureStarted>(x => x.ContextId);
    Identity<BpnFeatureError>(x => x.ContextId);
    Identity<BpnFeatureCompleted>(x => x.ContextId);
  }
  public static void Apply(BpnWebApiContainer view, IEvent<WebApiContainerCreated> @event)
  {
    view.Id = @event.Data.Id;
    view.SystemId = @event.Data.SystemId;
    view.SystemName = @event.Data.SystemName;
    view.Name = @event.Data.Name;
    view.Description = @event.Data.Descrption;
    view.CreatedTimestamp = @event.Timestamp;
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnWebApiContainer view, IEvent<DraftFeatureCreated> @event)
  {
    view.Features.Add(new FeatureDetails(
      Id: @event.Data.FeatureId,
      Revisions: new List<FeatureRevisions>() {
        new FeatureRevisions(
            Name: @event.Data.Name,
            Objective: @event.Data.Objective,
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
    var diagram = new C4ComponentDiagram(view.SystemName, view,view.Features.ToArray());
    view.C4ComponentDiagramSvg = C4DiagramHelper.GenerateC4(diagram);
    
  }
  public static void Apply(BpnWebApiContainer view, IEvent<FeatureReleased> @event)
  {
    var entry = view.Features.FirstOrDefault(entry => entry.Id == @event.Data.FeatureId);
    if (entry != null)
    {
      view.Features.Remove(entry);
      DateTimeOffset? dateTimeOffset = null;
      entry.Revisions.Add(new FeatureRevisions(
        Name: @event.Data.Name, 
        Objective: @event.Data.Objective,
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
  public static void Apply(BpnWebApiContainer view, IEvent<EnvironmentsUpdated> @event)
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
  public static void Apply(BpnWebApiContainer view, IEvent<DraftFeaturePurposeChanged> @event)
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
  public static void Apply(BpnWebApiContainer view, IEvent<BpnFeatureStarted> @event)
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
  public static void Apply(BpnWebApiContainer view, IEvent<BpnFeatureError> @event)
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
  public static void Apply(BpnWebApiContainer view, IEvent<BpnFeatureCompleted> @event)
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
  public class FeatureRevisions(string Name, long Revision, string Objective, Environment[] Environments, FeatureStats Stats)
  {
    [Required]
    public string Name { get; set; } = Name;
    [Required]
    public string Objective { get; set; } = Objective;
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
  public class BpnWebApiContainer
  {
    [Required] public Guid Id { get; set; } = Guid.Empty;
    [Required] public Guid SystemId { get; set; } = Guid.Empty;
    [Required] public string SystemName { get; set; } = "";
    [Required] public string Name { get; set; } = "";
    [Required] public string Description { get; set; } = "";

    [Required] public string C4ComponentDiagramSvg { get; set; } = "<svg/>";
    [Required] public DateTimeOffset LastUpdatedTimestamp { get; set; }
    [Required] public DateTimeOffset CreatedTimestamp { get; set; }
    [Required] public List<FeatureDetails> Features { get; set; } = [];
    public BpnWebApiContainer() { }

    //Person/Customer for documentation (including relation to context)
    //Relations between contexts/containers - https://c4model.com/diagrams/container
  }
//TODO: ServiceContainer (named configuration)

}