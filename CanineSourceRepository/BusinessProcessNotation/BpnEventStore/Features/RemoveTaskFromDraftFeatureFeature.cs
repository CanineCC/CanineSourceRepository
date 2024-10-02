using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class RemoveTaskFromDraftFeatureFeature : IFeature
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapDelete("BpnEngine/v1/DraftFeature/RemoveTask/{featureId}/{task}", async (HttpContext context, [FromServices] IDocumentSession session, Guid featureId, Guid task, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/RemoveTask", featureId, task, ct);
      return Results.Ok(id);
    }).WithName("RemoveTaskFromDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTaskRemoved>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTaskRemoved(TaskId: taskId));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
