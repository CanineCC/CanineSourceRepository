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
    var features = BpnRepository.All();
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
    var startNode = feature.Nodes.First();
    var inputType = startNode.GetCompiledType(assembly);
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
      .Accepts(startNode.GetCompiledType(assembly), false, "application/json"); // Define input content type
  }

  public static async Task<bool> Run(IDocumentSession session, CancellationToken ct, dynamic inputJson, BpnFeature feature, Assembly assembly, Guid? correlationId, Bpn? nextNode = null)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    var invocationEvents = new List<IEvent>();
    var initial = nextNode == null;
    if (correlationId == null)
    {
      correlationId = Guid.CreateVersion7();
    }
    if (nextNode == null)
    {
      nextNode = feature.Nodes.First();
      invocationEvents.Add(new BpnFeatureStarted(DateTime.UtcNow, feature.Id, feature.Version, correlationId.Value));
    }

    NodeWrapper DiNode = new();
    if (nextNode.ServiceDependency == Bpn.ServiceInjection.PostgreSql)
    {
      DiNode = new NodePostgreSqlWrapper(); //check the nextNode for DI setting, in order to create the correct wrapper for the node!
    }
    //consider if we can start a transaction in this scope, if it runs across multiple nodes
    var response = await DiNode.Execute(session, ct, inputJson, nextNode, correlationId.Value, assembly);

    if (response.Success == false) return false;
    var success = true;

    foreach (var featureConnection in feature.Connections.Where(p => p.FromBPN == nextNode.Id))
    {//HOWTO FAN OUT?

      if (featureConnection.ConditionIsMeet(response.Result, assembly))
      {
        invocationEvents.Add(new ConnectionUsed(nextNode.Id, featureConnection.ToBPN));
        var mapToNode = feature.Nodes.FirstOrDefault(node => node.Id == featureConnection.ToBPN);
        if (mapToNode == null)
        {
          invocationEvents.Add(new BpnFeatureError(new ErrorEvent($"Critical feature error, the node:'{featureConnection.ToBPN}' from connection does not exist", string.Empty)));
          continue;
        }

        var map = featureConnection.MapObject(response.Result, mapToNode.GetCompiledType(assembly));
        if (await Run(session, ct, map, feature, assembly, correlationId, mapToNode) == false)
        {
          success = false;
        }
      }
      else
      {
        invocationEvents.Add(new ConnectionSkipped(nextNode.Id, featureConnection.ToBPN));
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


public record NodeResult(bool Success, dynamic? Result);

//TODO: other DIs (pop3, smtp, restapi, eventsourced/marten etc.)
public class NodePostgreSqlWrapper : NodeWrapper
{
  public override async Task<NodeResult> Execute(IDocumentSession session, CancellationToken ct, dynamic inputJson, Bpn node, Guid correlationId, Assembly assembly)
  {
    await using var connection = new Npgsql.NpgsqlConnection("YourConnectionStringHere");
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync();

    var res = await RunAndLog(session, ct, inputJson, node, correlationId, assembly, connection);
    if (res.success)
    {
      await transaction.CommitAsync(); // Commit transaction on success
    }
    else
    {
      await transaction.RollbackAsync(); // Rollback transaction on failure
    }

    return res;
  }
}








public class NodeWrapper
{
  public virtual async Task<NodeResult> Execute(IDocumentSession session, CancellationToken ct, dynamic inputJson, Bpn node, Guid correlationId, Assembly assembly)
  {
    return await RunAndLog(session, ct, inputJson, node, correlationId, assembly);
  }

  public async Task<NodeResult>
    RunAndLog(IDocumentSession session, CancellationToken ct, dynamic input, Bpn node, Guid correlationId, Assembly assembly, object? dependencyService = null)
  {

    var invocationEvents = new List<IEvent>();
    var stopwatch = Stopwatch.StartNew();
    dynamic? result = null;
    try
    {
      invocationEvents.Add(new NodeInitialized(node.Id, InputLogger.LogInput(input)));

      var inputValidation = (ValueTuple<bool, List<string>>)node.VerifyInputData(input, assembly);
      var (isOk, missingFields) = inputValidation;

      if (isOk == false)
      {
        invocationEvents.Add(new NodeFailed(node.Id, new ErrorEvent($"Missing input fields for '{node.Name}' ({node.Id}): ", string.Join(",", missingFields)), stopwatch.Elapsed));
        await session.RegisterEvents(ct, correlationId, invocationEvents.ToArray());
        throw new ArgumentException($"Missing input fields: {string.Join(",", missingFields)}");
      }

      switch (node)
      {
        case CodeBlock codeBlock:
          result = await codeBlock.Execute(input, dependencyService, assembly);
          break;
        case ApiInputBlock apiInputBlock://TODO: USER CONTEXT
          result = await apiInputBlock.Execute(input, new UserContext("userId", "userName", ["Anonymous"], "ipaddress", true, "auth type", null), assembly);
          break;
        default:
          invocationEvents.Add(new NodeFailed(node.Id, new ErrorEvent($"Execution for nodetype '{node.GetTypeName()}'is not implemented", string.Empty), stopwatch.Elapsed));
          await session.RegisterEvents(ct, correlationId, invocationEvents.ToArray());
          return new NodeResult(false, result);
      }

      invocationEvents.Add(new NodeSucceeded(node.Id, stopwatch.Elapsed));
    }
    catch (Exception ex)
    {
      if (ex.InnerException != null)
      {
        invocationEvents.Add(new NodeFailed(node.Id, new ErrorEvent(ex.InnerException.Message, ex.InnerException.StackTrace ?? ""), stopwatch.Elapsed));
        await session.RegisterEvents(ct, correlationId, invocationEvents.ToArray());

        if (ex.InnerException is UnauthorizedAccessException)
        {
          throw ex.InnerException;
        }
      }
      else
      {
        invocationEvents.Add(new NodeFailed(node.Id, new ErrorEvent(ex.Message, ex.StackTrace ?? ""), stopwatch.Elapsed));
        await session.RegisterEvents(ct, correlationId, invocationEvents.ToArray());
        throw;
      }
      return new NodeResult(false, result);
    }
    await session.RegisterEvents(ct, correlationId, invocationEvents.ToArray());
    return new NodeResult(true, result);
  }
}
