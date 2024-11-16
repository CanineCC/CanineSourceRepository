using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.DraftComponentFeatures;

public class AddTaskToDraftFeatureFeature : IFeature
{
  public record DraftFeatureTaskAdded(BpnTask Task);
  public class AddTaskToDraftFeatureBody
  {
    [Required]
    public Guid FeatureId { get; set; }
    [Required]
    public BpnTask Task { get; set; }
  }

  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/AddTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddTaskToDraftFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/AddTask", request.FeatureId, request.Task, ct);
      return Results.Ok(id);
    }).WithName("AddTaskToDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(AddTaskToDraftFeatureBody), false, "application/json");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeatureTaskAdded>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, BpnTask task, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<DraftFeatureComponentAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new DraftFeatureTaskAdded(Task: task));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
