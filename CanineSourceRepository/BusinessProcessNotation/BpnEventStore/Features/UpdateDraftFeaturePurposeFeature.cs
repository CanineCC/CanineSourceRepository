using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class UpdateDraftFeaturePurposeFeature : IFeature
{
  public record Request(Guid FeatureId, string Name, string Objective, string FlowOverview);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPatch($"BpnEngine/v1/DraftFeature/UpdatePurpose", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] Request request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/UpdatePurpose", request.FeatureId, request.Name, request.Objective, request.FlowOverview, ct);
      return Results.Ok(id);
    }).WithName("UpdateDraftFeaturePurpose")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(Request), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnDraftFeatureProjection.BpnDraftFeature.DraftFeaturePurposeChanged>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, string name, string objective, string flowOverview, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeaturePurposeChanged(Name: name, Objective: objective, FlowOverview: flowOverview));

    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
