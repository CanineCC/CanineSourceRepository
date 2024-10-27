using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;

public class UpdateAssertionOnTaskFeature : IFeature
{
  public record AssertionUpdatedOnTaskBody(Guid FeatureId, Guid TaskId, TestCase TestCase);
  public record UpdateAssertionOnTaskBody
  {
    [Required]
    public Guid FeatureId { get; set; }
    [Required]
    public Guid TaskId { get; set; }
    [Required]
    public Guid AssertionId { get; set; }
    [Required]
    public TestCase TestCase { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/UpdateAssertion", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateAssertionOnTaskBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/UpdateAssertion", request.FeatureId, request.TaskId, request.TestCase, ct);
      return Results.Accepted();
    }).WithName("UpdateAssertion")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(UpdateAssertionOnTaskBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<AssertionUpdatedOnTaskBody>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, TestCase testCase, CancellationToken ct)
  {
    //TODO Validation
    var @event = new AssertionUpdatedOnTaskBody(
    FeatureId: featureId,
    TaskId: taskId,
    TestCase: testCase);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
