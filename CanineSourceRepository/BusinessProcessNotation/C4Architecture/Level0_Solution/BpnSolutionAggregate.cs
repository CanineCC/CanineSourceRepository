﻿using Marten.Events.Projections;
using System.ComponentModel.DataAnnotations;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSolution;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSystem;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

public class BpnSolutionAggregate
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
  }

  public Guid Id { get; internal set; }

  public void Apply(
    BpnSolutionAggregate aggregate,
    CreateSolutionFeature.SolutionCreated @event
  )
  {
    aggregate.Id = @event.Id;
  }
}

public class BpnSolutionProjection : MultiStreamProjection<BpnSolutionProjection.BpnSolution, Guid>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet($"BpnEngine/v1/Solution/All", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
    {
      var bpnContexts = await session.Query<BpnSolution>().ToListAsync(ct);
      return Results.Ok(bpnContexts);
    }).WithName("GetAllSolutions")
      .Produces(StatusCodes.Status200OK, typeof(List<BpnSolution>))
      .WithTags("Solution");
    
    app.MapGet("BpnEngine/v1/System/DiagramSvg/{solutionId}", async (HttpContext context, [FromServices] IQuerySession session, Guid solutionId, CancellationToken ct) =>
      {
        var bpnSolution = await session.Query<BpnSolution>().Where(p => p.Id == solutionId).FirstAsync();
        context.Response.ContentType = "image/svg+xml"; 
        await context.Response.WriteAsync(bpnSolution.C4DiagramSvg, ct);
      }).WithName("GetC4_Level1DiagramSvg")
      .Produces(StatusCodes.Status200OK, typeof(string))
      .WithTags("System.Diagram");
  }
  public BpnSolutionProjection()
  {
    Identity<CreateSystemFeature.SystemCreated>(x => x.SolutionId);
    Identity<CreateSolutionFeature.SolutionCreated>(x => x.Id);
  }
  public static void Apply(BpnSolution view, IEvent<CreateSolutionFeature.SolutionCreated> @event)
  {
    view.Id = @event.Data.Id;
    view.Name = @event.Data.Name;
    view.CreatedTimestamp = @event.Timestamp;
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnSolution view, IEvent<CreateSystemFeature.SystemCreated> @event)
  {
    view.Systems.Add(new SystemDetails(@event.Data.Id, @event.Data.Name, @event.Data.Description));
    view.LastUpdatedTimestamp = @event.Timestamp;
    
    var diagram = new C4SystemDiagram("TODO", view.Systems.ToArray());
    view.C4DiagramSvg = C4DiagramHelper.GenerateC4(diagram);

  }
  public class SystemDetails(Guid Id, string Name, string Description)
  {
    [Required]
    public Guid Id { get; set; } = Id;
    [Required]
    public string Name { get; set; } = Name;
    [Required]
    public string Description { get; set; } = Description;
  }
  public class BpnSolution
  {
    [Required]
    public Guid Id { get; set; } = Guid.Empty;
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Description { get; set; } = "";
    [Required]
    public string C4DiagramSvg { get; set; } = "<svg />";
    
    [Required]
    public DateTimeOffset CreatedTimestamp { get; set; }
    [Required]
    public DateTimeOffset LastUpdatedTimestamp { get; set; }
    [Required]
    public List<SystemDetails> Systems { get; set; } = [];
    //Relations to other systems? (maybe determin by: messages sendt and received? + services called)
    //Person/Customer for documentation (including relation to system) - https://c4model.com/diagrams/system-context
    public BpnSolution() { }
  }
}
