using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class ResetDraftFeatureFeature : IFeature
{
  public record Request(Guid FeatureId);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/Reset", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] Request request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Reset", request.FeatureId, ct);
      return Results.Ok(id);
    }).WithName("ResetDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(Request), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureReset>();
  }
  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    var feature = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureReset(
      Tasks: feature?.Tasks ?? [],
      Transitions: feature?.Transitions ?? [],
      Diagram: feature?.Diagram ?? new BpnFeatureDiagram()
    ));

    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
