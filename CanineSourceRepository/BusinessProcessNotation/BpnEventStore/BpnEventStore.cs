using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSolution;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnSystem;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;
using Marten.Events.Projections;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore;

public static class BpnEventStore
{

  public static async Task GenerateDefaultData(this IDocumentSession session, CancellationToken ct)
  {
    if (session.Query<BpnBpnWebApiContainerProjection.BpnWebApiContainer>().Any()) return;

    var entryBlock = new ApiInputTask("Create user endpoint", ["Anonymous"]);
    entryBlock = (entryBlock.AddRecordType(new BpnTask.RecordDefinition("Api",
      new BpnTask.DataDefinition("Name", "string")
      )) as ApiInputTask)!;
    entryBlock.Input = "Api";

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
    createUserBlock.BusinessPurpose = "Validate that the user has a verified email address before allowing access to premium content.";
    createUserBlock.BehavioralGoal = "Ensure the email is verified and allow access to content.";
    createUserBlock.Input = "Input";
    createUserBlock.Output = "Output";
    createUserBlock.Code = @$"
    var userId = Guid.CreateVersion7();
    //Add the user to the user database
    return new Output(userId, input.Name/*, input.AccessScope*/);
    ";

    var logUserBlock = new CodeTask("Log user");
    logUserBlock = (logUserBlock.AddRecordType(
      new BpnTask.RecordDefinition("Input",
      new BpnTask.DataDefinition("Id", "Guid"),
      new BpnTask.DataDefinition("Name", "string")
      )) as CodeTask)!;
    logUserBlock.BusinessPurpose = "Validate that the user has a verified email address before allowing access to premium content.";
    logUserBlock.BehavioralGoal = "Ensure the email is verified and allow access to content.";
    logUserBlock.Input = "Input";
    logUserBlock.Code = @$"
    Console.WriteLine(input.Id.ToString() + input.Name);
    ";

    var transition = new BpnTransition(
      entryBlock.Id,
      createUserBlock.Id,
      "Call Accepted",
      "input.Name != string.Empty",
      new MapField("input.Name", "Name")//,//issue with lists and multiple fields of same type, but with different mappings
                                   // new Map("input.Name ?? \"Anonymous\"", "AccessScope")
      );
    var logTransition = new BpnTransition(
      createUserBlock.Id,
      logUserBlock.Id,
      "Log info",
      "true",
      new MapField("output.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new MapField("output.Id", "Id")
      );

    var causationId = "GenerateDefaultData";

    var solutionId =
      await CreateSolutionFeature.Execute(session, causationId, "Hello world", "Simple scarfold solution", ct);
    var systemId = await CreateSystemFeature.Execute(session, causationId,  solutionId, "User system", "System for storing and verifying users", ct);
    var containerId = await CreateContainerFeature.Execute(session, causationId, "User (Demo)", "User api",systemId, ct);
    var featureId = await AddDraftFeatureFeature.Execute(
                          session,
                          causationId: causationId,
                          bpnContextId: containerId,
                          name: "Create user",
                          objective: "Enable users to register, validate their email, and gain access to premium content.",
                          flowOverview: "The user enters their registration details, verifies their email, and is granted access to restricted areas.",
                          ct: ct);
    await AddTaskToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, task: entryBlock, ct);
    await AddTaskToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, task: createUserBlock, ct);
    await AddTaskToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, task: logUserBlock, ct);
    await AddTransitionToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, transition: transition, ct);
    await AddTransitionToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, transition: logTransition, ct);
    //await ReleaseFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, user: "system", ct);
  }
  public static void RegisterBpnEventStore(this StoreOptions options)
  {
    options.Projections.LiveStreamAggregation<BpnSolutionAggregate>();
    options.Projections.Add<BpnSolutionProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnSolutionProjection.BpnSolution>();

    options.Projections.LiveStreamAggregation<BpnSystemAggregate>();
    options.Projections.Add<BpnSystemProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnSystemProjection.BpnSystem>();

    options.Projections.LiveStreamAggregation<BpnBpnWebApiContainerAggregate>();
    options.Projections.Add<BpnBpnWebApiContainerProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnBpnWebApiContainerProjection.BpnWebApiContainer>();
  
    options.Projections.LiveStreamAggregation<BpnFeatureAggregate>();
    options.Projections.Add<BpnFeatureProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnFeature>();
    options.Projections.Add<BpnFeatureStatsProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnFeatureStatsProjection.BpnFeatureStat>();



    options.Projections.LiveStreamAggregation<BpnDraftFeatureAggregate>();
    options.Projections.Add<BpnDraftFeatureProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnDraftFeatureProjection.BpnDraftFeature>();


    var interfaceType = typeof(IFeature);
    var implementations = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        .ToList();

    foreach (var implementation in implementations)
    {
      var registerEventsMethod = implementation.GetMethod("RegisterBpnEvents", BindingFlags.Public | BindingFlags.Static);
      registerEventsMethod?.Invoke(null, [options]);
    }
  }

  public static string[] ApiRevisions { get { return ["v1"]; } }

  public static void RegisterBpnEventStore(this WebApplication app)
  {
    var interfaceType = typeof(IFeature);
    var implementations = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        .ToList();

    foreach (var implementation in implementations)
    {
      var registerMethod = implementation.GetMethod("RegisterBpnEventStore", BindingFlags.Public | BindingFlags.Static);
      registerMethod?.Invoke(null, [app]);
    }
    BpnSolutionAggregate.RegisterBpnEventStore(app);
    BpnDraftFeatureAggregate.RegisterBpnEventStore(app);
    BpnBpnWebApiContainerAggregate.RegisterBpnEventStore(app);
    BpnFeatureAggregate.RegisterBpnEventStore(app);
    BpnSystemProjection.RegisterBpnEventStore(app);

    BpnSolutionProjection.RegisterBpnEventStore(app);
    BpnDraftFeatureProjection.RegisterBpnEventStore(app);
    BpnFeatureProjection.RegisterBpnEventStore(app);
    BpnFeatureStatsProjection.RegisterBpnEventStore(app);
    BpnBpnWebApiContainerProjection.RegisterBpnEventStore(app);
  }
}

public static class DocumentSessionExtension
{

  public static async Task RegisterEventsOnBpnContext(
    this IDocumentSession session,
    CancellationToken ct,
    Guid id,
    string causationId,
    params object[] @events)
  {
    session.CorrelationId = id.ToString("N");
    session.CausationId = causationId;

    await session.Events.WriteToAggregate<BpnBpnWebApiContainerAggregate>(
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

  
}

/*
public static class TypeExtensions
{
  // This extension method checks if a type is a record type
  public static bool IsRecord(this Type type)
  {
    return type.GetMethod("<Clone>$") != null;
  }
}*/