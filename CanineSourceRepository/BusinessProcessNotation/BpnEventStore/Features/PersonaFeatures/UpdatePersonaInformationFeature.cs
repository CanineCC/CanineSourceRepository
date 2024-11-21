
using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.PersonaFeatures;

public class UpdatePersonaInformationFeature : IFeature
{
  public record PersonaInformationUpdated(Guid PersonaId,string Name,string Description, Scope Scope);
  public record UpdatePersonaInformationBody
  {
    [Required] public Guid PersonaId{ get; set; }
    [Required] public string Name { get; set; } = "";
    [Required] public string Description { get; set; }= "";
    [Required] public Scope Scope { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPut($"BpnEngine/v1/Persona/Update", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdatePersonaInformationBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/Persona/Update", request.PersonaId, request.Name, request.Description, request.Scope, ct);
      return Results.Accepted();
    }).WithName("UpdatePersona")
     .Produces(StatusCodes.Status200OK)
     .WithTags("Persona")
     .Accepts(typeof(UpdatePersonaInformationBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<PersonaInformationUpdated>();
  }
  private static async Task Execute(IDocumentSession session, string causationId, Guid personaId, string name, string description, Scope scope, CancellationToken ct)
  {
    var @event = new PersonaInformationUpdated(
      PersonaId: personaId,
      Name: name,
      Description: description,
      Scope: scope);
    await session.RegisterEventsOnPersona(ct, personaId, causationId, @event);
  }
}
