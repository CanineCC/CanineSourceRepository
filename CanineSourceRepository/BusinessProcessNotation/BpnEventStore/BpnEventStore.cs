using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using Marten.Events.Projections;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureProjection;
using CanineSourceRepository.BusinessProcessNotation.BpnContext;
using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task.Snippets;
using static CanineSourceRepository.DynamicCompiler;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore;

public static class BpnEventStore
{
  //TODO: Master db with templates

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
    var contextId = await CreateContextFeature.Execute(session, causationId, "User (Demo)", ct);
    var featureId = await AddDraftFeatureFeature.Execute(
                          session,
                          causationId: causationId,
                          bpnContextId: contextId,
                          name: "User",
                          objective: "Enable users to register, validate their email, and gain access to premium content.",
                          flowOverview: "The user enters their registration details, verifies their email, and is granted access to restricted areas.",
                          ct: ct);
    await AddTaskToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, task: entryBlock, ct);
    await AddTaskToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, task: createUserBlock, ct);
    await AddTaskToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, task: logUserBlock, ct);
    await AddTransitionToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, transition: transition, ct);
    await AddTransitionToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, transition: logTransition, ct);
    await ReleaseFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, user: "system", ct);
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


    var interfaceType = typeof(IFeature);
    var implementations = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        .ToList();

    foreach (var implementation in implementations)
    {
      var registerEventsMethod = implementation.GetMethod("RegisterBpnEvents", BindingFlags.Public | BindingFlags.Static);
      registerEventsMethod?.Invoke(null, new object[] { options });
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
      registerMethod?.Invoke(null, new object[] { app });
    }


    app.MapPost($"BpnEngine/v1/Task/VerifyCodeBlock", (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CodeTask codeTask, CancellationToken ct) =>
    {
      var res = codeTask.VerifyCode();
      if (res.success)
        return Results.Accepted();

      return Results.BadRequest(res.errors);
    }).WithName("VerifyCodeBlock")
      .Produces(StatusCodes.Status202Accepted)
      .Produces(StatusCodes.Status400BadRequest, typeof(CompileError))
      .WithTags("DraftFeature"); 

    app.MapPost($"BpnEngine/v1/Task/GetSnippetsForCodeBlock", (HttpContext context, [FromServices] IDocumentSession session, [FromBody] CodeTask codeTask, CancellationToken ct) =>
    {
      var input = codeTask.RecordTypes.FirstOrDefault(p => p.Name == codeTask.Input);
      var output = codeTask.RecordTypes.FirstOrDefault(p => p.Name == codeTask.Output);

      var snippets = new List<CodeSnippet>();
      if (input != null && output != null)
      {
        snippets.AddRange([
          new CodeSnippet("Auto construct output", AutoConstructorGenerator.GenerateMapping(input, output, codeTask.RecordTypes.ToArray())),
          new CodeSnippet("Auto mapper", AutoMapperGenerator.GenerateMapping(input, output, codeTask.RecordTypes.ToArray()))
          //TODO: snippets from DI ?!
        ]);
      }
    }).WithName("GetSnippetsForCodeBlock")
      .Produces(StatusCodes.Status200OK, typeof(List<CodeSnippet>))
      .WithTags("DraftFeature"); 

    //TODO: Move events to Aggregates (as they are written to aggregates)
    app.MapGet($"BpnEngine/v1/Context/GetAll", async (HttpContext context, [FromServices] IDocumentSession session, CancellationToken ct) =>
    {//TODO: This belongs on the projection!
      var bpnContexts = await session.Query<BpnContextProjection.BpnContext>().ToListAsync(ct);
      return Results.Ok(bpnContexts);
    }).WithName("GetAllContexts")
      .Produces(StatusCodes.Status200OK, typeof(BpnContextProjection.BpnContext))
      .WithTags("Context");
    
    app.MapGet("BpnEngine/v1/Feature/Get/{featureId}/{version}", async (HttpContext context, [FromServices] IDocumentSession session, Guid featureId, long version, CancellationToken ct) =>
    {//TODO: This belongs on the projection!
      var bpnFeature = await session.Query<BpnFeatureProjection.BpnFeature>().Where(p => p.Id == featureId).SingleOrDefaultAsync();
      if (bpnFeature == null) return Results.NotFound();

      var bpnVersion = bpnFeature.Versions.FirstOrDefault(ver => ver.Revision == version);
      if (bpnVersion == null) return Results.NotFound();
      //TODO: include stats on bpnVersion (same as on context)
      return Results.Ok(bpnVersion);
    }).WithName("GetFeatureVersion")
      .Produces(StatusCodes.Status200OK, typeof(BpnFeatureProjection.BpnFeatureVersion))
      .WithTags("Feature");


    app.MapGet("BpnEngine/v1/DraftFeature/Get/{featureId}", async (HttpContext context, [FromServices] IDocumentSession session, Guid featureId, CancellationToken ct) =>
    {//TODO: This belongs on the projection!
      var bpnFeature = await session.Query<BpnDraftFeatureProjection.BpnDraftFeature>().Where(p => p.Id == featureId).SingleOrDefaultAsync();
      if (bpnFeature == null) return Results.NotFound();

      return Results.Ok(bpnFeature);
    }).WithName("GetDraftFeature")
  .Produces(StatusCodes.Status200OK, typeof(BpnDraftFeatureProjection.BpnDraftFeature))
  .WithTags("DraftFeature");

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


/*
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
}*/

public static class TypeExtensions
{
  // This extension method checks if a type is a record type
  public static bool IsRecord(this Type type)
  {
    return type.GetMethod("<Clone>$") != null;
  }
}