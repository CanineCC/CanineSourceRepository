using CanineSourceRepository.BusinessProcessNotation.Engine;
using Marten.Events.Daemon.Resiliency;
using Marten;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
using Weasel.Core;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore;
using NSwag.Generation.Processors;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Sockets;
using System.Net;
using Marten.Events;
using EngineEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCaching();
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAllOrigins",
      builder =>
      {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
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
app.UseMiddleware<ThrottlingMiddleware>(TimeSpan.FromMilliseconds(25));
if (!app.Environment.IsDevelopment())
{
  app.UseHsts();
}
app.UseCors("AllowAllOrigins");
app.UseResponseCaching();
//app.UseHttpsRedirection();
app.UseRouting();
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
