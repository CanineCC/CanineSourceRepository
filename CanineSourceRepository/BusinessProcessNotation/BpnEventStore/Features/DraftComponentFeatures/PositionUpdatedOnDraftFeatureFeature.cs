using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.DraftComponentFeatures;

public class PositionUpdatedOnDraftFeatureFeature : IFeature
{
  public record DraftFeatureDiagramPositionUpdated(BpnPosition Position);
  public class PositionUpdatedOnDraftFeatureBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public Position Position { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPatch($"BpnEngine/v1/DraftFeature/Diagram/PositionUpdated", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] PositionUpdatedOnDraftFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Diagram/PositionUpdated", request.FeatureId, request.TaskId, request.Position, ct);
      return Results.Ok(id);
    }).WithName("PositionUpdatedOnDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature.Diagram")
     .Accepts(typeof(PositionUpdatedOnDraftFeatureBody), false, "application/json");


    app.MapPatch($"BpnEngine/v1/DraftFeature/Diagram/PositionsUpdated", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] List<PositionUpdatedOnDraftFeatureBody> request, CancellationToken ct) =>
    {
      foreach (var req in request)
      {
        await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Diagram/PositionsUpdated", req.FeatureId, req.TaskId, req.Position, ct);
      }
      return Results.NoContent();
    }).WithName("PositionsUpdatedOnDraftFeature")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Diagram")
     .Accepts(typeof(List<PositionUpdatedOnDraftFeatureBody>), false, "application/json");

  }

  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeatureDiagramPositionUpdated>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, Position position, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<DraftFeatureComponentAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new DraftFeatureDiagramPositionUpdated(new BpnPosition(taskId, position)));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }


}
