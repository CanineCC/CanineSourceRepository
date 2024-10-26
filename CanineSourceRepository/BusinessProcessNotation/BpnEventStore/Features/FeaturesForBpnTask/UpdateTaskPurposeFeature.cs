using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;

public class UpdateTaskPurposeFeature : IFeature
{
  public record TaskPurposeUpdated(Guid FeatureId, Guid TaskId, string Name, string BusinessPurpose, string BehavioralGoal);
  public class UpdateTaskPurposeFeatureBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string BusinessPurpose { get; set; }

    [Required]
    public string BehavioralGoal { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPut($"BpnEngine/v1/DraftFeature/UpdateTaskPurposeFeature", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateTaskPurposeFeatureBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/UpdateTaskPurposeFeature", request.FeatureId, request.TaskId, request.Name, request.BusinessPurpose, request.BehavioralGoal, ct);
      return Results.Accepted();
    }).WithName("UpdateTaskPurposeFeature")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(UpdateTaskPurposeFeature), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<TaskPurposeUpdated>();
  }
  public static async Task  Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, string name, string businessPurpose, string behavioralGoal, CancellationToken ct)
  {
    //TODO: Check that it exist, otherwise fail
    var @event = new TaskPurposeUpdated(
      FeatureId: featureId,
      TaskId: taskId,
      Name: name,
      BusinessPurpose: businessPurpose,
      BehavioralGoal: behavioralGoal);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
