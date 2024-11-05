using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;

public class UpdateTestCaseOnTaskFeature : IFeature
{
  public record TestCaseUpdatedOnTaskBody(Guid FeatureId, Guid TaskId, TestCase TestCase);
  public class UpdateTestCaseOnTaskBody
  {
    [Required]
    public Guid FeatureId { get; set; }
    [Required]
    public Guid TaskId { get; set; }
    [Required]
    public TestCase TestCase { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/UpdateTestCase", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateTestCaseOnTaskBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/UpdateTestCase", request.FeatureId, request.TaskId, request.TestCase, ct);
      return Results.Accepted();
    }).WithName("UpdateTestCase")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(UpdateTestCaseOnTaskBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<TestCaseUpdatedOnTaskBody>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, TestCase testCase, CancellationToken ct)
  {
    //TODO Validation
    var @event = new TestCaseUpdatedOnTaskBody(
    FeatureId: featureId,
    TaskId: taskId,
    TestCase: testCase);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
