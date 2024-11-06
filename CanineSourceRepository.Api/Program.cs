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
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;
using Marten.Events;
using EngineEvents;
using JasperFx.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NJsonSchema;
using NSwag.Generation.Processors.Contexts;

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

builder.Services.Configure<JsonOptions>(options =>
{
  options.JsonSerializerOptions.Converters.AddRange(BpnEngine.BpnJsonSerialize.Converters);
});
  
builder.Services.AddSingleton<BpnSignalRService>(sp => new BpnSignalRService(sp.GetRequiredService<IHubContext<BpnHub>>(), builder.Configuration));
builder.Services.AddHostedService<BpnSignalRService>();
builder.Services.AddHostedService<EngineEventsBackgroundService>();
builder.Services.AddMarten(config =>
{
  config.Connection(connectionString.ConnectionString);
  config.RegisterBpnEngine();
  config.RegisterBpnEventStore();
  config.UseSystemTextJsonForSerialization(
    BpnEngine.BpnJsonSerialize, 
    enumStorage: EnumStorage.AsString
    );

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

foreach (var revision in BpnEventStore.ApiRevisions)
{
  builder.Services.AddOpenApiDocument(config =>
  {
    config.DefaultResponseReferenceTypeNullHandling = NJsonSchema.Generation.ReferenceTypeNullHandling.NotNull;
    config.DocumentName = "engine_" + revision;
    config.Title = "BpnEngine API " + revision.ToUpper();
    config.Version = revision;
    config.DocumentProcessors.Add(new EnumDocumentProcessor());
    
    config.OperationProcessors.Add(new OperationProcessor(ctx =>
    {
      // Only include operations with the "BpnEngine" tag
      return ctx.OperationDescription.Path.StartsWith("/BpnEngine") && ctx.OperationDescription.Path.Contains("/" + revision + "/"); //ctx.OperationDescription.Operation.Tags.Contains("BpnEngine");
    }));

  });
}
foreach (var revision in BpnEngine.PotentialApiRevisions)
{
  builder.Services.AddOpenApiDocument(config =>
  {
    config.DefaultResponseReferenceTypeNullHandling = NJsonSchema.Generation.ReferenceTypeNullHandling.NotNull;
    config.DocumentName = "bpn_" + revision;
    config.Title = "BPN API " + revision.ToUpper();
    config.Version = revision;
    config.DocumentProcessors.Add(new EnumDocumentProcessor());

    config.OperationProcessors.Add(new OperationProcessor(ctx =>
    {
      // Only include operations with the "BpnEngine" tag
      return !ctx.OperationDescription.Path.StartsWith("/BpnEngine") && ctx.OperationDescription.Path.Contains("/" + revision + "/"); // ctx.OperationDescription.Operation.Tags.Contains("Bpn");
    }));
  });
}

builder.Services.AddEndpointsApiExplorer(); // Enables OpenAPI
builder.Services.AddSingleton<JsonSerializerOptions>(BpnEngine.BpnJsonSerialize);
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

    foreach (var revision in BpnEngine.ApiRevision(session))
    {
      settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute($"Bpn API {revision.ToUpper()}", $"/swagger/bpn_{revision}/swagger.json"));
    }
    foreach (var revision in BpnEventStore.ApiRevisions)
    {
      settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute($"BpnEngine API {revision.ToUpper()}", $"/swagger/engine_{revision}/swagger.json"));
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

public class EnumDocumentProcessor : IDocumentProcessor
{
  public void Process(DocumentProcessorContext context)
  {
    var schemas = context.SchemaResolver.Schemas;
    foreach (var schema in schemas)
    {
      if (schema.IsEnumeration)
      {
        schema.Type = JsonObjectType.String;
        schema.Enumeration.Clear();
        foreach (var name in schema.EnumerationNames)
        {
          schema.Enumeration.Add(name);
        }
      }
    }
  }
}
