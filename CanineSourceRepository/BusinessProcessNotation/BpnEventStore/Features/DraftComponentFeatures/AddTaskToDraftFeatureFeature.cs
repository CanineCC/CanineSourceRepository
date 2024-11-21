using System.ComponentModel.DataAnnotations;
using CanineSourceRepository.BusinessProcessNotation.Engine;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.DraftComponentFeatures;

public class AddTaskToDraftFeatureFeature : IFeature
{
  public record DraftFeatureTaskAdded(
    Guid SystemId,
    Guid ContainerId,
    Guid FeatureId,
    Guid TaskId,
    string Name,
    string BusinessPurpose,
    string BehavioralGoal,
    string? Input,
    string? Output,
    string? Code,
    //List<TestCase> TestCases,
    Guid? NamedConfigurationId,
    string NamedConfigurationName,
    Guid ServiceTypeId,
    ImmutableList<RecordDefinition> RecordTypes    
  );
  public class AddTaskToDraftFeatureBody
  {
    [Required]
    public Guid FeatureId { get; set; }
    [Required]
    public Task Task { get; set; }
  }

  public class Task
  {
    [Required] public string Name { get; set; }
    [Required] public string BusinessPurpose { get; set; }
    [Required] public string BehavioralGoal { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }
    [Required] public string? Code { get; set; }
    //[Required] public List<TestCase> TestCases { get; set; } = [];
    public Guid? NamedConfigurationId { get; set; }
    [Required] public ImmutableList<RecordDefinition> RecordTypes { get; set; } = [];
  }

  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/DraftFeature/AddTask", async (HttpContext context, [FromServices] IDocumentSession session, [FromBody] AddTaskToDraftFeatureBody request, CancellationToken ct) =>
    {
      var id = await Execute(session, "WebApplication/v1/BpnEngine/DraftFeature/AddTask", request.FeatureId, request.Task, ct);
      return Results.Ok(id);
    }).WithName("AddTaskToDraftFeature")
     .Produces(StatusCodes.Status200OK)
     .WithTags("DraftFeature")
     .Accepts(typeof(AddTaskToDraftFeatureBody), false, "application/json");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
    options.Events.AddEventType<DraftFeatureTaskAdded>();
  }

  public static async Task<Guid> Execute(IDocumentSession session, string causationId, Guid featureId, Task task, CancellationToken ct)
  {
    var featureComponentAggregate = await session.Events.AggregateStreamAsync<DraftFeatureComponentAggregate>(featureId, token: ct);
    if (featureComponentAggregate == null) throw new Exception($"FeatureComponent '{featureId}' was not found");
    
    var webApiContainerAggregate = await session.Events.AggregateStreamAsync<WebApiContainerAggregate>(featureComponentAggregate.ContainerId, token: ct);
    if (webApiContainerAggregate == null) throw new Exception($"WebApiContainer '{featureComponentAggregate.ContainerId}' was not found");
    
    //move named-configuration-to own feature
    var namedConfigurationAggregate = task.NamedConfigurationId == null ? null : await session.Events.AggregateStreamAsync<NamedConfigurationAggregate>(task.NamedConfigurationId.Value, token: ct);
    if (task.NamedConfigurationId != null && namedConfigurationAggregate == null) throw new Exception($"NamedConfiguration '{task.NamedConfigurationId.Value}' was not found");
    
    var aggregate = await session.Events.AggregateStreamAsync<DraftFeatureComponentAggregate>(featureId, token: ct);
    if (aggregate == null) throw new Exception($"Draft feature '{featureId}' was not found");
    var @event = new DraftFeatureTaskAdded(
      SystemId: webApiContainerAggregate.SystemId,
      ContainerId:featureComponentAggregate.ContainerId,
      FeatureId:featureComponentAggregate.Id,
      TaskId: Guid.CreateVersion7(), 
      Name: task.Name, 
      BusinessPurpose: task.BusinessPurpose,
      BehavioralGoal: task.BehavioralGoal, Input: task.Input, Output: task.Output, Code: task.Code,
      NamedConfigurationId: namedConfigurationAggregate?.Id,
      NamedConfigurationName: namedConfigurationAggregate?.Name ?? "",
      ServiceTypeId: namedConfigurationAggregate == null 
        ? ServiceType.ServiceTypes.First(p=>p.InjectedComponent.GetType() == typeof(NoService) ).Id 
        : ServiceType.ServiceTypes.First(p=>p.Id==namedConfigurationAggregate.ServiceTypeId).Id,
      RecordTypes: task.RecordTypes.ToImmutableList()
    );
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, @event);

    return @event.TaskId;
  }
}
