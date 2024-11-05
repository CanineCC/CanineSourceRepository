using System.ComponentModel.DataAnnotations;
using Environment = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component.Environment;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature;

public class UpdateEnvironmentsOnFeatureFeature : IFeature
{
  public record EnvironmentsUpdated(Guid ContextId, Guid FeatureId, long FeatureRevision, Environment[] Environment);
  public class UpdateEnvironmentsOnFeatureBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public long FeatureRevision { get; set; }

    [Required]
    public Environment[] Environment { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPatch($"BpnEngine/v1/Feature/UpdateEnvironment", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateEnvironmentsOnFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/Feature/UpdateEnvironment", request.FeatureId, request.FeatureRevision, request.Environment, ct);
      return Results.Ok(id);
    }).WithName("UpdateEnvironmentsOnFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("Feature")
     .Accepts(typeof(UpdateEnvironmentsOnFeatureBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<EnvironmentsUpdated>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, long featureRevision, Environment[] environment, CancellationToken ct)
  {
    var draftAggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (draftAggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    var aggregate = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new EnvironmentsUpdated(
      ContextId: draftAggregate.ContextId,
      FeatureId: featureId,
      FeatureRevision: featureRevision,
      Environment: environment));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }

}
