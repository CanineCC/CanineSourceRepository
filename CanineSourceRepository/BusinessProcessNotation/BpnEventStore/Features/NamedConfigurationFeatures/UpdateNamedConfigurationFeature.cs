
namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.NamedConfigurationFeatures;

public class UpdateNamedConfigurationFeature : IFeature
{
  public record NamedConfigurationUpdated(Guid NamedConfigurationId, Guid SystemId, Guid ServiceTypeId, string Name, string Description, Scope Scope, Dictionary<string,string> Configuration);
  public record UpdateNamedConfigurationBody
  {
    [Required] public Guid NamedConfigurationId { get; set; } 
    [Required] public string Name { get; set; } = "";
    [Required] public string Description { get;  set; }= "";
    [Required] public Scope Scope { get; set;  } 
    [Required] public Dictionary<string,string> Configuration { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPut($"BpnEngine/v1/NamedConfiguration/Update", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] UpdateNamedConfigurationBody request, CancellationToken ct) =>
    {
      await Execute(session, "WebApplication/v1/BpnEngine/NamedConfiguration/Update", request.NamedConfigurationId, request.Name, request.Description, request.Scope, request.Configuration, ct);
      return Results.Accepted();
    }).WithName("UpdateNamedConfiguration")
    .Produces(StatusCodes.Status202Accepted)
     .WithTags("NamedConfiguration")
     .Accepts(typeof(UpdateNamedConfigurationBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<NamedConfigurationUpdated>();
  }
  public static async Task  Execute(IDocumentSession session, string causationId, Guid namedConfigurationId, string name, string description, Scope scope, Dictionary<string,string> configuration, CancellationToken ct)
  {
    var namedConfigurationAggregate = await session.Events.AggregateStreamAsync<NamedConfigurationAggregate>(namedConfigurationId, token: ct);
    if (namedConfigurationAggregate == null) throw new Exception($"Named configuration '{namedConfigurationId}' was not found");

    var @event = new NamedConfigurationUpdated(
      NamedConfigurationId: namedConfigurationId,
      SystemId: namedConfigurationAggregate.SystemId,
      ServiceTypeId: namedConfigurationAggregate.ServiceTypeId,
      Name: name,
      Description: description,
      Scope: scope,
      Configuration: configuration
    );
    await session.RegisterEventsOnNamedConfiguration(ct, namedConfigurationId, causationId, @event);
  }
}