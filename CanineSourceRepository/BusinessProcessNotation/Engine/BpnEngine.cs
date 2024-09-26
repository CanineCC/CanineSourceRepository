using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using EngineEvents;
using Marten;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace CanineSourceRepository.BusinessProcessNotation.Engine;

public static class BpnEngine
{
  public static WebApplication RegisterAll(this WebApplication app)
  {
    var features = BpnFeatureRepository.All();
    foreach (var feature in features)
    {
      var newestVersion = features.Where(p => feature.Id == p.Id).Max(p => p.Version);

      var assembly = feature.ToAssembly();

      if (newestVersion == feature.Version)
      {
        AddEnpoint(app, $"{feature.Name.ToPascalCase()}", feature, assembly);
      }
      AddEnpoint(app, $"{feature.Name.ToPascalCase()}/v{feature.Version}", feature, assembly);
    };


    return app;
  }

  private static void AddEnpoint(WebApplication app, string name, BpnFeature feature, Assembly assembly)
  {
    var startTask = feature.Tasks.First();
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
        await Run(session, ct, input, feature, assembly, null, null);
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

  public static async Task<bool> Run(IDocumentSession session, CancellationToken ct, dynamic inputJson, BpnFeature feature, Assembly assembly, Guid? correlationId, BpnTask? nextTask = null)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    var invocationEvents = new List<IEvent>();
    var initial = nextTask == null;
    if (correlationId == null)
    {
      correlationId = Guid.CreateVersion7();
    }
    if (nextTask == null)
    {
      nextTask = feature.Tasks.First();
      invocationEvents.Add(new FeatureStarted(DateTime.UtcNow, feature.Id, feature.Version, correlationId.Value));
    }

    ServiceInjection DiService = nextTask.GetServiceDependency();

    //consider if we can start a transaction in this scope, if it runs across multiple nodes
    var response = await DiService.Execute(session, ct, inputJson, nextTask, correlationId.Value, assembly);

    if (response.Success == false) return false;
    var success = true;

    foreach (var featureTransition in feature.Transitions.Where(p => p.FromBPN == nextTask.Id))
    {//HOWTO FAN OUT?

      if (featureTransition.ConditionIsMeet(response.Result, assembly))
      {
        invocationEvents.Add(new TransitionUsed(nextTask.Id, featureTransition.ToBPN));
        var mapToTask = feature.Tasks.FirstOrDefault(task => task.Id == featureTransition.ToBPN);
        if (mapToTask == null)
        {
          invocationEvents.Add(new FeatureError(new ErrorEvent($"Critical feature error, the node:'{featureTransition.ToBPN}' from transition does not exist", string.Empty)));
          continue;
        }

        var map = featureTransition.MapObject(response.Result, mapToTask.GetCompiledType(assembly));
        if (await Run(session, ct, map, feature, assembly, correlationId, mapToTask) == false)
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
      invocationEvents.Add(new BpnFeatureCompleted(DateTime.UtcNow, stopwatch.Elapsed));
      stopwatch.Stop();
    }

    await session.RegisterEvents(ct, correlationId.Value, invocationEvents.ToArray());
    return success;
  }
}