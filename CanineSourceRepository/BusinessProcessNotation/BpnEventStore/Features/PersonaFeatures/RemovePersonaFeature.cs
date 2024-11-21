using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.PersonaFeatures;

public class RemovePersonaFeature : IFeature
{
    public record PersonaRemoved(Guid PersonaId);
    public class RemovePersonaBody
    {
        public RemovePersonaBody(Guid personaId)
        {
            PersonaId = personaId;
        }
        [Required]
        public Guid PersonaId { get; set; }
    }
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapDelete($"BpnEngine/v1/Persona/Remove", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] RemovePersonaBody request, CancellationToken ct) =>
            {
                await Execute(session, "WebApplication/v1/BpnEngine/Persona/Remove", request.PersonaId, ct);
                return Results.Accepted();
            }).WithName("RemovePersona")
            .Produces(StatusCodes.Status202Accepted)
            .WithTags("Persona")
            .Accepts(typeof(RemovePersonaBody), false, "application/json"); // Define input content type

    }
    public static void RegisterBpnEvents(StoreOptions options)
    {
        options.Events.AddEventType<PersonaRemoved>();
    }
    public static async Task Execute(IDocumentSession session, string causationId, Guid personaId, CancellationToken ct)
    {
        await session.RegisterEventsOnPersona(ct, personaId, causationId, new PersonaRemoved( PersonaId: personaId));
    }
}