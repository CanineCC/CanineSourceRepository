using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class RemoveTransitionFromDraftFeatureFeature : IFeature
{
  public record Request(Guid FeatureId, BpnTransition Transition);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapDelete($"BpnEngine/v1/DraftFeature/RemoveTransition", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] Request request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/RemoveTransition", request.FeatureId, request.Transition, ct);
      return Results.Ok(id);
    }).WithName("RemoveTransitionFromDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(Request), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTransitionAdded>();
  }
  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, BpnTransition transition, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTransitionRemoved(Transition: transition));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }

}
