using CanineSourceRepository.BusinessProcessNotation.Engine;
using Marten.Events.Daemon.Resiliency;
using Marten;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
using Weasel.Core;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore;
using NSwag.Generation.Processors;
using System.Text.Json;
using System.Net.Sockets;
using System.Net;
using Marten.Events;
using EngineEvents;
using Microsoft.AspNetCore.SignalR;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureProjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCaching();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAllOrigins",
      builder =>
      {
        builder//.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(origin => true);
      });
});

var connectionString = new NpgsqlConnectionStringBuilder
{
  Host = builder.Configuration.GetSection("Marten:Host").Value, 
  Port = int.Parse(builder.Configuration.GetSection("Marten:Port").Value!), 
  Database = builder.Configuration.GetSection("Marten:Database").Value,  
  Username = builder.Configuration.GetSection("Marten:Username").Value,  
  Password = builder.Configuration.GetSection("Marten:Password").Value,  
  MaxPoolSize = int.Parse(builder.Configuration.GetSection("Marten:MaxPoolSize").Value!),  
  Pooling = bool.Parse(builder.Configuration.GetSection("Marten:Pooling").Value!) 
};

builder.Services.AddSingleton<BpnSignalRService>(sp =>
    new BpnSignalRService(sp.GetRequiredService<IHubContext<BpnHub>>(), builder.Configuration));
builder.Services.AddHostedService<BpnSignalRService>();
builder.Services.AddHostedService<EngineEventsBackgroundService>();
builder.Services.AddMarten(config =>
{
  config.Connection(connectionString.ConnectionString);
  config.RegisterBpnEngine();
  config.RegisterBpnEventStore();
  config.UseSystemTextJsonForSerialization(BpnEngine.bpnJsonSerialize, enumStorage: EnumStorage.AsString);

  config.Policies.ForAllDocuments(x =>
  {
    x.Metadata.CausationId.Enabled = true;
    x.Metadata.CorrelationId.Enabled = true;
    x.Metadata.Headers.Enabled = true;
  });
  config.Events.StreamIdentity = StreamIdentity.AsGuid;
  config.AutoCreateSchemaObjects = AutoCreate.All;
})
  .ApplyAllDatabaseChangesOnStartup()
  .UseLightweightSessions()
  .AddAsyncDaemon(DaemonMode.HotCold);

foreach (var version in BpnEventStore.ApiVersions)
{
  builder.Services.AddOpenApiDocument(config =>
  {
    config.DocumentName = "engine_" + version;
    config.Title = "BpnEngine API " + version.ToUpper();
    config.Version = version;
    config.OperationProcessors.Add(new OperationProcessor(ctx =>
    {
      // Only include operations with the "BpnEngine" tag
      return ctx.OperationDescription.Path.StartsWith("/BpnEngine") && ctx.OperationDescription.Path.Contains("/" + version + "/"); //ctx.OperationDescription.Operation.Tags.Contains("BpnEngine");
    }));

  });
}
foreach (var version in BpnEngine.PotentialApiVersions)
{
  builder.Services.AddOpenApiDocument(config =>
  {
    config.DocumentName = "bpn_" + version;
    config.Title = "BPN API " + version.ToUpper();
    config.Version = version;

    config.OperationProcessors.Add(new OperationProcessor(ctx =>
    {
      // Only include operations with the "BpnEngine" tag
      return !ctx.OperationDescription.Path.StartsWith("/BpnEngine") && ctx.OperationDescription.Path.Contains("/" + version + "/"); // ctx.OperationDescription.Operation.Tags.Contains("Bpn");
    }));
  });
}

builder.Services.AddScoped<IClientNotificationService, BpnSignalRService>();
builder.Services.AddEndpointsApiExplorer(); // Enables OpenAPI
builder.Services.AddSingleton<JsonSerializerOptions>(BpnEngine.bpnJsonSerialize);
builder.Services.AddHsts(options =>
{
  options.Preload = true;
  options.IncludeSubDomains = true;
  options.MaxAge = TimeSpan.FromDays(360);
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddLogging(loggingBuilder =>
{
  loggingBuilder.AddConsole();
  loggingBuilder.AddDebug();
});

var app = builder.Build();
app.UseRouting();

app.UseCors("AllowAllOrigins");

app.UseEndpoints(endpoints =>
{
  endpoints.MapHub<BpnHub>("/bpnHub");
  // Other endpoint mappings
});

app.UseMiddleware<ThrottlingMiddleware>(TimeSpan.FromMilliseconds(25));
if (!app.Environment.IsDevelopment())
{
  app.UseHsts();
} else
{
 // app.UseDeveloperExceptionPage();
}
app.UseResponseCaching();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
  var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
  await session.GenerateDefaultData(CancellationToken.None);
  app.RegisterAll(session);
}
app.RegisterBpnEventStore();

app.UseOpenApi(options =>
{
  options.DocumentName = "My API Documentation";
});
using (var scope = app.Services.CreateScope())
{
  var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
  app.UseSwaggerUi(settings =>
  {

    foreach (var version in BpnEngine.ApiVersions(session))
    {
      settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute($"Bpn API {version.ToUpper()}", $"/swagger/bpn_{version}/swagger.json"));
    }
    foreach (var version in BpnEventStore.ApiVersions)
    {
      settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute($"BpnEngine API {version.ToUpper()}", $"/swagger/engine_{version}/swagger.json"));
    }
    settings.TagsSorter = "alpha"; // Alphabetically sorts tags
    settings.OperationsSorter = "alpha"; // Alphabetically sorts endpoints
  }); // Adds Swagger UI
}
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var cancellationTokenSource = new CancellationTokenSource(); 
lifetime.ApplicationStopping.Register(() =>
{
  Console.WriteLine("Application is stopping...");
  cancellationTokenSource.Cancel();  // Signal cancellation
});

//app.MapHub<BpnHub>("/bpnHub", (options) => { 
//  options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
//} );

WaitForExistingServerProcessToClose();
app.Run();  // Start the server


void WaitForExistingServerProcessToClose()
{
  while (!IsPortAvailable(7053 /*port*/))//maybe not hardcode
  {
    Thread.Sleep(10);
    SignalExistingServerProcessToClose();
  }
}
void SignalExistingServerProcessToClose()
{
  Console.WriteLine(LifetimeService.WaitingForPort);
}
bool IsPortAvailable(int port)
{
  try
  {
    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
    {
      socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
      return true; // If we can bind to it, the port is available
    }
  }
  catch (SocketException)
  {
    return false; // Port is in use
  }
}



public class BpnHub : Hub
{
  public async Task JoinBpnContextGroup(string bpnContextId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"BpnContext");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have joined BpnContext");
  }
  public async Task LeaveBpnContextGroup(string bpnContextId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"BpnContext");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have left BpnContext");
  }
  public async Task JoinBpnFeatureGroup(string bpnFeatureId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"BpnFeature-{bpnFeatureId}");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have joined BpnFeature group {bpnFeatureId}");
  }
  public async Task LeaveBpnFeatureGroup(string bpnFeatureId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"BpnFeature-{bpnFeatureId}");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have left BpnFeature group {bpnFeatureId}");
  }
}



public class BpnSignalRService : IClientNotificationService, IHostedService
{
  private readonly IHubContext<BpnHub> _hubContext;
  private Timer _timer;
  private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
  private readonly string _connectionString;
  private readonly Dictionary<string, long> _versions = new Dictionary<string, long>();

  public BpnSignalRService(IHubContext<BpnHub> hubContext, IConfiguration configuration)
  {
    _hubContext = hubContext;
    _connectionString = new NpgsqlConnectionStringBuilder
    {
      Host = configuration.GetSection("Marten:Host").Value,
      Port = int.Parse(configuration.GetSection("Marten:Port").Value!),
      Database = configuration.GetSection("Marten:Database").Value,
      Username = configuration.GetSection("Marten:Username").Value,
      Password = configuration.GetSection("Marten:Password").Value,
      MaxPoolSize = int.Parse(configuration.GetSection("Marten:MaxPoolSize").Value!),
      Pooling = bool.Parse(configuration.GetSection("Marten:Pooling").Value!)
    }.ConnectionString;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    // Start the timer when the service starts
    _timer = new Timer(async _ => await CheckForUpdates(cancellationToken), null, TimeSpan.Zero, _pollingInterval);
    return Task.CompletedTask;
  }
  private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

  private async Task CheckForUpdates(CancellationToken cancellationToken)
  {
    await _semaphore.WaitAsync(cancellationToken);
    try
    {
      await SendNewEvents(cancellationToken);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
    }
    finally { _semaphore.Release(); }
  }
  private async Task SendNewEvents(CancellationToken stoppingToken)
  {
    await using var conn = new NpgsqlConnection(_connectionString);
    await conn.OpenAsync(stoppingToken);

    // Command to select all relevant records
    await using var cmd = new NpgsqlCommand("SELECT id, type, version FROM public.mt_streams;", conn);
    await using var reader = await cmd.ExecuteReaderAsync(stoppingToken);

    // We are using a flag to know if this is the first run
    bool firstRun = _versions.Count == 0;

    // Iterate over the result set
    while (await reader.ReadAsync(stoppingToken))
    {
      var id = await reader.GetFieldValueAsync<Guid>(0, stoppingToken);
      var typeName = reader.IsDBNull(1) ? null : await reader.GetFieldValueAsync<string>(1, stoppingToken);
      var version = await reader.GetFieldValueAsync<long>(2, stoppingToken);
      var key = $"{typeName}_{id}";

      if (firstRun)
      {
        // On the first run, populate the _versions dictionary
        _versions.Add(key, version); // Initialize version
      }
      else
      {
        // Check if this entry exists in _versions
        if (_versions.TryGetValue(key, out long existingVersion))
        {
          // Compare versions to see if we have an update
          if (existingVersion < version)
          {
            _versions[key] = version; // Update version in dictionary

            // Notify of updates based on type
            if (typeName == "bpn_context_aggregate")
            {
              await UpdateBpnContext();
            }
            else if (typeName == "bpn_draft_feature_aggregate")
            {
              await UpdateBpnFeature(id);
            }
          }
        }
        else
        {
          // If the entry is not in _versions, add it
          _versions.Add(key, version); // Initialize new entry
        }
      }
    }
  }



  public async Task UpdateBpnContext()
  {
    await _hubContext.Clients.Group($"BpnContext")
        .SendAsync("ReceiveBpnContextUpdate", "The context has been updated.");
  }

  public async Task UpdateBpnFeature(Guid bpnFeatureId)
  {
    await _hubContext.Clients.Group($"BpnFeature-{bpnFeatureId}")
        .SendAsync("ReceiveBpnFeatureUpdate", bpnFeatureId, "The feature has been updated.");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    // Dispose of the timer when the service stops
    _timer?.Change(Timeout.Infinite, 0);
    _timer?.Dispose();
    return Task.CompletedTask;
  }
}