/*
namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.ContainerFeatures;

public class AddPersonaFeature : IFeature
{

    //TODO::
    //Lav persona om til egen entitet, med relation til forskellige systemer og forskellige containers
    //lav også UI til at vedligeholde disse personas
    //overvej om disse også skal indgå i (automatiske) test
    //overvej om disse skal bruges til gruppe/rolle system -> autogen af sikkerhedssystem baseret på hvilke profiler der bruger systemet
    public record PersonaAdded(Guid SolutionId, Guid SystemId, Guid ContainerId, Guid PersonaId, string Name, string Description, string RelationToContainer);
    public class AddPersonaBody
    {
        public AddPersonaBody(Guid containerId, string name, string description, string relationToContainer)
        {
            ContainerId = containerId;   
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            RelationToSystem = relationToContainer ?? throw new ArgumentNullException(nameof(relationToContainer));
        }
        [Required]
        public Guid ContainerId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string RelationToSystem { get; set; }
    }
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapPost($"BpnEngine/v1/Container/AddPersona", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddPersonaBody request, CancellationToken ct) =>
            {
                var id = await Execute(session, "WebApplication/v1/BpnEngine/Container/AddPersona", request.ContainerId, request.Name, request.Description, request.RelationToSystem, ct);
                return Results.Ok(id);
            }).WithName("AddPersona")
            .Produces(StatusCodes.Status200OK)
            .WithTags("Container")
            .Accepts(typeof(AddPersonaBody), false, "application/json"); // Define input content type

    }
    public static void RegisterBpnEvents(StoreOptions options)
    {
        options.Events.AddEventType<PersonaAdded>();
    }
    public static async Task<Guid> Execute(IDocumentSession session, string causationId, Guid containerId, string name,string description, string relationToSystem,CancellationToken ct)
    {
        var aggregate = await session.Events.AggregateStreamAsync<WebApiContainerAggregate>(containerId, token: ct);
        if (aggregate == null) throw new Exception($"Container '{containerId}' was not found");// return new ValidationResponse(false, $"System '{systemId}' was not found", ResultCode.NotFound);
        var systemAggregate = await session.Events.AggregateStreamAsync<SystemAggregate>(aggregate.SystemId, token: ct);
        if (systemAggregate == null) throw new Exception($"System '{aggregate.SystemId}' was not found");// return new ValidationResponse(false, $"System '{systemId}' was not found", ResultCode.NotFound);

        var newId = Guid.CreateVersion7();
        await session.RegisterEventsOnBpnContext(ct, containerId, causationId, new PersonaAdded(SolutionId: systemAggregate.SolutionId, SystemId: aggregate.SystemId, ContainerId:containerId, PersonaId: newId, Name: name, Description: description, RelationToContainer: relationToSystem));
        return newId;
    }
}*/