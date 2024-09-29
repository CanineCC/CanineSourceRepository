using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using Marten.Events.Projections;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnFeatureDiagram;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnFeatureProjection;

namespace CanineSourceRepository.BusinessProcessNotation.Context;

public static class BpnEventStore
{
  public static async Task GenerateDefaultData(this IDocumentSession session, CancellationToken ct)
  {
    var data = session.Query<BpnContextProjection.BpnContext>().ToList();
    if (session.Query<BpnContextProjection.BpnContext>().Any()) return;

    var entryBlock = new ApiInputTask("Create user endpoint", ["Anonymous"]);
    entryBlock = (entryBlock.AddRecordType(new BpnTask.RecordDefinition("Api",
      new BpnTask.DataDefinition("Name", "string")
      )) as ApiInputTask)!;
    entryBlock = entryBlock with
    {
      Input = "Api",
    };

    var createUserBlock = new CodeTask("Create user logic");
    createUserBlock = (createUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Output",
      new BpnTask.DataDefinition("Id", "Guid"),
      new BpnTask.DataDefinition("Name", "string")
      )) as CodeTask)!;
    createUserBlock = (createUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Input",
      new BpnTask.DataDefinition("Name", "string")
      )) as CodeTask)!;
    createUserBlock = createUserBlock with
    {
      BusinessPurpose = "Validate that the user has a verified email address before allowing access to premium content.",
      BehavioralGoal = "Ensure the email is verified and allow access to content.",
      Input = "Input",
      Output = "Output",
      Code = @$"
    var userId = Guid.CreateVersion7();
    //TODO: Add the user to the user database
    return new Output(userId, input.Name/*, input.AccessScope*/);
    "
    };

    var logUserBlock = new CodeTask("Log user");
    logUserBlock = (logUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Input",
      new BpnTask.DataDefinition("Id", "Guid"),
      new BpnTask.DataDefinition("Name", "string")
      )) as CodeTask)!;
    logUserBlock = logUserBlock with
    {
      BusinessPurpose = "Validate that the user has a verified email address before allowing access to premium content.",
      BehavioralGoal = "Ensure the email is verified and allow access to content.",
      Input = "Input",
      Code = @$"
    Console.WriteLine(input.Id.ToString() + input.Name);
    "
    };

    var transition = new BpnTransition(
      entryBlock.Id,
      createUserBlock.Id,
      "Call Accepted",
      "input.Name != string.Empty",
      new Map("input.Name", "Name")//,//issue with lists and multiple fields of same type, but with different mappings
                                   // new Map("input.Name ?? \"Anonymous\"", "AccessScope")
      );
    var logTransition = new BpnTransition(
      createUserBlock.Id,
      logUserBlock.Id,
      "Log info",
      "true",
      new Map("output.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new Map("output.Id", "Id")
      );

    var causationId = "GenerateDefaultData";
    var contextId = await session.CreateBpnContext(causationId, "Users (Demo)", ct);
    var featureId = await session.AddDraftFeature(
                          causationId: causationId,
                          bpnContextId: contextId,
                          name: "User",
                          objective: "Enable users to register, validate their email, and gain access to premium content.",
                          flowOverview: "The user enters their registration details, verifies their email, and is granted access to restricted areas.",
                          ct: ct);
    await session.AddTaskToDraftFeature(causationId: causationId, featureId: featureId, task: entryBlock, ct);
    await session.AddTaskToDraftFeature(causationId: causationId, featureId: featureId, task: createUserBlock, ct);
    await session.AddTaskToDraftFeature(causationId: causationId, featureId: featureId, task: logUserBlock, ct);
    await session.AddTransitionToDraftFeature(causationId: causationId, featureId: featureId, transition: transition, ct);
    await session.AddTransitionToDraftFeature(causationId: causationId, featureId: featureId, transition: logTransition, ct);
    await session.ReleaseFeature(causationId: causationId, featureId: featureId, user: "system", ct);
  }
  public static void RegisterBpnEventStore(this StoreOptions options)
  {
    options.Projections.LiveStreamAggregation<BpnContextAggregate>();
    options.Projections.Add<BpnContextProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnContextProjection.BpnContext>();

    options.Projections.LiveStreamAggregation<BpnFeatureAggregate>();
    options.Projections.Add<BpnFeatureProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnFeature>();

    options.Projections.LiveStreamAggregation<BpnDraftFeatureAggregate>();
    options.Projections.Add<BpnDraftFeatureProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnDraftFeatureProjection.BpnDraftFeature>();

    foreach (var eventDefinition in EventRegistrar.RegisterEvents(Assembly.GetExecutingAssembly()))
    {
      options.Events.AddEventType(eventDefinition);
    }
  }
  public static void RegisterBpnEventStore(this WebApplication app)
  {
    Type eventStoreType = typeof(BpnEventStore);
    MethodInfo[] methods = eventStoreType.GetMethods(BindingFlags.Public | BindingFlags.Static);

    foreach (var method in methods)
    {
      if (method.ReturnType.FullName == "System.Threading.Tasks.Task" || method.ReturnType.FullName == "System.Void")
      {
        continue;
      }
      if (method.ReturnType.GenericTypeArguments.Length != 1)
      {
        continue;
      }

      var parameters = method.GetParameters();

      var name = method.Name.ToPascalCase();
      
      //TODO: MapGet for Context + Feature

      //TODO: Input parameter needs to be documented for openapi!!

      // Map route based on method name and parameters
      app.MapPost($"/{name}", async (HttpContext context) =>
      {
        var paramValues = new object[parameters.Length];

        // Populate parameters from request
        foreach (var param in parameters)
        {
          string paramName = param.Name!;
          string? paramValue = context.Request.Query[paramName].ToString();

          // Convert query parameter to correct type
          paramValues[Array.IndexOf(parameters, param)] = Convert.ChangeType(paramValue, param.ParameterType);
        }

        // Invoke the static method with parameters
        var result = method.Invoke(null, paramValues);

        // If the method returns a Task, await it
        if (result is Task task)
        {
          await task.ConfigureAwait(false);

          // Check if Task<T> to get the result
          var taskType = task.GetType();
          if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
          {
            var resultProperty = taskType.GetProperty("Result");
            result = resultProperty?.GetValue(task);
          }
          else
          {
            result = null;
          }
        }

        // Return the result
        return Results.Json(result);
      }).WithName(name)
      .Produces(StatusCodes.Status202Accepted)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status500InternalServerError)
      .Accepts(method.ReturnType.GenericTypeArguments.First(), false, "application/json"); // Define input content type
    }
  }

  private static async Task RegisterEventsOnBpnContext(
    this IDocumentSession session, 
    CancellationToken ct, 
    Guid id, 
    string causationId, 
    params object[] @events)
  {
    session.CorrelationId = id.ToString("N");
    session.CausationId = causationId;

    await session.Events.WriteToAggregate<BpnContextAggregate>(
            id,
            stream => stream.AppendMany(@events),
            ct);
    await session.SaveChangesAsync();
  }
  private static async Task RegisterEventsOnBpnFeature(
    this IDocumentSession session,
    CancellationToken ct,
    Guid id,
    string causationId,
    params object[] @events)
  {
    session.CorrelationId = id.ToString("N");
    session.CausationId = causationId;

    await session.Events.WriteToAggregate<BpnFeatureAggregate>(
            id,
            stream => stream.AppendMany(@events),
            ct);
    await session.SaveChangesAsync();
  }

  private static async Task RegisterEventsOnBpnDraftFeature(
    this IDocumentSession session,
    CancellationToken ct,
    Guid id,
    string causationId,
    params object[] @events)
  {
    session.CorrelationId = id.ToString("N");
    session.CausationId = causationId;

    await session.Events.WriteToAggregate<BpnDraftFeatureAggregate>(
            id,
            stream => stream.AppendMany(@events),
            ct);
    await session.SaveChangesAsync();
  }
  

  public static async Task<Guid> CreateBpnContext(this IDocumentSession session, string causationId, string name, CancellationToken ct)
  {
    var newId = Guid.CreateVersion7();
    await session.RegisterEventsOnBpnContext(ct, newId, causationId, new BpnContextProjection.BpnContext.ContextCreated(Id: newId, Name: name));
    return newId;
  }
  public static async Task<Guid> AddDraftFeature(this IDocumentSession session, string causationId, Guid bpnContextId, string name, string objective, string flowOverview, CancellationToken ct)
  {
    var featureId = Guid.CreateVersion7();
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureCreated(
      FeatureId: featureId, 
      Name: name, 
      Objective : objective, 
      FlowOverview: flowOverview));
    await session.RegisterEventsOnBpnContext(ct, bpnContextId, causationId, new BpnContextProjection.BpnContext.FeatureAddedToContext(FeatureId: featureId));

    return featureId;
  }
  public static async Task<ValidationResponse> ResetDraftFeature(this IDocumentSession session, string causationId, Guid featureId, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    var feature = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);
    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureReset(
      Tasks: feature?.Tasks ?? [],
      Transitions: feature?.Transitions ?? [],
      Diagram: feature?.Diagram ?? new BpnFeatureDiagram()
    ));

    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
  public static async Task<ValidationResponse> UpdateDraftFeaturePurpose(this IDocumentSession session, string causationId, Guid featureId, string name, string objective, string flowOverview, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeaturePurposeChanged(Name: name, Objective: objective, FlowOverview: flowOverview));

    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
  public static async Task<ValidationResponse> ReleaseFeature(this IDocumentSession session, string causationId, Guid featureId, string user, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    var result = aggregate.IsValid();
    if (result.IsValid == false) return result;

    var feature = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);

    Dictionary<Guid, Guid> newIds = aggregate.Tasks.ToDictionary(task => task.Id, task => Guid.CreateVersion7());

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new BpnFeatureProjection.BpnFeature.FeatureReleased(
      ReleasedBy: user, 
      Name: aggregate.Name, 
      Objective: aggregate.Objective, 
      FlowOverview: aggregate.FlowOverview, 
      Tasks: aggregate.Tasks.Select(task=> task with { Id = newIds[task.Id] }).ToImmutableList(), 
      Transitions: aggregate.Transitions.Select(transition => transition with { FromBPN = newIds[transition.FromBPN], ToBPN = newIds[transition.ToBPN] }).ToImmutableList(),
      Diagram: aggregate.Diagram,
      feature?.Revision+1 ?? 1));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
  public static async Task<ValidationResponse> AddTaskToDraftFeature(this IDocumentSession session, string causationId, Guid featureId, BpnTask task, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null ) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTaskAdded(Task: task));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }

  public static async Task<ValidationResponse> RemoveTaskFromDraftFeature(this IDocumentSession session, string causationId, Guid featureId, BpnTask task, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTaskRemoved(Task: task));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
  public static async Task<ValidationResponse> AddTransitionToDraftFeature(this IDocumentSession session, string causationId, Guid featureId, BpnTransition transition, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTransitionAdded(Transition: transition));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
  public static async Task<ValidationResponse> RemoveTransitionFromDraftFeature(this IDocumentSession session, string causationId, Guid featureId, BpnTransition transition, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTransitionRemoved(Transition: transition));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }

  public static async Task<ValidationResponse> UpdateEnvironmentsOnFeature(this IDocumentSession session, string causationId, Guid featureId, long featureVersion, Feature.Environment[] environment, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new BpnFeatureProjection.BpnFeature.EnvironmentsUpdated(FeatureVersion: featureVersion, Environment: environment));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }

  public static async Task<ValidationResponse> PositionUpdatedOnDraftFeature(this IDocumentSession session, string causationId, Guid featureId, Guid taskId, Position position, Feature.Environment[] environment, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new BpnFeatureDiagram.DraftFeatureDiagramPositionUpdated(new BpnFeatureDiagram.BpnPosition(taskId, position)));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
  public static async Task<ValidationResponse> WaypointUpdatedOnDraftFeature(this IDocumentSession session, string causationId, Guid featureId, Guid fromTaskId, Guid toTaskId, Position[] positions, Feature.Environment[] environment, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return new ValidationResponse(false, $"Draft feature '{featureId}' was not found", ResultCode.NotFound);

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new BpnFeatureDiagram.DraftFeatureDiagramWaypointUpdated(new ConnectionWaypoints(fromTaskId, toTaskId, positions)));
    return new ValidationResponse(true, string.Empty, ResultCode.NoContent);
  }
}


public static class EventRegistrar
{
  public static List<Type> RegisterEvents(Assembly assembly)
  {
    var eventDefinitions = new List<Type>();
    // Get all types in the namespace that implement SingleStreamProjection<>
    var projectionTypes = assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract)
        .Where(t => t.BaseType != null && t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(SingleStreamProjection<>))
        .ToList();

    // Iterate over each projection class found
    foreach (var projectionType in projectionTypes)
    {
      var nestedTypes = projectionType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
      foreach (var nestedType in nestedTypes)
      {
        // Find all records within this nested class
        var records = nestedType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => t.IsClass && t.IsRecord())
            .ToList();

        eventDefinitions.AddRange(records);
      }
    }
    return eventDefinitions;
  }
}

public static class TypeExtensions
{
  // This extension method checks if a type is a record type
  public static bool IsRecord(this Type type)
  {
    return type.GetMethod("<Clone>$") != null;
  }
}