using System.ComponentModel.DataAnnotations;
using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.TaskFeatures;

public class RemoveTestCaseFromTaskFeature : IFeature
{
  public record TestCaseRemovedFromTask(Guid FeatureId, Guid TaskId, Guid AssertionId);
  public record RemoveTestCaseFromTask
  {
    [Required]
    public Guid FeatureId { get; set; }
    [Required]
    public Guid TaskId { get; set; }
    [Required]
    public Guid AssertionId { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapDelete($"BpnEngine/v1/DraftFeature/RemoveTestCase", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] RemoveTestCaseFromTask request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/RemoveTestCase", request.FeatureId, request.TaskId, request.AssertionId, ct);
      return Results.Accepted();
    }).WithName("RemoveTestCase")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(RemoveTestCaseFromTask), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<TestCaseRemovedFromTask>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, Guid assertionId, CancellationToken ct)
  {
    //TODO validate exist? (feature, task assertion)
    var @event = new TestCaseRemovedFromTask(
    FeatureId: featureId,
    TaskId: taskId,
    AssertionId: assertionId);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
