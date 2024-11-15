namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.PersonaFeatures;

public class ComponentNoLongerConsumedByPersonaFeature : IFeature
{
    public record ComponentNoLongerConsumedByPersona(Guid PersonaId, Guid SolutionId, Guid SystemId, Guid ContainerId, Guid ComponentId);
    public record ComponentNoLongerConsumedByPersonaBody
    {
        [Required] public Guid PersonaId { get; set; }
        [Required] public Guid ComponentId { get; set; }
    }
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapPost($"BpnEngine/v1/Persona/UnconsumeComponent", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] ComponentNoLongerConsumedByPersonaBody request, CancellationToken ct) =>
            {
                await Execute(session, "WebApplication/v1/BpnEngine/Persona/UnconsumeComponent", request.PersonaId, request.ComponentId,  ct);
                return Results.Accepted();
            }).WithName("ComponentNoLongerConsumedByPersona")
            .Produces(StatusCodes.Status202Accepted)
            .WithTags("Persona")
            .Accepts(typeof(ComponentNoLongerConsumedByPersonaBody), false, "application/json");

    }
    public static void RegisterBpnEvents(StoreOptions options)
    {
        options.Events.AddEventType<ComponentNoLongerConsumedByPersona>();
    }
    private static async Task Execute(IDocumentSession session, string causationId, Guid personaId, Guid componentId,  CancellationToken ct)
    {
        var componentAggregate =
            await session.Events.AggregateStreamAsync<DraftFeatureComponentAggregate>(componentId, token: ct);
        if (componentAggregate == null) throw new Exception($"Component '{componentId}' was not found");
        var containerAggregate = await session.Events.AggregateStreamAsync<WebApiContainerAggregate>(componentAggregate.ContainerId, token: ct);
        if (containerAggregate == null) throw new Exception($"Container '{componentAggregate.ContainerId}' was not found");
        var systemAggregate = await session.Events.AggregateStreamAsync<SystemAggregate>(containerAggregate.SystemId, token: ct);
        if (systemAggregate == null) throw new Exception($"System '{containerAggregate.SystemId}' was not found");
        
        var @event = new ComponentNoLongerConsumedByPersona(
            PersonaId: personaId,
            SystemId:systemAggregate.Id,
            ContainerId:containerAggregate.Id,
            SolutionId: systemAggregate.SolutionId,
            ComponentId: componentId
            );
        await session.RegisterEventsOnBpnDraftFeature(ct, personaId, causationId, @event);
    }
}