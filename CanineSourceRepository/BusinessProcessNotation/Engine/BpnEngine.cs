using EngineEvents;
using Marten.Events.Projections;
using BpnTask = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.BpnTask;

namespace CanineSourceRepository.BusinessProcessNotation.Engine;

public static class BpnEngine
{
  public static readonly JsonSerializerOptions BpnJsonSerialize = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters =
    {
      new JsonStringEnumConverter(),
   //   new BpnConverter(),
      new EventLogJsonConverter()
    }
  };
  public static readonly string CodeNamespace = "CanineSourceRepository";

  public static void RegisterBpnEngine(this StoreOptions options)
  {
    options.Projections.LiveStreamAggregation<FeatureInvocationAggregate>();
  //  options.Projections.Add<FeatureInvocationProjection>(ProjectionLifecycle.Async);
 //   options.Schema.For<FeatureInvocationProjection.FeatureInvocation>().Index(x => x.FeatureId);
    options.Events.AddEventType<BpnFeatureStarted>();
    options.Events.AddEventType<BpnFeatureError>();
    options.Events.AddEventType<BpnTaskInitialized>();
    options.Events.AddEventType<BpnTaskFailed>();
    options.Events.AddEventType<BpnFailedTaskReInitialized>();
    options.Events.AddEventType<BpnTaskSucceeded>();
    options.Events.AddEventType<BpnTransitionUsed>();
    options.Events.AddEventType<BpnTransitionSkipped>();
  }

  public record StackTrace(Guid CorrelationId, StackElement[] Trace, string UserInformation, DateTimeOffset Timestamp);
  public record StackElement(Guid BpnId, string Name, long Version, DateTimeOffset Timestamp, TimeSpan Duration, string DataInput);
  public record UserContext(string UserId, string UserName, string[] AccessScopes, string IpAddress, bool IsAuthenticated, string AuthenticationType, DateTime? TokenExpiry);

  public static WebApplication RegisterAll(this WebApplication app, IDocumentSession session)
  {
    var contexts = session.Query<BpnBpnWebApiContainerProjection.BpnWebApiContainer>().ToList();
    foreach (var context in contexts)
    {
      var usedIds = context.Features.Select(feature => feature.Id).ToList();
      var features = session.Query<FeatureComponentProjection.BpnFeature>().Where(p=> usedIds.Contains(p.Id)).ToList();
      foreach (var feature in features)
      {
        var assembly = feature.ToAssembly();

        foreach (var version in feature.Revisions)
        {
          AddEnpoint(app, $"Commands/v{version.Revision}/{version.Name.ToPascalCase()}", context, feature, version, assembly);
        }
      };
    }
    return app;
  }

  public static string[] PotentialApiRevisions => Enumerable.Range(1, 999).Select(i => $"v{i}") .ToArray();  

  public static string[] ApiRevision(IDocumentSession session)
  {
    return session.Query<FeatureComponentProjection.BpnFeature>().ToList().SelectMany(feat => feat.Revisions.Select(p=> $"v{p.Revision}")).Distinct().ToArray();
  }

  private static void AddEnpoint(WebApplication app, string name,
    BpnBpnWebApiContainerProjection.BpnWebApiContainer bpnWebApiContainer, BpnFeature feature,
    BpnFeatureRevision revision, Assembly assembly)
  {
    var groupName = bpnWebApiContainer.Name.ToPascalCase();

    var startTask = revision.Tasks.First();
    var inputType = startTask.GetCompiledType(assembly);
    app.MapPost(name,
        async Task<IResult> (HttpContext context, [FromServices] IDocumentSession session, CancellationToken ct) =>
        {
//TODO:: Add real personas
          var roleFromJwt = "test".ToPascalCase();
          var hasAccess = bpnWebApiContainer.AllowAnonymous || 
                          (bpnWebApiContainer.Personas
                            .FirstOrDefault(persona => persona.Name.ToPascalCase() == roleFromJwt)?.Components
                            .Contains(feature.Id) ??
                          false);

          object? input = null;
          using (var reader = new StreamReader(context.Request.Body))
          {
            var body = await reader.ReadToEndAsync();
            input = JsonSerializer.Deserialize(body, inputType)!;
          }

          try
          {
            await Run(session, ct, input, bpnWebApiContainer.Id, feature, revision, assembly, null, null);
            return Results.Accepted();
          }
          catch (UnauthorizedAccessException)
          {
            return Results.Unauthorized();
          }
          catch (ArgumentException ex)
          {
            return Results.BadRequest(ex.Message);
          }
          catch (Exception)
          {
            return Results.InternalServerError();
          }
        }).WithName($"{bpnWebApiContainer.Name.ToPascalCase()}/{name}") // Set the operation ID to the feature's name
      //.WithGroupName(groupName)
      .WithTags(groupName)
      .Produces(StatusCodes.Status202Accepted) // Specify return types
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status500InternalServerError)
      .Accepts(startTask.GetCompiledType(assembly), false, "application/json"); // Define input content type
  }

  public static async Task<bool> Run(
    IDocumentSession session, 
    CancellationToken ct, 
    dynamic inputJson, 
    Guid containerId, 
    BpnFeature feature, 
    FeatureComponentProjection.BpnFeatureRevision revision, 
    Assembly assembly, 
    Guid? correlationId, 
    BpnTask? nextTask = null)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    var initial = nextTask == null;
    if (correlationId == null)
    {
      correlationId = Guid.CreateVersion7();
    }
    if (nextTask == null)
    {
      nextTask = revision.Tasks.First();
      EngineEventsQueue.EnqueueEngineEvents(new BpnFeatureStarted(containerId, feature.Id, revision.Revision, DateTime.UtcNow, correlationId.Value));
    }

    ServiceInjection DiService = nextTask.GetServiceDependency();

    //consider if we can start a transaction in this scope, if it runs across multiple nodes
    var response = await DiService.Execute(session, ct, inputJson, containerId, feature.Id, revision.Revision, nextTask, correlationId.Value, assembly);//version!?

    if (response.Success == false) return false;
    var success = true;

    foreach (var featureTransition in revision.Transitions.Where(p => p.FromBPN == nextTask.Id))
    {//HOWTO FAN OUT?

      if (featureTransition.ConditionIsMeet(response.Result, assembly))
      {
        EngineEventsQueue.EnqueueEngineEvents(new BpnTransitionUsed(correlationId.Value, containerId, feature.Id, revision.Revision, nextTask.Id, featureTransition.ToBPN));
        var mapToTask = revision.Tasks.FirstOrDefault(task => task.Id == featureTransition.ToBPN);
        if (mapToTask == null)
        {
          EngineEventsQueue.EnqueueEngineEvents(new BpnFeatureError(correlationId.Value, containerId, feature.Id, revision.Revision, new ErrorEvent($"Critical feature error, the node:'{featureTransition.ToBPN}' from transition does not exist", string.Empty)));
          continue;
        }

        var map = featureTransition.MapObject(response.Result, mapToTask.GetCompiledType(assembly));
        if (await Run(session, ct, map, containerId, feature, revision, assembly, correlationId, mapToTask) == false)
        {
          success = false;
        }
      }
      else
      {
        EngineEventsQueue.EnqueueEngineEvents(new BpnTransitionSkipped(correlationId.Value, containerId, feature.Id, revision.Revision, nextTask.Id, featureTransition.ToBPN));
      }
    }
    if (initial && success)
    {
      EngineEventsQueue.EnqueueEngineEvents(new BpnFeatureCompleted(correlationId.Value, containerId, feature.Id, revision.Revision, DateTime.UtcNow, stopwatch.Elapsed.TotalMilliseconds));
      stopwatch.Stop();
    }

    return success;
  }
}