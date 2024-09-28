using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using Marten.Events.Projections;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnFeatureProjection;

namespace CanineSourceRepository.BusinessProcessNotation.Context;

public static class BpnEventStore
{
  public static async Task GenerateDefaultData(this IDocumentSession session, CancellationToken ct)
  {
    //var data = session.Query<BpnContextProjection.BpnContext>().ToList();
    //if (session.Query<BpnContextProjection.BpnContext>().Any()) return;

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
      //   new Bpn.DataDefinition("Accessscope", "string")
      )) as CodeTask)!;
    createUserBlock = (createUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Input",
      new BpnTask.DataDefinition("Name", "string")
      //  new Bpn.DataDefinition("Accessscope", "string")
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
    return new Output(userId, input.Name/*, input.Accessscope*/);
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
                                   // new Map("input.Name ?? \"Anonymous\"", "Accessscope")
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
    //await session.UpdateEnvironmentsOnFeature(causationId: causationId, featureId: featureId, environment: [Feature.Environment.Development, Feature.Environment.Testing], ct);


    /*todo: diagram...*/
    //diagram = new BpnFeatureDiagram();
    //diagram.FeatureVersion = feature.Version;
    //diagram.FeatureId = feature.Id;
    ////unneeded? diagram.BpnConnectionWaypoints
    //diagram.BpnPositions.Add(new BpnFeatureDiagram.BpnPosition(entryBlock.Id, new BpnFeatureDiagram.Position(50, 50)));
    //diagram.BpnPositions.Add(new BpnFeatureDiagram.BpnPosition(createUserBlock.Id, new BpnFeatureDiagram.Position(300, 50)));
    //diagram.BpnPositions.Add(new BpnFeatureDiagram.BpnPosition(logUserBlock.Id, new BpnFeatureDiagram.Position(550, 50)));
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

  public static async Task RegisterEventsOnBpnContext(
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
  public static async Task RegisterEventsOnBpnFeature(
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

  public static async Task RegisterEventsOnBpnDraftFeature(
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
  public static async Task<bool> ResetDraftFeature(this IDocumentSession session, string causationId, Guid bpnContextId, Guid featureId, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return false;//does not exists!

    var feature = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);


    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureReset(
      Tasks: feature?.Tasks ?? [],
      Transitions: feature?.Transitions ?? []
    ));
    
    //await session.RegisterEventsOnBpnContext(ct, bpnContextId, causationId, new BpnContextProjection.BpnContext.FeatureRemoved(FeatureId: featureId));

    return true;
  }

  public static async Task<bool> UpdateDraftFeaturePurpose(this IDocumentSession session, string causationId, Guid featureId, string name, string objective, string flowOverview, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return false;//does not exists!

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeaturePurposeChanged(Name: name, Objective: objective, FlowOverview: flowOverview));

    return true;
  }
  
  public static async Task<bool> ReleaseFeature(this IDocumentSession session, string causationId, Guid featureId, string user, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return false;//does not exists!

    var (Valid, Reason) = aggregate.IsValid();
    if (Valid == false) return false; //TODO: Reason!?

    var feature = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);

    Dictionary<Guid, Guid> newIds = aggregate.Tasks.ToDictionary(task => task.Id, task => Guid.CreateVersion7());

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new BpnFeatureProjection.BpnFeature.FeatureReleased(
      ReleasedBy: user, 
      Name: aggregate.Name, 
      Objective: aggregate.Objective, 
      FlowOverview: aggregate.FlowOverview, 
      Tasks: aggregate.Tasks.Select(task=> task with { Id = newIds[task.Id] }).ToImmutableList(), 
      Transitions: aggregate.Transitions.Select(transition => transition with { FromBPN = newIds[transition.FromBPN], ToBPN = newIds[transition.ToBPN] }).ToImmutableList(), 
      feature?.Revision+1 ?? 1));
    return true;
  }
  
  public static async Task<bool> AddTaskToDraftFeature(this IDocumentSession session, string causationId, Guid featureId, BpnTask task, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null ) return false;//does not exists!

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTaskAdded(Task: task));
    return true;
  }

  public static async Task<bool> RemoveTaskFromDraftFeature(this IDocumentSession session, string causationId, Guid featureId, BpnTask task, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return false;//does not exists!

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTaskRemoved(Task: task));
    return true;
  }
  public static async Task<bool> AddTransitionToDraftFeature(this IDocumentSession session, string causationId, Guid featureId, BpnTransition transition, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return false;//does not exists!

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTransitionAdded(Transition: transition));
    return true;
  }
  public static async Task<bool> RemoveTransitionFromDraftFeature(this IDocumentSession session, string causationId, Guid featureId, BpnTransition transition, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnDraftFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return false;//does not exists!

    await session.RegisterEventsOnBpnDraftFeature(ct, featureId, causationId, new BpnDraftFeatureProjection.BpnDraftFeature.DraftFeatureTransitionRemoved(Transition: transition));
    return true;
  }

  public static async Task<bool> UpdateEnvironmentsOnFeature(this IDocumentSession session, string causationId, Guid featureId, long featureVersion, Feature.Environment[] environment, CancellationToken ct)
  {
    var aggregate = await session.Events.AggregateStreamAsync<BpnFeatureAggregate>(featureId, token: ct);
    if (aggregate == null) return false;//does not exists!

    await session.RegisterEventsOnBpnFeature(ct, featureId, causationId, new BpnFeatureProjection.BpnFeature.EnvironmentsUpdated(FeatureVersion: featureVersion, Environment: environment));
    return true;
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