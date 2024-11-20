using System.ComponentModel.DataAnnotations;
using BpnTask = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.BpnTask;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.DraftComponentFeatures;

public class ReleaseFeatureFeature : IFeature
{
  public record FeatureReleased(
    Guid ContainerId, 
    Guid FeatureId, 
    string ReleasedBy, 
    string Name, 
    string Objective, 
    string FlowOverview, 
    ImmutableList<BpnTask> Tasks, 
    ImmutableList<BpnTransition> Transitions, 
    FeatureComponentDiagram ComponentDiagram, 
    long Revision);
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
    var aggregate = await session.Events.AggregateStreamAsync<DraftFeatureComponentAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    var result = aggregate.IsValid();
    if (result.IsValid == false) return result;

    var feature = await session.Events.AggregateStreamAsync<FeatureComponentAggregate>(featureId, token: ct);

    Dictionary<Guid, Guid> newIds = aggregate.Tasks.ToDictionary(task => task.Id, task => Guid.CreateVersion7());

    var bpnPositions = aggregate.ComponentDiagram.BpnPositions.Select(task => task with { Id = newIds[task.Id] }).ToList();
    var bpnWaypoints = aggregate.ComponentDiagram.BpnConnectionWaypoints.Select(transition => transition with { FromBPN = newIds[transition.FromBPN], ToBPN = newIds[transition.ToBPN] }).ToList();

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new FeatureReleased(
        ContainerId: aggregate.ContainerId,
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
        ComponentDiagram: new FeatureComponentDiagram() { BpnPositions = bpnPositions, BpnConnectionWaypoints = bpnWaypoints },
        feature?.Revision + 1 ?? 1)); 
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
