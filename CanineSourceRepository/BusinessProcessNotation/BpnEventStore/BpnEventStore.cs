using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using Marten.Events.Projections;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureProjection;
using CanineSourceRepository.BusinessProcessNotation.BpnContext;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureStatsProjection;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore;

public static class BpnEventStore
{

  public static async Task GenerateDefaultData(this IDocumentSession session, CancellationToken ct)
  {
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
    //Add the user to the user database
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
    var contextId = await CreateContextFeature.Execute(session, causationId, "User (Demo)", ct);
    var featureId = await AddDraftFeatureFeature.Execute(
                          session,
                          causationId: causationId,
                          bpnContextId: contextId,
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
    options.Projections.LiveStreamAggregation<BpnContextAggregate>();
    options.Projections.Add<BpnContextProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnContextProjection.BpnContext>();

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

  public static string[] ApiVersions { get { return ["v1"]; } }

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
    BpnDraftFeatureAggregate.RegisterBpnEventStore(app);
    BpnContextAggregate.RegisterBpnEventStore(app);
    BpnFeatureAggregate.RegisterBpnEventStore(app);

    BpnDraftFeatureProjection.RegisterBpnEventStore(app);
    BpnFeatureProjection.RegisterBpnEventStore(app);
    BpnFeatureStatsProjection.RegisterBpnEventStore(app);
    BpnContextProjection.RegisterBpnEventStore(app);
  }
}


public static class LifetimeService
{
  public const string WaitingForPort = "Waiting for port to be free";
  //TODO: Release API... so we dont restart every time a feature is released!
  //TODO: What about updating the envoriments (if prod is selected, or is that just a header? - issue if not on same server)
  private static bool StartNewInstance()
  {
    var newProcessStartInfo = new ProcessStartInfo
    {
      FileName = "dotnet",
      Arguments = Assembly.GetEntryAssembly()!.Location,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true
    };
    try
    {
      var newProcess = Process.Start(newProcessStartInfo);
      if (newProcess == null) return false;
      using (StreamReader reader = newProcess.StandardOutput)
      {
        string? outputLine;
        while ((outputLine = reader.ReadLine()) != null)
        {
          Console.WriteLine($"New process output: {outputLine}"); // Log output for debugging
          if (outputLine.Contains(LifetimeService.WaitingForPort))
          {
            return true;
          }
        }
      }
      return false;
    }
    catch
    {
      return false;
    }
  }
  private static void ShutdownThisInstance(IHostApplicationLifetime lifetime)
  {
    lifetime.StopApplication();//Gracefull shutdown - IF CancellationToken have ben properly implemented throughout the system
  }

  public static void Restart(this IHostApplicationLifetime lifetime)
  {
    //do some domain logic, that updates the database, which is used when the process is started to dynamically enable endpoints and document via openapi

    if (StartNewInstance())
    {
      ShutdownThisInstance(lifetime);
    }
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

  
}


public static class TypeExtensions
{
  // This extension method checks if a type is a record type
  public static bool IsRecord(this Type type)
  {
    return type.GetMethod("<Clone>$") != null;
  }
}