using Task = System.Threading.Tasks.Task;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.ServerFeatures;

public class RestartServerFeature : IFeature
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/RestartServer", async (HttpContext context, [FromServices] IDocumentSession session, [FromServices] IHostApplicationLifetime applicationLifetime /*maybe refactor?*/, CancellationToken ct) =>
    {
      context.Response.StatusCode = StatusCodes.Status202Accepted;
      await context.Response.WriteAsJsonAsync(new { Message = "Server is restarting." });
      await context.Response.CompleteAsync();
      await Task.Delay(100); // Wait for a short time before shutting down
      // Shut down the application gracefully
      applicationLifetime.Restart();
    }).WithName("RestartServer")
     .Produces(StatusCodes.Status202Accepted)
     .WithTags("Server");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
  }
}
