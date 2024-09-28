using EngineEvents;
using CanineSourceRepository.BusinessProcessNotation.Engine;
using CanineSourceRepository.Ui.Controllers;
using Marten.Events.Daemon.Resiliency;
using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
using Weasel.Core;
using Marten.Events.Projections;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.Context;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnFeatureProjection;

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






using (var scope = app.Services.CreateScope())
{
  var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
  await session.GenerateDefaultData(CancellationToken.None);

  app.RegisterAll(session);
}

app.Run();
