using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class AddTaskToDraftFeatureFeature : IFeature
{
  public record DraftFeatureTaskAdded(BpnTask Task);
  public record AddCodeTaskToDraftFeatureBody(Guid FeatureId, CodeTask Task);
  public record AddApiTaskToDraftFeatureBody(Guid FeatureId, ApiInputTask Task);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/AddCodeTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddCodeTaskToDraftFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/AddCodeTask", request.FeatureId, request.Task, ct);
      return Results.Ok(id);
    }).WithName("AddCodeTaskToDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(AddCodeTaskToDraftFeatureBody), false, "application/json");

    app.MapPost($"BpnEngine/v1/DraftFeature/AddApiTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddApiTaskToDraftFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/AddApiTask", request.FeatureId, request.Task, ct);
      return Results.Ok(id);
    }).WithName("AddApiTaskToDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(AddApiTaskToDraftFeatureBody), false, "application/json");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeatureTaskAdded>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, BpnTask task, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new DraftFeatureTaskAdded(Task: task));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
