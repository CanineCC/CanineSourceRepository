using CanineSourceRepository.BusinessProcessNotation.Engine;
using Marten.Events.Daemon.Resiliency;
using Marten;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
using Weasel.Core;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore;
using NSwag.Generation.Processors;

var builder = WebApplication.CreateBuilder(args);


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
    .AddAsyncDaemon(DaemonMode.HotCold);


foreach (var version in BpnEventStore.ApiVersions)
{
  builder.Services.AddOpenApiDocument(config =>
  {
    config.DocumentName = "bpnengine_" + version;
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

// Add services to the container.
//builder.Services.AddControllersWithViews()
//.AddJsonOptions(options =>
//{
//  options.JsonSerializerOptions.Converters.Add(new BpnConverter());
//});

builder.Services.AddEndpointsApiExplorer(); // Enables OpenAPI

builder.Services.AddHsts(options =>
{
  options.Preload = true;
  options.IncludeSubDomains = true;
  options.MaxAge = TimeSpan.FromDays(360);
});



builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
//        .AddCookie(options =>
//        {
//          options.LoginPath = "/Account/Login";
//          options.LogoutPath = "/Account/Logout";
//        });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
//  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

//app.UseStaticFiles(new StaticFileOptions
//{
//  OnPrepareResponse = ctx =>
//  {
//    // Cache static files like fonts for 1 year
//    ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
//  }
//});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//app.MapStaticAssets();
//app.UseOutputCache();

app.UseOpenApi();   // Generates OpenAPI document

using (var scope = app.Services.CreateScope())
{
  var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
  await session.GenerateDefaultData(CancellationToken.None);
  app.RegisterAll(session);

  app.UseSwaggerUi(settings =>
  {

    foreach (var version in BpnEngine.ApiVersions(session))
    {
      settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute($"Bpn API {version.ToUpper()}", $"/swagger/bpn_{version}/swagger.json"));
    }
    foreach (var version in BpnEventStore.ApiVersions)
    {
      settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute($"BpnEngine API {version.ToUpper()}", $"/swagger/bpnengine_{version}/swagger.json"));
    }
    settings.TagsSorter = "alpha"; // Alphabetically sorts tags
    settings.OperationsSorter = "alpha"; // Alphabetically sorts endpoints
  }); // Adds Swagger UI
}

app.RegisterBpnEventStore();


app.Run();
