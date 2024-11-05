using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature;

public class ReleaseFeatureFeature : IFeature
{
  public record FeatureReleased(Guid ContextId, Guid FeatureId, string ReleasedBy, string Name, string Objective, string FlowOverview, ImmutableList<BpnTask> Tasks, ImmutableList<BpnTransition> Transitions, BpnFeatureDiagram Diagram, long Revision);
  public class ReleaseFeatureBody
  {
    [Required]
    public Guid FeatureId { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    string user = "update to user from bearer";
    app.MapPost($"BpnEngine/v1/DraftFeature/Release", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] ReleaseFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Release", request.FeatureId, user, ct);

      return Results.Ok(id);
    }).WithName("ReleaseFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(ReleaseFeatureBody), false, "application/json");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<FeatureReleased>();
  }

  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, string user, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    var result = aggregate.IsValid();
    if (result.IsValid == false) return result;

    var feature = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);

    Dictionary<Guid, Guid> newIds = aggregate.Tasks.ToDictionary(task => task.Id, task => Guid.CreateVersion7());

    var bpnPositions = aggregate.Diagram.BpnPositions.Select(task => task with { Id = newIds[task.Id] }).ToList();
    var bpnWaypoints = aggregate.Diagram.BpnConnectionWaypoints.Select(transition => transition with { FromBPN = newIds[transition.FromBPN], ToBPN = newIds[transition.ToBPN] }).ToList();

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new FeatureReleased(
        ContextId: aggregate.ContextId,
        FeatureId: featureId,
        ReleasedBy: user,
        Name: aggregate.Name,
        Objective: aggregate.Objective,
        FlowOverview: aggregate.FlowOverview,
        Tasks: aggregate.Tasks.Select(task => {
            task.Id = newIds[task.Id];
            return task;
        }).ToImmutableList(),
        Transitions: aggregate.Transitions.Select(transition => transition with { FromBPN = newIds[transition.FromBPN], ToBPN = newIds[transition.ToBPN] }).ToImmutableList(),
        Diagram: new BpnFeatureDiagram() { BpnPositions = bpnPositions, BpnConnectionWaypoints = bpnWaypoints },
        feature?.Revision + 1 ?? 1)); 
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
