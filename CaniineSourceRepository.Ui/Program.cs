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
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCaching();

var connectionString = new NpgsqlConnectionStringBuilder
{
  Host = "127.0.0.1",
  Port = 6432,
  Database = "CSR",
  Username = "citizix_user",
  Password = "S3cret"
};
builder.Services.AddNpgsqlDataSource(connectionString.ConnectionString);
builder.Services.AddMarten(serviceProvider =>
{
  var options = new StoreOptions();
  options.RegisterBpnEngine();
  options.RegisterBpnEventStore();
  options.Policies.ForAllDocuments(x =>
  {
    x.Metadata.CausationId.Enabled = true;
    x.Metadata.CorrelationId.Enabled = true;
    x.Metadata.Headers.Enabled = true;
  });
  options.Events.MetadataConfig.EnableAll();

  if (builder.Environment.IsDevelopment()) options.AutoCreateSchemaObjects = AutoCreate.All;
  return options;
})
    .UseNpgsqlDataSource()
    .ApplyAllDatabaseChangesOnStartup()
    .AddAsyncDaemon(DaemonMode.HotCold)
    .UseLightweightSessions();

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
builder.Services.AddSingleton<JsonSerializerOptions>(new JsonSerializerOptions
{
  Converters = { new JsonStringEnumConverter() }
});

builder.Services.AddHsts(options =>
{
  options.Preload = true;
  options.IncludeSubDomains = true;
  options.MaxAge = TimeSpan.FromDays(360);
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
  app.UseHsts();
}


app.UseResponseCaching();
app.UseHttpsRedirection();
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





while (!IsPortAvailable(7053 /*port*/))//maybe not hardcode
{
  Thread.Sleep(10);
  Console.WriteLine($"Waiting for the port...");
}

Console.WriteLine("Server starting...");//used as signal to existing process that this process is ready to take over (in case of a graceful restart)
app.Run();  // Start the server


bool IsPortAvailable(int port)
{
  // Check if the port is available
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