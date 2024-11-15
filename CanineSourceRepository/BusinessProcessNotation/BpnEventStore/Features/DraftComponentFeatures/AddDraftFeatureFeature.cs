using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.DraftComponentFeatures;

public class AddDraftFeatureFeature : IFeature
{
  public record DraftFeatureCreated(Guid ContainerId, Guid FeatureId, string Name, string Objective, string FlowOverview);
  public record AddDraftFeatureBody
  {
    [Required]
    public Guid ContainerId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Objective { get; set; }

    [Required]
    public string FlowOverview { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddDraftFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Add", request.ContainerId, request.Name, request.Objective, request.FlowOverview, ct);
      return Results.Ok(id);
    }).WithName("AddDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(AddDraftFeatureBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeatureCreated>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, Guid containerId, string name, string objective, string flowOverview, CancellationToken ct)
  {
    var featureId = Guid.CreateVersion7();

    var @event = new DraftFeatureCreated(
    ContainerId: containerId,
    FeatureId: featureId,
    Name: name,
    Objective: objective,
    FlowOverview: flowOverview);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);

    return featureId;
  }
}
