using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSystem;

public class CreateSystemFeature : IFeature
{
  public record SystemCreated(Guid Id, string Name);
  public class CreateSystemBody
  {
    public CreateSystemBody(string name)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    [Required]
    public string Name { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/System/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CreateContainerBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/System/Add", request.Name, ct);
      return Results.Ok(id);
    }).WithName("CreateSystem")
     .Produces(StatusCodes.Status200OK)
     .WithTags("System")
     .Accepts(typeof(CreateContainerBody), false, "application/json"); // Define input content type

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<SystemCreated>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, string name, CancellationToken ct)
  {
    var newId = Guid.CreateVersion7();
    await session.RegisterEventsOnBpnContext(ct, newId, causationId, new SystemCreated(Id: newId, Name: name));
    return newId;
  }
}
