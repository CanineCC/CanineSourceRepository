using System.ComponentModel.DataAnnotations;
using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.TaskFeatures;

public class UpdateCodeOnTaskFeature : IFeature
{
  public record CodeUpdatedOnTask(Guid FeatureId, Guid TaskId, string Code);
  public class UpdateCodeOnTaskBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public string Code { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPut($"BpnEngine/v1/DraftFeature/UpdateCodeOnTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateCodeOnTaskBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/UpdateCodeOnTask", request.FeatureId, request.TaskId, request.Code, ct);
      return Results.Accepted();
    }).WithName("UpdateCodeOnTaskFeature")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(UpdateCodeOnTaskBody), false, "application/json");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<CodeUpdatedOnTask>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, string code, CancellationToken ct)
  {
    //TODO: Check that it exist, otherwise fail
    //TODO: check that type is codeTask (not apiTask)
    var @event = new CodeUpdatedOnTask(
      FeatureId: featureId,
      TaskId: taskId,
      Code: code);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
