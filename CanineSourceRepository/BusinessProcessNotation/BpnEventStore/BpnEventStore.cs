using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.NamedConfigurationFeatures;
using CanineSourceRepository.BusinessProcessNotation.Engine;
using Marten.Events.Projections;
using BpnTask = CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.BpnTask;
using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore;

public static class BpnEventStore
{

  public static async Task GenerateDefaultData(this IDocumentSession session, CancellationToken ct)
  {
    if (session.Query<BpnBpnWebApiContainerProjection.BpnWebApiContainer>().Any()) return;
    
    

    var causationId = "GenerateDefaultData";
    var solutionId =
      await CreateSolutionFeature.Execute(session, causationId, "Demo solution", "Simple scarfold solution", ct);
    var systemId = await CreateSystemFeature.Execute(session, causationId,  solutionId, "User system", "System for storing and verifying users", ct);

    var namedConfigurationId = await AddNamedConfigurationFeature.Execute(
      session, 
      causationId, 
      systemId,
      ServiceType.ServiceTypes.First(p => p.InjectedComponent.GetType() == typeof(PostgreSqlService)).Id, 
      "Customer Database", 
      "Internal customer db",
      Scope.Internal, 
      new Dictionary<string, string>()
      {
        { "Host", "localhost" },
        { "Port", "5432" },
        { "Database", "customerdb" },
        { "Username", "admin" },
        { "Password", "todo-encrypted-todo" }
      }, ct);
    var containerId = await CreateContainerFeature.Execute(session, causationId, "User (Demo)", "User api",systemId, ct);
    var featureId = await AddDraftFeatureFeature.Execute(
      session,
      causationId: causationId,
      containerId: containerId,
      name: "Create user",
      objective: "Enable users to register, validate their email, and gain access to premium content.",
      flowOverview: "The user enters their registration details, verifies their email, and is granted access to restricted areas.",
      ct: ct);
    
    
    
    var createUserBlock = new AddTaskToDraftFeatureFeature.Task()
    {
      BusinessPurpose =
        "Validate that the user has a verified email address before allowing access to premium content.",
      BehavioralGoal =  "Ensure the email is verified and allow access to content.",
      Input = "Input",
      Output = "Output",
      Code = @$"
    var userId = Guid.CreateVersion7();
    //Add the user to the user database
    return new Output(userId, input.Name/*, input.AccessScope*/);
    ",
      NamedConfigurationId = null,
      Name = "Create user logic",
      RecordTypes = [
        new RecordDefinition("Input",  new DataDefinition("Name", "string")),
        new RecordDefinition("Output", new DataDefinition("Id", "Guid"), new DataDefinition("Name", "string"))
      ]
    };      
    var logUserBlock = new AddTaskToDraftFeatureFeature.Task()
    {
      BusinessPurpose =
        "Store the user in the database for later lookup.",
      BehavioralGoal = "Insert the user into the customer database",
      Input = "Input",
      Code = $"Console.WriteLine(input.Id.ToString() + input.Name);",
      NamedConfigurationId = namedConfigurationId,
      Name = "Store user in db",
      RecordTypes = [
        new RecordDefinition("Input", new DataDefinition("Id", "Guid"), new DataDefinition("Name", "string"))
      ]
    };

    var createUserId =await AddTaskToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, task: createUserBlock, ct);
    var logUserId = await AddTaskToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, task: logUserBlock, ct);

    var logTransition = new BpnTransition(
      createUserId,
      logUserId,
      "Log info",
      "true",
      new MapField("output.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
      new MapField("output.Id", "Id")
      );

    

    
    

    var personaId = await AddPersonaFeature.Execute(session, causationId, "System Administrator",  "An administrator in the system",Scope.Internal, ct);
    await PersonaConsumeComponentFeature.Execute(session, causationId, personaId, featureId, "Calls api to create a user", ct);

    await AddTransitionToDraftFeatureFeature.Execute(session, causationId: causationId, featureId: featureId, transition: logTransition, ct);
  }
  public static void RegisterBpnEventStore(this StoreOptions options)
  {
    options.Projections.LiveStreamAggregation<SolutionAggregate>();
    options.Projections.Add<SolutionProjection>(ProjectionLifecycle.Async);
    options.Schema.For<SolutionProjection.BpnSolution>();

    options.Projections.LiveStreamAggregation<SystemAggregate>();
    options.Projections.Add<SystemProjection>(ProjectionLifecycle.Async);
    options.Schema.For<SystemProjection.BpnSystem>();

    options.Projections.LiveStreamAggregation<WebApiContainerAggregate>();
    options.Projections.Add<BpnBpnWebApiContainerProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnBpnWebApiContainerProjection.BpnWebApiContainer>();
  
    options.Projections.LiveStreamAggregation<FeatureComponentAggregate>();
    options.Projections.Add<FeatureComponentProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnFeature>();
    options.Projections.Add<BpnFeatureStatsProjection>(ProjectionLifecycle.Async);
    options.Schema.For<BpnFeatureStatsProjection.BpnFeatureStat>();

    options.Projections.LiveStreamAggregation<DraftFeatureComponentAggregate>();
    options.Projections.Add<DraftFeatureComponentProjection>(ProjectionLifecycle.Async);
    options.Schema.For<DraftFeatureComponentProjection.BpnDraftFeature>();

    options.Projections.LiveStreamAggregation<PersonaAggregate>();
    options.Projections.Add<PersonaProjection>(ProjectionLifecycle.Async);
    options.Schema.For<PersonaProjection.Persona>();
    
    options.Projections.LiveStreamAggregation<NamedConfigurationAggregate>();
    options.Projections.Add<NamedConfigurationProjection>(ProjectionLifecycle.Async);
    options.Schema.For<NamedConfigurationProjection.NamedConfiguration>();
    

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
    DraftFeatureComponentAggregate.RegisterBpnEventStore(app);

    SystemProjection.RegisterBpnEventStore(app);
    SolutionProjection.RegisterBpnEventStore(app);
    DraftFeatureComponentProjection.RegisterBpnEventStore(app);
    FeatureComponentProjection.RegisterBpnEventStore(app);
    BpnFeatureStatsProjection.RegisterBpnEventStore(app);
    BpnBpnWebApiContainerProjection.RegisterBpnEventStore(app);
    PersonaProjection.RegisterBpnEventStore(app);
    NamedConfigurationProjection.RegisterBpnEventStore(app);
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

    await session.Events.WriteToAggregate<WebApiContainerAggregate>(
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

    await session.Events.WriteToAggregate<FeatureComponentAggregate>(
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

    await session.Events.WriteToAggregate<DraftFeatureComponentAggregate>(
            id,
            stream => stream.AppendMany(@events),
            ct);
    await session.SaveChangesAsync();
  }
  public static async Task RegisterEventsOnPersona(
    this IDocumentSession session,
    CancellationToken ct,
    Guid id,
    string causationId,
    params object[] @events)
  {
    session.CorrelationId = id.ToString("N");
    session.CausationId = causationId;

    await session.Events.WriteToAggregate<PersonaAggregate>(
      id,
      stream => stream.AppendMany(@events),
      ct);
    await session.SaveChangesAsync();
  }

  public static async Task RegisterEventsOnNamedConfiguration(
    this IDocumentSession session,
    CancellationToken ct,
    Guid id,
    string causationId,
    params object[] @events)
  {
    session.CorrelationId = id.ToString("N");
    session.CausationId = causationId;

    await session.Events.WriteToAggregate<NamedConfigurationAggregate>(
      id,
      stream => stream.AppendMany(@events),
      ct);
    await session.SaveChangesAsync();
  }
  
}
