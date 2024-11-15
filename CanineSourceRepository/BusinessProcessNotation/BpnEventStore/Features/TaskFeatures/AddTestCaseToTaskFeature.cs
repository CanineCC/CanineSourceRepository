using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.TaskFeatures;

public class AddTestCaseToTaskFeature : IFeature
{
  public record TestCaseAddedToTaskBody(Guid FeatureId, Guid TaskId, TestCase TestCase);
  public record AddTestCaseToTaskBody
  {
    [Required]
    public Guid FeatureId { get; set; }
    [Required]
    public Guid TaskId { get; set; }
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Input { get; set; } = "";
    [Required]
    public AssertDefinition[] Asserts { get; set; } = [];
  }

  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/AddTestCase", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddTestCaseToTaskBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/AddTestCase", request.FeatureId, request.TaskId, request.Name, request.Input, request.Asserts, ct);
      return Results.Ok(id);
    }).WithName("AddTestCase")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(AddTestCaseToTaskBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
      options.Events.AddEventType<TestCaseAddedToTaskBody>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, string name, string input, AssertDefinition[] asserts, CancellationToken ct)
  {
    var id = Guid.CreateVersion7();
    var @event = new TestCaseAddedToTaskBody(
        FeatureId: featureId,
        TaskId: taskId,
        TestCase: new TestCase { Id = id, Input = input, Name = name, Asserts = asserts  } );
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
    return id;
  }
}
