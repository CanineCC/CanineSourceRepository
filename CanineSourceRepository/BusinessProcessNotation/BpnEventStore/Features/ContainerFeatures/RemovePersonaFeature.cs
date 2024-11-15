/*
namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.ContainerFeatures;

public class RemovePersonaFeature : IFeature
{//TODO: OVERVEJ AT FLYTTE TIL CONTAINER - så "uses" på System blot er udledt af at PERSONER har en eller flere relationer til containers i systemet!!!
    //Overvej om persona skal have en liste af relationer (en pr. container)
    public record PersonaRemoved(Guid SolutionId,Guid SystemId, Guid ContainerId, Guid PersonaId);
    public class RemovePersonaBody
    {
        public RemovePersonaBody(Guid containerId, Guid personaId)
        {
            PersonaId = personaId;
            ContainerId = containerId;
        }
        [Required]
        public Guid ContainerId { get; set; }
        [Required]
        public Guid PersonaId { get; set; }
    }
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapDelete($"BpnEngine/v1/Container/RemovePersona", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] RemovePersonaBody request, CancellationToken ct) =>
            {
                await Execute(session, "WebApplication/v1/BpnEngine/Container/RemovePersona", request.ContainerId, request.PersonaId, ct);
                return Results.Accepted();
            }).WithName("RemovePersona")
            .Produces(StatusCodes.Status202Accepted)
            .WithTags("Container")
            .Accepts(typeof(RemovePersonaBody), false, "application/json"); // Define input content type

    }
    public static void RegisterBpnEvents(StoreOptions options)
    {
        options.Events.AddEventType<PersonaRemoved>();
    }
    public static async Task Execute(IDocumentSession session, string causationId, Guid containerId, Guid personaId, CancellationToken ct)
    {
        var aggregate = await session.Events.AggregateStreamAsync<WebApiContainerAggregate>(containerId, token: ct);
        if (aggregate == null) throw new Exception($"Container '{containerId}' was not found");// return new ValidationResponse(false, $"System '{systemId}' was not found", ResultCode.NotFound);
        var systemAggregate = await session.Events.AggregateStreamAsync<SystemAggregate>(aggregate.SystemId, token: ct);
        if (systemAggregate == null) throw new Exception($"System '{aggregate.SystemId}' was not found");// return new ValidationResponse(false, $"System '{systemId}' was not found", ResultCode.NotFound);

        await session.RegisterEventsOnBpnContext(ct, containerId, causationId, new PersonaRemoved( SolutionId: systemAggregate.SolutionId, SystemId: aggregate.SystemId, ContainerId: containerId, PersonaId: personaId));
    }
}*/