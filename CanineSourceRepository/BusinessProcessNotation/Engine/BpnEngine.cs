﻿using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using EngineEvents;
using Marten.Events.Projections;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnFeatureProjection;

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
    options.Events.AddEventType<FeatureStarted>();
    options.Events.AddEventType<FeatureError>();
    options.Events.AddEventType<TaskInitialized>();
    options.Events.AddEventType<TaskFailed>();
    options.Events.AddEventType<FailedTaskReInitialized>();
    options.Events.AddEventType<TaskSucceeded>();
    options.Events.AddEventType<TransitionUsed>();
    options.Events.AddEventType<TransitionSkipped>();
  }

  public record StackTrace(Guid CorrelationId, StackElement[] Trace, string UserInformation, DateTimeOffset Timestamp);
  public record StackElement(Guid BpnId, string Name, long Version, DateTimeOffset Timestamp, TimeSpan Duration, string DataInput);
  public record UserContext(string UserId, string UserName, string[] AccessScopes, string IpAddress, bool IsAuthenticated, string AuthenticationType, DateTime? TokenExpiry);

  public static WebApplication RegisterAll(this WebApplication app, IDocumentSession session)
  {
    var features = session.Query<BpnFeatureProjection.BpnFeature>().ToList();
    foreach (var feature in features)
    {
//      var newestVersion = features.Where(p => feature.Id == p.Id).Max(p => p.Version);

      var assembly = feature.ToAssembly();

      foreach (var version in feature.Versions)
      {
        AddEnpoint(app, $"{version.Name.ToPascalCase()}/v{version.Revision}", feature, version, assembly);
      }
      var newest = feature.Versions.Last();
      AddEnpoint(app, $"{newest.Name.ToPascalCase()}", feature, newest, assembly);

    };


    return app;
  }

  private static void AddEnpoint(WebApplication app, string name, BpnFeature feature, BpnFeatureProjection.BpnFeatureVersion version, Assembly assembly)
  {
    var startTask = version.Tasks.First();
    var inputType = startTask.GetCompiledType(assembly);
    app.MapPut(name, async Task<IResult> (HttpContext context, [FromServices]IDocumentSession session, CancellationToken ct) =>
    {
      object? input = null;
      using (var reader = new StreamReader(context.Request.Body))
      {
        var body = await reader.ReadToEndAsync();
        input = JsonSerializer.Deserialize(body, inputType)!;
      }

      try
      {
        await Run(session, ct, input, feature, version, assembly, null, null);
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
      .Produces(StatusCodes.Status202Accepted) // Specify return types
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status500InternalServerError)
      .Accepts(startTask.GetCompiledType(assembly), false, "application/json"); // Define input content type
  }

  public static async Task<bool> Run(IDocumentSession session, CancellationToken ct, dynamic inputJson, BpnFeature feature, BpnFeatureProjection.BpnFeatureVersion version, Assembly assembly, Guid? correlationId, BpnTask? nextTask = null)
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
      invocationEvents.Add(new FeatureStarted(DateTime.UtcNow, feature.Id, version.Revision, correlationId.Value));
    }

    ServiceInjection DiService = nextTask.GetServiceDependency();

    //consider if we can start a transaction in this scope, if it runs across multiple nodes
    var response = await DiService.Execute(session, ct, inputJson, nextTask, correlationId.Value, assembly);//version!?

    if (response.Success == false) return false;
    var success = true;

    foreach (var featureTransition in version.Transitions.Where(p => p.FromBPN == nextTask.Id))
    {//HOWTO FAN OUT?

      if (featureTransition.ConditionIsMeet(response.Result, assembly))
      {
        invocationEvents.Add(new TransitionUsed(nextTask.Id, featureTransition.ToBPN));
        var mapToTask = version.Tasks.FirstOrDefault(task => task.Id == featureTransition.ToBPN);
        if (mapToTask == null)
        {
          invocationEvents.Add(new FeatureError(new ErrorEvent($"Critical feature error, the node:'{featureTransition.ToBPN}' from transition does not exist", string.Empty)));
          continue;
        }

        var map = featureTransition.MapObject(response.Result, mapToTask.GetCompiledType(assembly));
        if (await Run(session, ct, map, feature, version, assembly, correlationId, mapToTask) == false)
        {
          success = false;
        }
      }
      else
      {
        invocationEvents.Add(new TransitionSkipped(nextTask.Id, featureTransition.ToBPN));
      }
    }
    if (initial && success)
    {
      invocationEvents.Add(new BpnFeatureCompleted(DateTime.UtcNow, stopwatch.Elapsed.TotalMilliseconds));
      stopwatch.Stop();
    }

    await session.RegisterEvents(ct, correlationId.Value, feature.Id, invocationEvents.ToArray());
    return success;
  }
}