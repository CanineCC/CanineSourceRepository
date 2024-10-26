using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature;

public class UpdateDraftFeaturePurposeFeature : IFeature
{
  public record DraftFeaturePurposeChanged(Guid ContextId, Guid FeatureId, string Name, string Objective, string FlowOverview);
  public class UpdateDraftFeaturePurposeBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Objective { get; set; }

    [Required]
    public string FlowOverview { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPatch($"BpnEngine/v1/DraftFeature/UpdatePurpose", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateDraftFeaturePurposeBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/UpdatePurpose", request.FeatureId, request.Name, request.Objective, request.FlowOverview, ct);
      return Results.Ok(id);
    }).WithName("UpdateDraftFeaturePurpose")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(UpdateDraftFeaturePurposeBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeaturePurposeChanged>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, string name, string objective, string flowOverview, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new DraftFeaturePurposeChanged(
      ContextId: aggregate.ContextId,
      FeatureId: featureId,
      Name: name,
      Objective: objective,
      FlowOverview: flowOverview));

    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
