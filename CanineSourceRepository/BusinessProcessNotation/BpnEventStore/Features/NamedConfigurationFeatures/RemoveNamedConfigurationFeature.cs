
using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.NamedConfigurationFeatures;

public class RemoveNamedConfigurationFeature : IFeature
{
  public record NamedConfigurationRemoved(Guid NamedConfigurationId, Guid SystemId, Guid ServiceTypeId);
  public record RemoveNamedConfigurationBody
  {
    [Required] public Guid NamedConfigurationId { get; set; } 
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapDelete($"BpnEngine/v1/NamedConfiguration/Remove", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] RemoveNamedConfigurationBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/NamedConfiguration/Remove", request.NamedConfigurationId,ct);
      return Results.Accepted();
    }).WithName("RemoveNamedConfiguration")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("NamedConfiguration")
     .Accepts(typeof(RemoveNamedConfigurationBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<NamedConfigurationRemoved>();
  }
  public static async Task  Execute(IDocumentSession session, string causationId, Guid namedConfigurationId, CancellationToken ct)
  {
    var namedConfigurationAggregate = await session.Events.AggregateStreamAsync<NamedConfigurationAggregate>(namedConfigurationId, token: ct);
    if (namedConfigurationAggregate == null) throw new Exception($"Named configuration '{namedConfigurationId}' was not found");
    
    var @event = new NamedConfigurationRemoved(
      NamedConfigurationId: namedConfigurationId,
      SystemId: namedConfigurationAggregate.SystemId,
      ServiceTypeId: namedConfigurationAggregate.ServiceTypeId
    );
    await session.RegisterEventsOnNamedConfiguration(ct, namedConfigurationId, causationId, @event);
  }
}