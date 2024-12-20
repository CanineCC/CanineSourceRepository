﻿using Marten.Events.Projections;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

public class SystemAggregate
{
  public Guid Id { get; internal set; }
  public Guid SolutionId { get; internal set; }
  public string Name { get; internal set; }
  
  public void Apply(
    SystemAggregate aggregate,
    CreateSystemFeature.SystemCreated @event
  )
  {
    aggregate.Id = @event.Id;
    aggregate.SolutionId = @event.SolutionId;
    aggregate.Name = @event.Name;
  }

}


public class SystemProjection : MultiStreamProjection<SystemProjection.BpnSystem, Guid>
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
        var bpnSystem = await session.Query<SystemProjection.BpnSystem>().Where(p=>p.Id == systemId).FirstAsync(ct);
        context.Response.ContentType = "image/svg+xml"; 
        await context.Response.WriteAsync(bpnSystem.C4ContainerDiagramSvg, ct);
      }).WithName("GetC4_level2DiagramSvg")
      .Produces(StatusCodes.Status200OK, typeof(string))
      .WithTags("Container.Diagram");
  }
  public SystemProjection()
  {
    Identity<CreateSystemFeature.SystemCreated>(x => x.Id);
    Identity<WebApiContainerCreated>(x => x.SystemId);
    Identity<PersonaConsumeComponentFeature.ComponentCosumedByPersona>(x => x.SystemId);
    Identity<ComponentNoLongerConsumedByPersonaFeature.ComponentNoLongerConsumedByPersona>(x => x.SystemId);
    Identity<DraftFeatureTaskAdded>(x => x.SystemId);
  }
  
  public static void Apply(BpnSystem view, IEvent<DraftFeatureTaskAdded> @event)
  {
    view.LastUpdatedTimestamp = @event.Timestamp;
    var container = view.Containers.First(p => p.Id == @event.Data.ContainerId);

    if (@event.Data.ServiceTypeId == Guid.Empty) return;

    container.TasksWithService.Add(
      new Task()
      {
        ServiceDependencyId = @event.Data.ServiceTypeId,
        NamedConfigurationName = @event.Data.NamedConfigurationName,
        Id = @event.Data.TaskId
      }
    );
    
    var diagram = new C4ContainerDiagram(view, view.Containers.ToArray());
    view.C4ContainerDiagramSvg = C4DiagramHelper.GenerateC4(diagram);
  }

  
  public static void Apply(BpnSystem view, IEvent<ComponentNoLongerConsumedByPersonaFeature.ComponentNoLongerConsumedByPersona> @event)
  {
    view.LastUpdatedTimestamp = @event.Timestamp;
    var container = view.Containers.First(p => p.Id == @event.Data.ContainerId);
    var persona = container.Personas.FirstOrDefault(p => p.Id == @event.Data.PersonaId);
    if (persona == null) return;
    persona.Components.RemoveAll(p=>p == @event.Data.ComponentId);
    if (persona.Components.Count == 0)
    {
      container.Personas.RemoveAll(p=>p.Id == @event.Data.PersonaId);
      var diagram = new C4ContainerDiagram(view, view.Containers.ToArray());
      view.C4ContainerDiagramSvg = C4DiagramHelper.GenerateC4(diagram);
    }
  }
  public static void Apply(BpnSystem view, IEvent<PersonaConsumeComponentFeature.ComponentCosumedByPersona> @event)
  {
    view.LastUpdatedTimestamp = @event.Timestamp;
    var container = view.Containers.First(p => p.Id == @event.Data.ContainerId);
    var persona = container.Personas.FirstOrDefault(p => p.Id == @event.Data.PersonaId);
    if (persona == null)
    {
      persona = new Persona() { Id = @event.Data.PersonaId, Name = @event.Data.Name, Description = @event.Data.Description};
      container.Personas.Add(persona);
      var diagram = new C4ContainerDiagram(view, view.Containers.ToArray());
      view.C4ContainerDiagramSvg = C4DiagramHelper.GenerateC4(diagram);
    }
    persona.Components.Add(@event.Data.ComponentId);
  }

  public static void Apply(BpnSystem view, IEvent<CreateSystemFeature.SystemCreated> @event)
  {
    view.Id = @event.Data.Id;
    view.Name = @event.Data.Name;
    view.Description = @event.Data.Description;
    view.CreatedTimestamp = @event.Timestamp;
    view.LastUpdatedTimestamp = @event.Timestamp;
  }
  public static void Apply(BpnSystem view, IEvent<WebApiContainerCreated> @event)
  {
    view.Containers.Add(new ContainerDetails(@event.Data.Id, @event.Data.Name, @event.Data.Descrption));
    view.LastUpdatedTimestamp = @event.Timestamp;
    var diagram = new C4ContainerDiagram(view, view.Containers.ToArray());
    view.C4ContainerDiagramSvg = C4DiagramHelper.GenerateC4(diagram);
  }
  public class ContainerDetails(Guid Id, string Name, string Description)
  {
    [Required]
    public Guid Id { get; set; } = Id;
    [Required]
    public string Name { get; set; } = Name;
    [Required]
    public string Description { get; set; } = Description;
    [Required]
    public List<Persona> Personas { get; set; } = [];

    [Required]
    public List<Task> TasksWithService { get; set; } = [];

  }
  
  public class Task
  {
    public Guid Id { get; set; }
    public Guid ServiceDependencyId { get; set; }
    public string NamedConfigurationName { get; set; }
  }
  

  public class Persona
  {
    [Required]
    public Guid Id { get; set; }
    [Required]
    public List<Guid> Components { get; set; } = [];
    
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Description { get; set; } = "";
    [Required]
    public string RelationToContainer { get; set; } = "uses";
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
    public List<ContainerDetails> Containers { get; set; } = [];
    
    [Required]
    public string C4ContainerDiagramSvg { get; set; } = "<svg />";
    
    //Relations to other systems? (maybe determin by: messages sendt and received? + services called)
    //Person/Customer for documentation (including relation to system) - https://c4model.com/diagrams/system-context
    public BpnSystem() { }
  }
}
