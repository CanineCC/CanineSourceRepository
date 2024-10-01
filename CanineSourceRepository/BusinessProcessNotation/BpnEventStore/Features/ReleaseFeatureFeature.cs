using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class ReleaseFeatureFeature : IFeature
{
  public record Request(Guid FeatureId);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    string user = "TODO";
    app.MapPost($"BpnEngine/v1/DraftFeature/Release", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] Request request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Release", request.FeatureId, user, ct);
      return Results.Ok(id);
    }).WithName("ReleaseFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature", "Feature")
     .Accepts(typeof(Request), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<BpnFeatureProjection.BpnFeature.FeatureReleased>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, string user, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    var result = aggregate.IsValid();
    if (result.IsValid == false) return result;

    var feature = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);

    Dictionary<Guid, Guid> newIds = aggregate.Tasks.ToDictionary(task => task.Id, task => Guid.CreateVersion7());

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new BpnFeatureProjection.BpnFeature.FeatureReleased(
      ReleasedBy: user,
      Name: aggregate.Name,
      Objective: aggregate.Objective,
      FlowOverview: aggregate.FlowOverview,
      Tasks: aggregate.Tasks.Select(task => task with { Id = newIds[task.Id] }).ToImmutableList(),
      Transitions: aggregate.Transitions.Select(transition => transition with { FromBPN = newIds[transition.FromBPN], ToBPN = newIds[transition.ToBPN] }).ToImmutableList(),
      Diagram: aggregate.Diagram,
      feature?.Revision + 1 ?? 1));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
