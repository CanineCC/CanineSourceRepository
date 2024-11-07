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
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  
  public class ContextDetails(Guid Id, string Name)
  {
    [Required]
    public Guid Id { get; set; } = Id;
    [Required]
    public string Name { get; set; } = Name;
  }

  public class BpnSystem
  {
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