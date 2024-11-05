using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature;

public class AddTransitionToDraftFeatureFeature : IFeature
{
  public record DraftFeatureTransitionAdded(BpnTransition Transition);
  public class AddTransitionToDraftFeatureBody
  {
    [Required]
    public Guid FeatureId { get; set; }

    [Required]
    public BpnTransition Transition { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/AddTransition", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddTransitionToDraftFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/AddTransition", request.FeatureId, request.Transition, ct);
      return Results.Ok(id);
    }).WithName("AddTransitionToDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(AddTransitionToDraftFeatureBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeatureTransitionAdded>();
  }
  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, BpnTransition transition, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new DraftFeatureTransitionAdded(Transition: transition));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
