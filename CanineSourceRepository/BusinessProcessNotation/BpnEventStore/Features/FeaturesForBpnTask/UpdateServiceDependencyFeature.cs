namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;

public class UpdateServiceDependencyFeature : IFeature
{
  public record ServiceDependencyUpdated(Guid FeatureId, Guid TaskId, string ServiceDependency, string NamedConfiguration);
  public record UpdateServiceDependencyBody(Guid FeatureId, Guid TaskId, string ServiceDependency, string NamedConfiguration);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPut($"BpnEngine/v1/DraftFeature/UpdateServiceDependency", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateServiceDependencyBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/UpdateServiceDependency", request.FeatureId, request.TaskId, request.ServiceDependency, request.NamedConfiguration, ct);
      return Results.Accepted();
    }).WithName("UpdateServiceDependencyFeature")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("DraftFeature.Task")
     .Accepts(typeof(UpdateServiceDependencyBody), false, "application/json");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<ServiceDependencyUpdated>();
  }
  public static async Task Execute(IDocumentSession session, string causationId, Guid featureId, Guid taskId, string serviceDependency, string namedConfiguration, CancellationToken ct)
  {
    //TODO: Check that it exist, otherwise fail
    var @event = new ServiceDependencyUpdated(
    FeatureId: featureId,
    TaskId: taskId,
    ServiceDependency: serviceDependency,
    NamedConfiguration: namedConfiguration);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);
  }
}
