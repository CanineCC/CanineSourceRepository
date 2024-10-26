using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;

public class DeleteRecordOnTaskFeature : IFeature
{
  public record RecordDeletedOnTask(Guid FeatureId, Guid TaskId, string Name);
  public class DeleteRecordOnTaskBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public string Name { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapDelete($"BpnEngine/v1/DraftFeature/DeleteRecordOnTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] DeleteRecordOnTaskBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/DeleteRecordOnTask", request.FeatureId, request.TaskId, request.Name, ct);
      return Results.Accepted();
    }).WithName("DeleteRecordOnTaskFeature")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(DeleteRecordOnTaskBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<RecordDeletedOnTask>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, string name, CancellationToken ct)
  {
    //TODO: Check that it exist, otherwise fail
    var @event = new RecordDeletedOnTask(
    FeatureId: featureId,
    TaskId: taskId,
    Name: name);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
