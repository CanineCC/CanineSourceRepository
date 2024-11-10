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
  public string Name { get; internal set; }
  
  public void Apply(
    BpnSystemAggregate aggregate,
    CreateSystemFeature.SystemCreated @event
  )
  {
    aggregate.Id = @event.Id;
    aggregate.Name = @event.Name;
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
    
    
    app.MapGet("BpnEngine/v1/Container/DiagramSvg/{systemId}", async (HttpContext context, [FromServices] IQuerySession session, Guid systemId, CancellationToken ct) =>
      {
        var bpnSystem = await session.Query<BpnSystemProjection.BpnSystem>().Where(p=>p.Id == systemId).FirstAsync(ct);
        context.Response.ContentType = "image/svg+xml"; 
        await context.Response.WriteAsync(bpnSystem.C4ContainerDiagramSvg, ct);
      }).WithName("GetC4_level2DiagramSvg")
      .Produces(StatusCodes.Status200OK, typeof(string))
      .WithTags("Container.Diagram");
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
    view.Contexts.Add(new ContextDetails(@event.Data.Id, @event.Data.Name, @event.Data.Descrption));
    view.LastUpdatedTimestamp = @event.Timestamp;
    var diagram = new C4ContainerDiagram(view, view.Contexts.ToArray());
    view.C4ContainerDiagramSvg = C4DiagramHelper.GenerateC4(diagram);
  }
  public class ContextDetails(Guid Id, string Name, string Description)
  {
    [Required]
    public Guid Id { get; set; } = Id;
    [Required]
    public string Name { get; set; } = Name;
    [Required]
    public string Description { get; set; } = Description;
    
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
    
    [Required]
    public string C4ContainerDiagramSvg { get; set; } = "<svg />";
    
    //Relations to other systems? (maybe determin by: messages sendt and received? + services called)
    //Person/Customer for documentation (including relation to system) - https://c4model.com/diagrams/system-context
    public BpnSystem() { }
  }
}
