using System.ComponentModel.DataAnnotations;
using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.TaskFeatures;

public class AddRecordToTaskFeature : IFeature
{
  public record RecordAddedToTask(Guid FeatureId, Guid TaskId, RecordDefinition RecordDefinition);
  public class AddRecordToTaskBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public RecordDefinition RecordDefinition { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {//TODO: overvej at sætte id på serverside! ( så man ikke skal bruge index/name til update og delete)
    app.MapPost($"BpnEngine/v1/DraftFeature/AddRecordToTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddRecordToTaskBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/AddRecordToTask", request.FeatureId, request.TaskId, request.RecordDefinition, ct);
      return Results.Accepted();
    }).WithName("AddRecordToTaskFeature")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(AddRecordToTaskBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<RecordAddedToTask>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, RecordDefinition recordDefinition, CancellationToken ct)
  {
    //TODO: Check that it DOES NOT exist, otherwise fail
    var @event = new RecordAddedToTask(
      FeatureId: featureId,
      TaskId: taskId,
      RecordDefinition: recordDefinition);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
