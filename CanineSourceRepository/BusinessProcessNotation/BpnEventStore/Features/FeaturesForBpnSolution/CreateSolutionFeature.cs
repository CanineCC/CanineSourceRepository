using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSolution;

public class CreateSolutionFeature : IFeature
{
    public record SolutionCreated(Guid Id, string Name, string Description);
    public class CreateSolutionBody
    {
        public CreateSolutionBody(string name, string description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description= description ?? throw new ArgumentNullException(nameof(description));
        }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapPost($"BpnEngine/v1/Solution/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CreateSolutionBody request, CancellationToken ct) =>
            {
                var id = await Execute(session, "WebApplication/v1/BpnEngine/Solution/Add", request.Name, request.Description, ct);
                return Results.Ok(id);
            }).WithName("CreateSolution")
            .Produces(StatusCodes.Status200OK)
            .WithTags("Solution")
            .Accepts(typeof(CreateSolutionBody), false, "application/json"); // Define input content type

    }
    public static void RegisterBpnEvents(StoreOptions options)
    {
        options.Events.AddEventType<SolutionCreated>();
    }
    public static async Task<Guid> Execute(IDocumentSession session, string causationId, string name, string description, CancellationToken ct)
    {
        var newId = Guid.CreateVersion7();
        await session.RegisterEventsOnBpnContext(ct, newId, causationId, new SolutionCreated(Id: newId, Name: name, Description: description));
        return newId;
    }
}
