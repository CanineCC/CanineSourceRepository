using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;

public class DeleteAssertionOnTaskFeature : IFeature
{
  public record AssertionDeletedOnTask(Guid FeatureId, Guid TaskId, Guid AssertionId);
  public record DeleteAssertionOnTask
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
    app.MapDelete($"BpnEngine/v1/DraftFeature/DeleteAssertion", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] DeleteAssertionOnTask request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/DeleteAssertion", request.FeatureId, request.TaskId, request.AssertionId, ct);
      return Results.Accepted();
    }).WithName("DeleteAssertion")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(DeleteAssertionOnTask), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<AssertionDeletedOnTask>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, Guid assertionId, CancellationToken ct)
  {
    //TODO validate exist? (feature, task assertion)
    var @event = new AssertionDeletedOnTask(
    FeatureId: featureId,
    TaskId: taskId,
    AssertionId: assertionId);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
