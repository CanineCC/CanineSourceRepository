using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
using RTools_NTS.Util;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class RemoveTransitionFromDraftFeatureFeature : IFeature
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapDelete("BpnEngine/v1/DraftFeature/RemoveTransition/{featureId}/{fromBpn}/{toBpn}", async (HttpContext context, [FromServices] IDocumentSession session, Guid featureId, Guid fromBpn, Guid toBpn, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/RemoveTransition", featureId, fromBpn, toBpn, ct);
      return Results.Ok(id);
    }).WithName("RemoveTransitionFromDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTransitionAdded>();
  }
  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, Guid fromBpn, Guid toBpn, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTransitionRemoved(FromBpn: fromBpn, ToBpn: toBpn));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }

}
