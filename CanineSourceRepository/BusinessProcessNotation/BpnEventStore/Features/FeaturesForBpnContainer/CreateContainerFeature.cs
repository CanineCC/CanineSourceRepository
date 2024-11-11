using System.ComponentModel.DataAnnotations;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnContext;

public class CreateContainerFeature : IFeature
{
  public record WebApiContainerCreated(Guid Id, Guid SystemId, string SystemName, string Name, string Descrption);
  public class CreateContainerBody
  {
    public CreateContainerBody(string name, string description, Guid systemId)
    {
      SystemId = systemId;
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    [Required]
    public Guid SystemId { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/Container/Add", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CreateContainerBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/Container/Add", request.Name,  request.Description,request.SystemId, ct);
      return Results.Ok(id);
    }).WithName("CreateContainer")
     .Produces(StatusCodes.Status200OK)
     .WithTags("Container")
     .Accepts(typeof(CreateContainerBody), false, "application/json"); // Define input content type

  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<WebApiContainerCreated>();
  }
  public static async Task<Guid> Execute(IDocumentSession session, string causationId, string name, string description, Guid systemId, CancellationToken ct)
  {
    var system = await session.Events.AggregateStreamAsync<BpnSystemAggregate>(systemId, token: ct);
    var newId = Guid.CreateVersion7();
    await session.RegisterEventsOnBpnContext(ct, newId, causationId, new WebApiContainerCreated(Id: newId, Name: name, Descrption:description, SystemId: systemId, SystemName: system.Name));
    return newId;
  }
}
