﻿using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureDiagram;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class WaypointUpdatedOnDraftFeatureFeature : IFeature
{
  public record Request(Guid FeatureId, Guid FromTaskId, Guid ToTaskId, Position[] Positions);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPatch($"BpnEngine/v1/DraftFeature/Diagram/WaypointUpdated", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] Request request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Diagram/WaypointUpdated", request.FeatureId, request.FromTaskId, request.ToTaskId, request.Positions, ct);
      return Results.Ok(id);
    }).WithName("WaypointUpdatedOnDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature.Diagram")
     .Accepts(typeof(Request), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeatureDiagramWaypointUpdated>();
  }


  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, Guid fromTaskId, Guid toTaskId, Position[] positions, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new DraftFeatureDiagramWaypointUpdated(new ConnectionWaypoints(fromTaskId, toTaskId, positions)));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }


}
