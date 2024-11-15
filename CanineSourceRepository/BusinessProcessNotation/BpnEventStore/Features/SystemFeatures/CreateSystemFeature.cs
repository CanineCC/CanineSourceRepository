using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.SystemFeatures;

public class CreateSystemFeature : IFeature
{
  public record SystemCreated(Guid Id, Guid SolutionId, string Name, string Description);
  public class CreateSystemBody
  {
    public CreateSystemBody(Guid solutionId,string name, string description)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Description = description ?? throw new ArgumentNullException(nameof(description));
      SolutionId = solutionId;
    }
    [Required]
    public Guid SolutionId { get; set; }

    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/System/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CreateSystemBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/System/Add", request.SolutionId, request.Name, request.Description,ct);
      return Results.Ok(id);
    }).WithName("CreateSystem")
     .Produces(StatusCodes.Status200OK)
     .WithTags("System")
     .Accepts(typeof(CreateSystemBody), false, "application/json"); // Define input content type

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<SystemCreated>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, Guid solutionId, string name,string description, CancellationToken ct)
  {
    var newId = Guid.CreateVersion7();
    await session.RegisterEventsOnBpnContext(ct, newId, causationId, new SystemCreated(Id: newId,SolutionId:solutionId, Name: name, Description: description));
    return newId;
  }
}