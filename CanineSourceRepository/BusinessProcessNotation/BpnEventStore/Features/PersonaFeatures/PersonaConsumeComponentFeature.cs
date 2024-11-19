namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.PersonaFeatures;

public class PersonaConsumeComponentFeature : IFeature
{
    public record ComponentCosumedByPersona(Guid PersonaId,Guid SolutionId, Guid SystemId, Guid ContainerId, Guid ComponentId, string Name, string Description, string ConsumeText);
    public record ComponentCosumedByPersonaBody
    {
        [Required] public Guid PersonaId { get; set; }
        [Required] public string ConsumeDescription { get; set; } = "";
        [Required] public Guid ComponentId { get; set; }
    }
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapPost($"BpnEngine/v1/Persona/ConsumeComponent", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] ComponentCosumedByPersonaBody request, CancellationToken ct) =>
            {
                await Execute(session, "WebApplication/v1/BpnEngine/Persona/ConsumeComponent", request.PersonaId, request.ComponentId, request.ConsumeDescription, ct);
                return Results.Accepted();
            }).WithName("PersonaConsumeComponent")
            .Produces(StatusCodes.Status202Accepted)
            .WithTags("Persona")
            .Accepts(typeof(ComponentCosumedByPersonaBody), false, "application/json");

    }
    public static void RegisterBpnEvents(StoreOptions options)
    {
        options.Events.AddEventType<ComponentCosumedByPersona>();
    }
    public static async Task Execute(IDocumentSession session, string causationId, Guid personaId, Guid componentId, string ConsumeDescription, CancellationToken ct)
    {
        var personaAggregate = await session.Events.AggregateStreamAsync<PersonaAggregate>(personaId, token: ct);
        if (personaAggregate == null) throw new Exception($"Persona '{personaId}' was not found");
        var componentAggregate =
            await session.Events.AggregateStreamAsync<DraftFeatureComponentAggregate>(componentId, token: ct);
        if (componentAggregate == null) throw new Exception($"Component '{componentId}' was not found");
        var containerAggregate = await session.Events.AggregateStreamAsync<WebApiContainerAggregate>(componentAggregate.ContainerId, token: ct);
        if (containerAggregate == null) throw new Exception($"Container '{componentAggregate.ContainerId}' was not found");
        var systemAggregate = await session.Events.AggregateStreamAsync<SystemAggregate>(containerAggregate.SystemId, token: ct);
        if (systemAggregate == null) throw new Exception($"System '{containerAggregate.SystemId}' was not found");
        
        var @event = new ComponentCosumedByPersona(
            PersonaId: personaId,
            ComponentId: componentId,
            SolutionId: systemAggregate.SolutionId,
            ContainerId:containerAggregate.Id,
            SystemId:systemAggregate.Id,
            Name: personaAggregate.Name,
            Description: personaAggregate.Description,
            ConsumeText: ConsumeDescription);
        await session.RegisterEventsOnPersona(ct, personaId, causationId, @event);
    }
}