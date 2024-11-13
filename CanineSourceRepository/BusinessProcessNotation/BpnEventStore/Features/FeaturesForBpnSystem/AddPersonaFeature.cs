using System.ComponentModel.DataAnnotations;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSystem;

public class AddPersonaFeature : IFeature
{
    //TODO: Undersøg mulighed for at lave "hit boxes" på SVG så man kan zoome ind/ud på elementer
    
    //TODO: OVERVEJ AT FLYTTE TIL CONTAINER - så "uses" på System blot er udledt af at PERSONER har en eller flere relationer til containers i systemet!!!
    //Overvej om persona skal have en liste af relationer (en pr. container) - med features til at tilføje/opdatere/fjerne disse relationer
    public record PersonaAdded(Guid SolutionId, Guid SystemId, Guid PersonaId, string Name, string Description, string RelationToSystem);
    public class AddPersonaBody
    {
        public AddPersonaBody(Guid systemId, string name, string description, string relationToSystem)
        {
            SystemId = systemId;   
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            RelationToSystem = relationToSystem ?? throw new ArgumentNullException(nameof(relationToSystem));
        }
        [Required]
        public Guid SystemId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string RelationToSystem { get; set; }
    }
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapPost($"BpnEngine/v1/System/AddPersona", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddPersonaBody request, CancellationToken ct) =>
            {
                var id = await Execute(session, "WebApplication/v1/BpnEngine/System/AddPersona", request.SystemId, request.Name, request.Description, request.RelationToSystem, ct);
                return Results.Ok(id);
            }).WithName("AddPersona")
            .Produces(StatusCodes.Status200OK)
            .WithTags("System")
            .Accepts(typeof(AddPersonaBody), false, "application/json"); // Define input content type

    }
    public static void RegisterBpnEvents(StoreOptions options)
    {
        options.Events.AddEventType<PersonaAdded>();
    }
    public static async Task<Guid> Execute(IDocumentSession session, string causationId, Guid systemId, string name,string description, string relationToSystem,CancellationToken ct)
    {
        var aggregate = await session.Events.AggregateStreamAsync<BpnSystemAggregate>(systemId, token: ct);
        if (aggregate == null) throw new Exception($"System '{systemId}' was not found");// return new ValidationResponse(false, $"System '{systemId}' was not found", ResultCode.NotFound);
        var newId = Guid.CreateVersion7();
        await session.RegisterEventsOnBpnContext(ct, systemId, causationId, new PersonaAdded(SolutionId: aggregate.SolutionId, SystemId: systemId, PersonaId: newId, Name: name, Description: description, RelationToSystem: relationToSystem));
        return newId;
    }
}