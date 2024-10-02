using CanineSourceRepository.BusinessProcessNotation.BpnContext;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class AddDraftFeatureFeature : IFeature
{
  public record Request(Guid BpnContextId, string Name, string Objective, string FlowOverview);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] Request request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Add", request.BpnContextId, request.Name, request.Objective, request.FlowOverview, ct);
      return Results.Ok(id);
    }).WithName("AddDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(Request), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureCreated>();
  //  options.Events.AddEventType<BpnContextProjection.BpnContext.FeatureAddedToContext>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, Guid bpnContextId, string name, string objective, string flowOverview, CancellationToken ct)
  {
    var featureId = Guid.CreateVersion7();

    var @event = new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureCreated(
    ContextId: bpnContextId,
    FeatureId: featureId,
    Name: name,
    Objective: objective,
    FlowOverview: flowOverview);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);

//    await session.RegisterEventsOnBpnContext(ct, bpnContextId, causationId, new BpnContextProjection.BpnContext.FeatureAddedToContext(FeatureId: featureId, Name: name));

    return featureId;
  }
}
