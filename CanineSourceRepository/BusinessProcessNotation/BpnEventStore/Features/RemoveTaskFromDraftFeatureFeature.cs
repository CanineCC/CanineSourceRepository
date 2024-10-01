using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class RemoveTaskFromDraftFeatureFeature : IFeature
{
  public record Request(Guid FeatureId, BpnTask Task);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapDelete($"BpnEngine/v1/DraftFeature/RemoveTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] Request request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/RemoveTask", request.FeatureId, request.Task, ct);
      return Results.Ok(id);
    }).WithName("RemoveTaskFromDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(Request), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTaskRemoved>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, BpnTask task, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTaskRemoved(Task: task));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
