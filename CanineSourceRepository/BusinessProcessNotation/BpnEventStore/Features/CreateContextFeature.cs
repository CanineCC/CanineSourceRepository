using CanineSourceRepository.BusinessProcessNotation.BpnContext;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class CreateContextFeature : IFeature
{
  public record Request(string Name);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/Context/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] Request request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/Context/Add", request.Name, ct);
      return Results.Ok(id);
    }).WithName("CreateContext")
     .Produces(StatusCodes.Status200OK)
     .WithTags("Context")
     .Accepts(typeof(Request), false, "application/json"); // Define input content type

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnContextProjection.BpnContext.ContextCreated>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, string name, CancellationToken ct)
  {
    var newId = Guid.CreateVersion7();
    await session.RegisterEventsOnBpnContext(ct, newId, causationId, new BpnContextProjection.BpnContext.ContextCreated(Id: newId, Name: name));
    return newId;
  }
}
