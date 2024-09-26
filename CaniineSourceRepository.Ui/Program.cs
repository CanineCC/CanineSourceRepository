using CanineSourceRepository.BusinessProcessNotation;
using EngineEvents;
using CanineSourceRepository.BusinessProcessNotation.Engine;
using CanineSourceRepository.Ui.Controllers;
using Marten.Events.Daemon.Resiliency;
using Marten;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
using Weasel.Core;
using Marten.Events.Projections;

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
  options.Projections.LiveStreamAggregation<FeatureInvocationAggregate>();
  options.Projections.Add<FeatureInvocationProjection>(ProjectionLifecycle.Async);
  options.Schema.For<FeatureInvocationProjection.FeatureInvocation>().Index(x => x.FeatureId);
  //options.Projections.Errors.SkipApplyErrors = true;
  //options.Projections.Errors.SkipSerializationErrors = true;
  //options.Projections.Errors.SkipUnknownEvents = true;
  options.Events.AddEventType<BpnFeatureStarted>();
  options.Events.AddEventType<BpnFeatureError>();
  options.Events.AddEventType<NodeInitialized>();
  options.Events.AddEventType<NodeFailed>();
  options.Events.AddEventType<FailedNodeReInitialized>();
  options.Events.AddEventType<NodeSucceeded>();
  options.Events.AddEventType<ConnectionUsed>();
  options.Events.AddEventType<ConnectionSkipped>();




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



// Add services to the container.
builder.Services.AddControllersWithViews()
.AddJsonOptions(options =>
{
  options.JsonSerializerOptions.Converters.Add(new BpnConverter());
});

builder.Services.AddOutputCache();
builder.Services.AddEndpointsApiExplorer(); // Enables OpenAPI
builder.Services.AddOpenApiDocument();      // Adds OpenAPI support for .NET Minimal APIs

builder.Services.AddHsts(options =>
{
  options.Preload = true;
  options.IncludeSubDomains = true;
  options.MaxAge = TimeSpan.FromDays(360);
  //options.ExcludedHosts.Add("example.com");
  //options.ExcludedHosts.Add("www.example.com");
});


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
          options.LoginPath = "/Account/Login";
          options.LogoutPath = "/Account/Logout";
        });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions
{
  OnPrepareResponse = ctx =>
  {
    // Cache static files like fonts for 1 year
    ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
  }
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.UseOutputCache();

app.UseOpenApi();   // Generates OpenAPI document
app.UseSwaggerUi(); // Adds Swagger UI

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


BpnRepository.Load();
BpnDiagramRepository.Load();

BpnFeature feature;
BpnFeatureDiagram diagram;
if (BpnRepository.All().Count == 0)
{
  HomeController.GenerateDefaultData(out feature, out diagram);
  BpnRepository.Add(feature);
  BpnDiagramRepository.Add(diagram);
  BpnRepository.Save();
  BpnDiagramRepository.Save();
}


app.RegisterAll();

app.Run();
