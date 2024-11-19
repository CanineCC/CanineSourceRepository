
namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.NamedConfigurationFeatures;

public class AddNamedConfigurationFeature : IFeature
{
  public record NamedConfigurationAdded(Guid NamedConfigurationId, Guid SystemId, Guid ServiceTypeId, string Name, string Description, Scope Scope, Dictionary<string,string> Configuration);
  public record AddNamedConfigurationBody
  {
    [Required] public Guid SystemId { get; set; } 
    [Required] public Guid ServiceTypeId { get; set; }
    [Required] public string Name { get; set; } = "";
    [Required] public string Description { get;  set; }= "";
    [Required] public Scope Scope { get; set;  } 

    [Required] public Dictionary<string,string> Configuration { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/NamedConfiguration/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddNamedConfigurationBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/NamedConfiguration/Add", request.SystemId, request.ServiceTypeId, request.Name, request.Description, request.Scope, request.Configuration, ct);
      return Results.Ok(id);
    }).WithName("AddNamedConfiguration")
     .Produces(StatusCodes.Status200OK)
     .WithTags("NamedConfiguration")
     .Accepts(typeof(AddNamedConfigurationBody), false, "application/json");

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<NamedConfigurationAdded>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, Guid systemId, Guid serviceTypeId, string name, string description, Scope scope, Dictionary<string,string> configuration, CancellationToken ct)
  {
    var id = Guid.CreateVersion7();

    var @event = new NamedConfigurationAdded(
      NamedConfigurationId: id,
      SystemId: systemId,
      ServiceTypeId: serviceTypeId,
      Name: name,
      Description: description,
      Scope: scope,
      Configuration: configuration
    );
    await session.RegisterEventsOnNamedConfiguration(ct, id, causationId, @event);
    return id;
  }
}