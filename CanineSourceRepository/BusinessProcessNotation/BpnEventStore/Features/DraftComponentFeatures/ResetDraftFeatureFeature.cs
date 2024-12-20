﻿using System.ComponentModel.DataAnnotations;
using BpnTask = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.BpnTask;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.DraftComponentFeatures;

public class ResetDraftFeatureFeature : IFeature
{
  public record DraftFeatureReset(ImmutableList<BpnTask> Tasks, ImmutableList<BpnTransition> Transitions, FeatureComponentDiagram ComponentDiagram);
  public class ResetDraftFeatureBody
  {
    [Required]
    public Guid FeatureId { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/Reset", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] ResetDraftFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Reset", request.FeatureId, ct);
      return Results.Ok(id);
    }).WithName("ResetDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(ResetDraftFeatureBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeatureReset>();
  }
  public static async Task<ValidationResponse> Execute(IDocumentSession session, string causationId, Guid featureId, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<DraftFeatureComponentAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    var feature = await session.Events.AggregateStreamAsync<FeatureComponentAggregate>(featureId, token: ct);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new DraftFeatureReset(
      Tasks: feature?.Tasks ?? [],
      Transitions: feature?.Transitions ?? [],
      ComponentDiagram: feature?.ComponentDiagram ?? new FeatureComponentDiagram()
    ));

    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}
