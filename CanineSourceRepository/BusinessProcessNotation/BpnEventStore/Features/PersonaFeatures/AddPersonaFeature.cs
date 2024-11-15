
namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.PersonaFeatures;

public class AddPersonaFeature : IFeature
{
  public record PersonaCreated(Guid PersonaId,string Name,string Description, Scope Scope);
  public record AddPersonaBody
  {
    [Required] public string Name { get; set; } = "";
    [Required] public string Description { get; set; } = "";
    [Required] public Scope Scope { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/Persona/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddPersonaBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/Persona/Add", request.Name, request.Description, request.Scope, ct);
      return Results.Ok(id);
    }).WithName("AddPersona")
     .Produces(StatusCodes.Status200OK)
     .WithTags("Persona")
     .Accepts(typeof(AddPersonaBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<PersonaCreated>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, string name, string description, Scope scope, CancellationToken ct)
  {
    var id = Guid.CreateVersion7();

    var @event = new PersonaCreated(
      PersonaId: id,
      Name: name,
      Description: description,
      Scope: scope);
    await session.RegisterEventsOnBpnDraftFeature(ct, id, causationId, @event);

    return id;
  }
}