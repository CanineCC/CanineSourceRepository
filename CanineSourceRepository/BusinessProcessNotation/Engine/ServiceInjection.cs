using EngineEvents;
using static CanineSourceRepository.BusinessProcessNotation.Engine.BpnEngine;

namespace CanineSourceRepository.BusinessProcessNotation.Engine;

public abstract class ServiceInjection
{

  private static Type[] Services = { typeof(NoService), typeof(PostgreSqlService) };
  public static ServiceInjection ServiceLocator(string serviceName, string namedConfiguration)
  {
    var service = Services.FirstOrDefault(p => p.Name == serviceName);

    if (service == null)
    {
      throw new Exception($"Service '{serviceName}' not found.");
    }

    var serviceInstance = (ServiceInjection)Activator.CreateInstance(service)!;
    serviceInstance.Setup(namedConfiguration);
    return serviceInstance!;
  }

  public abstract string Name { get; }
  public abstract string Description { get; }
  public abstract string[] ApiDescription { get; }
  public abstract CodeSnippet[] ApiSnippets { get; }
  public abstract void Setup(string namedConfiguration);

  public abstract Task<TaskResult> Execute(IDocumentSession session, CancellationToken ct, dynamic inputJson, Guid containerId, Guid featureId, long featureVersion, BpnTask task, Guid correlationId, Assembly assembly);
  internal async Task<TaskResult> RunAndLog(IDocumentSession session, CancellationToken ct, dynamic input, Guid containerId, Guid featureId, long featureVersion, BpnTask task, Guid correlationId, Assembly assembly)
  {
    var stopwatch = Stopwatch.StartNew();
    dynamic? result = null;
    try
    {
      var evt = new BpnTaskInitialized(
          CorrelationId: correlationId,
          ContainerId: containerId,
          FeatureId: featureId,
          FeatureRevision: featureVersion,
          TaskId: task.Id,
          Input: InputLogger.LogInput(input));
      EngineEventsQueue.EnqueueEngineEvents(evt);

      var inputValidation = (ValueTuple<bool, List<string>>)task.VerifyInputData(input, assembly);
      var (isOk, missingFields) = inputValidation;

      if (isOk == false)
      {
        EngineEventsQueue.EnqueueEngineEvents(new BpnTaskFailed(correlationId, containerId, featureId, featureVersion, task.Id, new ErrorEvent($"Missing input fields for '{task.Name}' ({task.Id}): ", string.Join(",", missingFields)), stopwatch.Elapsed.TotalMilliseconds));
        throw new ArgumentException($"Missing input fields: {string.Join(",", missingFields)}");
      }

      switch (task)
      {
        case CodeTask codeBlock:
          result = await codeBlock.Execute(input, this, assembly);
          break;
        case ApiInputTask apiInputBlock://TODO: USER CONTEXT
          result = await apiInputBlock.Execute(input, new UserContext("userId", "userName", ["Anonymous"], "ipaddress", true, "auth type", null), assembly);
          break;
        default:
          EngineEventsQueue.EnqueueEngineEvents(new BpnTaskFailed(correlationId, containerId, featureId, featureVersion, task.Id, new ErrorEvent($"Execution for tasktype '{task.GetTypeName()}'is not implemented", string.Empty), stopwatch.Elapsed.TotalMilliseconds));
          return new TaskResult(false, result);
      }

      EngineEventsQueue.EnqueueEngineEvents(new BpnTaskSucceeded(correlationId, containerId, featureId, featureVersion, task.Id, stopwatch.Elapsed.TotalMilliseconds));
    }
    catch (Exception ex)
    {
      if (ex.InnerException != null)
      {
        EngineEventsQueue.EnqueueEngineEvents(new BpnTaskFailed(correlationId, containerId, featureId, featureVersion, task.Id, new ErrorEvent(ex.InnerException.Message, ex.InnerException.StackTrace ?? ""), stopwatch.Elapsed.TotalMilliseconds));

        if (ex.InnerException is UnauthorizedAccessException)
        {
          throw ex.InnerException;
        }
      }
      else
      {
        EngineEventsQueue.EnqueueEngineEvents(new BpnTaskFailed(correlationId, containerId, featureId, featureVersion, task.Id, new ErrorEvent(ex.Message, ex.StackTrace ?? ""), stopwatch.Elapsed.TotalMilliseconds));
        throw;
      }
      return new TaskResult(false, result);
    }
    return new TaskResult(true, result);
  }
}
public record TaskResult(bool Success, dynamic? Result);

public class NoService : ServiceInjection
{
  public override string Name => "No service";

  public override string Description => "No service injected";
  public override string[] ApiDescription => [];
  public override CodeSnippet[] ApiSnippets => [];
  public override void Setup(string namedConfiguration) { }

  public override async Task<TaskResult> Execute(IDocumentSession session, CancellationToken ct, dynamic inputJson, Guid containerId, Guid featureId, long featureVersion, BpnTask task, Guid correlationId, Assembly assembly)
  {
    return await RunAndLog(session, ct, inputJson, containerId, featureId, featureVersion, task, correlationId, assembly);
  }
}

public class PostgreSqlService : ServiceInjection, IAsyncDisposable
{
  public override string Name => "PostgreSql";
  public override string Description => "Provides functionality to insert, update, delete (via ExecuteNonQueryAsync) and select (via ExecuteQueryAsync) from a PostgreSql server. The service opens a connection and starts a transaction for you. It also ensures that the transaction is committed/rolledback and that the connection is closed when done.";
  public override string[] ApiDescription => ["async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters)", "async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters)"];
  public override CodeSnippet[] ApiSnippets => [new CodeSnippet("Mutate database", "_ = await ExecuteNonQueryAsync(sql, parameters);"), new CodeSnippet("Query database", "List<Dictionary<string, object>> result = await ExecuteQueryAsync(sql, parameters);")];

  private NpgsqlConnection? _connection;
  private NpgsqlTransaction? _transaction;
  
  public override void Setup(string namedConfiguration)
  {
    _connection = new NpgsqlConnection("YourConnectionStringHere");//TODO:: get from named services, service....
  }

  public async ValueTask DisposeAsync()
  {
    if (_transaction != null)
    {
      await _transaction.DisposeAsync();
    }

    if (_connection != null)
    {
      await _connection.DisposeAsync();
    }
  }

  public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters)
  {
    await using var command = new NpgsqlCommand(sql, _connection, _transaction);
    foreach (var param in parameters)
    {
      command.Parameters.AddWithValue(param.Key, param.Value);
    }
    return await command.ExecuteNonQueryAsync();
  }

  public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters)
  {
    await using var command = new NpgsqlCommand(sql, _connection, _transaction);
    foreach (var param in parameters)
    {
      command.Parameters.AddWithValue(param.Key, param.Value);
    }
    await using var reader = await command.ExecuteReaderAsync();
    var result = new List<Dictionary<string, object>>();
    while (await reader.ReadAsync())
    {
      var row = new Dictionary<string, object>();

      for (int i = 0; i < reader.FieldCount; i++)
      {
        row[reader.GetName(i)] = reader.GetValue(i);
      }

      result.Add(row);
    }
    return result;
  }

  public override async Task<TaskResult> Execute(IDocumentSession session, CancellationToken ct, dynamic inputJson, Guid containerId, Guid featureId, long featureVersion, BpnTask task, Guid correlationId, Assembly assembly)
  {
    if (_connection == null)
    {
      throw new Exception("Call setup before using the service");
    }

    await _connection.OpenAsync();
    _transaction = await _connection.BeginTransactionAsync();

    var res = await RunAndLog(session, ct, inputJson, containerId, featureId, featureVersion, task, correlationId, assembly);
    if (res.success)
    {
      await _transaction.CommitAsync(); // Commit transaction on success
    }
    else
    {
      await _transaction.RollbackAsync(); // Rollback transaction on failure
    }

    return res;
  }
}
