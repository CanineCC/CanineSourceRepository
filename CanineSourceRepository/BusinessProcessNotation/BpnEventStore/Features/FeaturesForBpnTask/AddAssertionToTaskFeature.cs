//namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;

//public class AddAssertionToTaskFeature : IFeature
//{
//  //public record DraftFeatureCreated(Guid ContextId, Guid FeatureId, string Name, string Objective, string FlowOverview);
//  public record AddAssertionToTaskBody(Guid BpnContextId, Guid FeatureId, Guid TaskId /*TODO*/);
//  public static void RegisterBpnEventStore(WebApplication app)
//  {
//    //app.MapPost($"BpnEngine/v1/DraftFeature/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddDraftFeatureBody request, CancellationToken ct) =>
//    //{
//    //  var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/Add", request.BpnContextId, request.Name, request.Objective, request.FlowOverview, ct);
//    //  return Results.Ok(id);
//    //}).WithName("AddDraftFeature")
//    // .Produces(StatusCodes.Status200OK)
//    // .WithTags("DraftFeature")
//    // .Accepts(typeof(AddDraftFeatureBody), false, "application/json");

//  }
//  public static void RegisterBpnEvents(StoreOptions options)
//  {
//    //    options.Events.AddEventType<DraftFeatureCreated>();
//  }
//  public static async Task Execute(IDocumentSession session, string causationId, CancellationToken ct)
//  {
//    //var featureId = Guid.CreateVersion7();
//    //
//    //var @event = new DraftFeatureCreated(
//    //ContextId: bpnContextId,
//    //FeatureId: featureId,
//    //Name: name,
//    //Objective: objective,
//    //FlowOverview: flowOverview);
//    //await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
//    //
//    //return featureId;
//  }
//}
