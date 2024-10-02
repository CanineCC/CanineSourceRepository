using CanineSourceRepository.BusinessProcessNotation.BpnContext;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using EngineEvents;
using Marten.Events.Projections;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureProjection;

namespace CanineSourceRepository.BusinessProcessNotation.Engine;

public static class BpnEngine
{
  public static readonly JsonSerializerOptions bpnJsonSerialize = new()
  {
    Converters = { new JsonStringEnumConverter(), new BpnConverter(), new EventLogJsonConverter() }
  };
  public static readonly string CodeNamespace = "CanineSourceRepository";

  public static void RegisterBpnEngine(this StoreOptions options)
  {
    options.Projections.LiveStreamAggregation<FeatureInvocationAggregate>();
    options.Projections.Add<FeatureInvocationProjection>(ProjectionLifecycle.Async);
    options.Schema.For<FeatureInvocationProjection.FeatureInvocation>().Index(x => x.FeatureId);
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
    //TODO: Get context also, use context to group the enpoints!
    //TODO: Ensure unique names (i.e. no two features are allowed to have same name within a context)
    var contexts = session.Query<BpnContextProjection.BpnContext>().ToList();
    foreach (var context in contexts)
    {
      var usedIds = context.Features.Select(feature => feature.Id).ToList();
      var features = session.Query<BpnFeatureProjection.BpnFeature>().Where(p=> usedIds.Contains(p.Id)).ToList();
      foreach (var feature in features)
      {
        var assembly = feature.ToAssembly();

        foreach (var version in feature.Versions)
        {
          AddEnpoint(app, $"Commands/v{version.Revision}/{version.Name.ToPascalCase()}", context, feature, version, assembly);
        }
      };
    }


    return app;
  }

  public static string[] PotentialApiVersions => Enumerable.Range(1, 999).Select(i => $"v{i}") .ToArray();  

  public static string[] ApiVersions(IDocumentSession session)
  {
    return session.Query<BpnFeatureProjection.BpnFeature>().ToList().SelectMany(feat => feat.Versions.Select(p=> $"v{p.Revision}")).Distinct().ToArray();
  }

  private static void AddEnpoint(WebApplication app, string name, BpnContextProjection.BpnContext bpnContext, BpnFeature feature, BpnFeatureProjection.BpnFeatureVersion version, Assembly assembly)
  {
    var groupName = bpnContext.Name.ToPascalCase();

    var startTask = version.Tasks.First();
    var inputType = startTask.GetCompiledType(assembly);
    app.MapPost(name, async Task<IResult> (HttpContext context, [FromServices]IDocumentSession session, CancellationToken ct) =>
    {
      object? input = null;
      using (var reader = new StreamReader(context.Request.Body))
      {
        var body = await reader.ReadToEndAsync();
        input = JsonSerializer.Deserialize(body, inputType)!;
      }

      try
      {
        await Run(session, ct, input, bpnContext.Id, feature, version, assembly, null, null);
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
    }).WithName(name) // Set the operation ID to the feature's name
      //.WithGroupName(groupName)
      .WithTags(groupName)
      .Produces(StatusCodes.Status202Accepted) // Specify return types
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status500InternalServerError)
      .Accepts(startTask.GetCompiledType(assembly), false, "application/json"); // Define input content type
  }

  public static async Task<bool> Run(IDocumentSession session, CancellationToken ct, dynamic inputJson, Guid contextId, BpnFeature feature, BpnFeatureProjection.BpnFeatureVersion version, Assembly assembly, Guid? correlationId, BpnTask? nextTask = null)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    var invocationEvents = new List<IEngineEvents>();
    var initial = nextTask == null;
    if (correlationId == null)
    {
      correlationId = Guid.CreateVersion7();
    }
    if (nextTask == null)
    {
      nextTask = version.Tasks.First();
      invocationEvents.Add(new BpnFeatureStarted(contextId, feature.Id, version.Revision, DateTime.UtcNow, correlationId.Value));
    }

    ServiceInjection DiService = nextTask.GetServiceDependency();

    //consider if we can start a transaction in this scope, if it runs across multiple nodes
    var response = await DiService.Execute(session, ct, inputJson, contextId, feature.Id, version.Revision, nextTask, correlationId.Value, assembly);//version!?

    if (response.Success == false) return false;
    var success = true;

    foreach (var featureTransition in version.Transitions.Where(p => p.FromBPN == nextTask.Id))
    {//HOWTO FAN OUT?

      if (featureTransition.ConditionIsMeet(response.Result, assembly))
      {
        invocationEvents.Add(new BpnTransitionUsed(contextId, feature.Id, version.Revision, nextTask.Id, featureTransition.ToBPN));
        var mapToTask = version.Tasks.FirstOrDefault(task => task.Id == featureTransition.ToBPN);
        if (mapToTask == null)
        {
          invocationEvents.Add(new BpnFeatureError(contextId, feature.Id, version.Revision, new ErrorEvent($"Critical feature error, the node:'{featureTransition.ToBPN}' from transition does not exist", string.Empty)));
          continue;
        }

        var map = featureTransition.MapObject(response.Result, mapToTask.GetCompiledType(assembly));
        if (await Run(session, ct, map, contextId, feature, version, assembly, correlationId, mapToTask) == false)
        {
          success = false;
        }
      }
      else
      {
        invocationEvents.Add(new BpnTransitionSkipped(contextId, feature.Id, version.Revision, nextTask.Id, featureTransition.ToBPN));
      }
    }
    if (initial && success)
    {
      invocationEvents.Add(new BpnFeatureCompleted(contextId, feature.Id, version.Revision, DateTime.UtcNow, stopwatch.Elapsed.TotalMilliseconds));
      stopwatch.Stop();
    }

    await session.RegisterEvents(ct, correlationId.Value, feature.Id, invocationEvents.ToArray());
    return success;
  }
}