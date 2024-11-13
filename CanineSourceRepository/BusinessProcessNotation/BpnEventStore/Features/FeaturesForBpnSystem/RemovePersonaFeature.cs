using System.ComponentModel.DataAnnotations;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSystem;

public class RemovePersonaFeature : IFeature
{//TODO: OVERVEJ AT FLYTTE TIL CONTAINER - så "uses" på System blot er udledt af at PERSONER har en eller flere relationer til containers i systemet!!!
    //Overvej om persona skal have en liste af relationer (en pr. container)
    public record PersonaRemoved(Guid SolutionId, Guid SystemId, Guid PersonaId);
    public class RemovePersonaBody
    {
        public RemovePersonaBody(Guid systemId, Guid personaId)
        {
            PersonaId = personaId;
            SystemId = systemId;
        }
        [Required]
        public Guid SystemId { get; set; }
        [Required]
        public Guid PersonaId { get; set; }
    }
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapDelete($"BpnEngine/v1/System/RemovePersona", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] RemovePersonaBody request, CancellationToken ct) =>
            {
                await Execute(session, "WebApplication/v1/BpnEngine/System/RemovePersona", request.SystemId, request.PersonaId, ct);
                return Results.Accepted();
            }).WithName("RemovePersona")
            .Produces(StatusCodes.Status202Accepted)
            .WithTags("System")
            .Accepts(typeof(RemovePersonaBody), false, "application/json"); // Define input content type

    }
    public static void RegisterBpnEvents(StoreOptions options)
    {
        options.Events.AddEventType<PersonaRemoved>();
    }
    public static async Task Execute(IDocumentSession session, string causationId, Guid systemId, Guid personaId, CancellationToken ct)
    {
        var aggregate = await session.Events.AggregateStreamAsync<BpnSystemAggregate>(systemId, token: ct);
        if (aggregate == null) throw new Exception($"System '{systemId}' was not found");// return new ValidationResponse(false, $"System '{systemId}' was not found", ResultCode.NotFound);
        await session.RegisterEventsOnBpnContext(ct, systemId, causationId, new PersonaRemoved(SolutionId: aggregate.SolutionId, SystemId: systemId, PersonaId: personaId));
    }
}