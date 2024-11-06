using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnContext;

public class CreateContainerFeature : IFeature
{
  public record WebApiContainerCreated(Guid Id, Guid SystemId, string Name);
  public class CreateContainerBody
  {
    public CreateContainerBody(string name, Guid systemId)
    {
      SystemId = systemId;
      Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    [Required]
    public Guid SystemId { get; set; }
    [Required]
    public string Name { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/Container/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CreateContainerBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/Container/Add", request.Name, request.SystemId, ct);
      return Results.Ok(id);
    }).WithName("CreateContainer")
     .Produces(StatusCodes.Status200OK)
     .WithTags("Container")
     .Accepts(typeof(CreateContainerBody), false, "application/json"); // Define input content type

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<WebApiContainerCreated>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, string name, Guid systemId, CancellationToken ct)
  {
    var newId = Guid.CreateVersion7();
    await session.RegisterEventsOnBpnContext(ct, newId, causationId, new WebApiContainerCreated(Id: newId, Name: name, SystemId: systemId));
    return newId;
  }
}
