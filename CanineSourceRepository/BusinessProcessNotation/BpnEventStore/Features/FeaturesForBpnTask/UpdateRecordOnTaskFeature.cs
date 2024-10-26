using System.ComponentModel.DataAnnotations;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task.BpnTask;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;

public class UpdateRecordOnTaskFeature : IFeature
{
  public record RecordUpdatedOnTask(Guid FeatureId, Guid TaskId, RecordDefinition RecordDefinition);
  public class UpdateRecordOnTaskBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public RecordDefinition RecordDefinition { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPut($"BpnEngine/v1/DraftFeature/UpdateRecordOnTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateRecordOnTaskBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/UpdateRecordOnTask", request.FeatureId, request.TaskId, request.RecordDefinition, ct);
      return Results.Accepted();
    }).WithName("UpdateRecordOnTaskFeature")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(UpdateRecordOnTaskBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<RecordUpdatedOnTask>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, RecordDefinition recordDefinition, CancellationToken ct)
  {
    //TODO: Check that it exist, otherwise fail
    var @event = new RecordUpdatedOnTask(
      FeatureId: featureId,
      TaskId: taskId,
      RecordDefinition: recordDefinition);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
