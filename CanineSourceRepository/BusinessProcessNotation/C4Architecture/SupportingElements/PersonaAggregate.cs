namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.SupportingElements;

public class PersonaAggregate
{
    public void Apply(
        PersonaAggregate aggregate,
        AddPersonaFeature.PersonaCreated @event
    )
    {
        aggregate.Id = @event.PersonaId;
        aggregate.Name = @event.Name;
        aggregate.Description = @event.Description;
        aggregate.Scope = @event.Scope;
    }
    
    public record RelationToComponent(Guid ComponentId, string Description);
    [Required] public Guid Id { get; internal set; } 
    [Required] public string Name { get; internal set; } 
    [Required] public string Description { get; internal set; } 
    [Required] public Scope Scope { get; internal set;} 
    //public List<RelationToComponent> ConsumeComponents { get; } = new List<RelationToComponent>();
}


public class PersonaProjection : SingleStreamProjection<PersonaProjection.Persona>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet($"BpnEngine/v1/Persona/All", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
    {
      var bpnContexts = await session.Query<Persona>().ToListAsync(ct);
      return Results.Ok(bpnContexts);
    }).WithName("GetAllPersonas")
      .Produces(StatusCodes.Status200OK, typeof(List<Persona>))
      .WithTags("Persona");
  }
  public static void Apply(Persona view, IEvent<ComponentNoLongerConsumedByPersonaFeature.ComponentNoLongerConsumedByPersona> @event)
  {
    view.Components.RemoveAll(p => p.ComponentId == @event.Data.ComponentId);
  }
  public static void Apply(Persona view, IEvent<PersonaConsumeComponentFeature.ComponentCosumedByPersona> @event)
  {
    view.Components.Add(new Persona.ConsumeComponent(@event.Data.ComponentId, @event.Data.ConsumeText));
  }

  public static void Apply(Persona view, IEvent<AddPersonaFeature.PersonaCreated> @event)
  {
    view.Id = @event.Data.PersonaId;
    view.Name = @event.Data.Name;
    view.Description = @event.Data.Description;
  }
  public class Persona
  {
    public record ConsumeComponent(Guid ComponentId, string ConsumeText); 
    [Required] public Guid Id { get; set; }
    [Required] public List<ConsumeComponent> Components { get; set; } = [];
    [Required] public string Name { get; set; } = "";
    [Required] public string Description { get; set; } = "";
  }
  
}