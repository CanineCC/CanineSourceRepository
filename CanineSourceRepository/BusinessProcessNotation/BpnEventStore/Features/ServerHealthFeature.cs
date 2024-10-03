namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;

public class ServerHealthFeature : IFeature
{
  private readonly static DateTimeOffset ServerStartTime = DateTimeOffset.Now;
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapPost($"BpnEngine/v1/Health", async (HttpContext context, [FromServices] IDocumentSession session, [FromServices] IHostApplicationLifetime applicationLifetime /*maybe refactor?*/, CancellationToken ct) =>
    {
      bool isHealthy = true; // Replace with health check logic
      string message = "Server is OK.";

      Process currentProcess = Process.GetCurrentProcess();
      long memoryUsage = currentProcess.WorkingSet64; // in bytes
      double memoryUsageMB = memoryUsage / (1024.0 * 1024.0);

      try
      {
        // Perform a simple query to check connectivity
        await session.Query<BpnContext.BpnContextProjection.BpnContext>().Take(1).ToListAsync(ct); // Replace YourEntity with a valid entity type in your database
      }
      catch (Exception ex)
      {
        isHealthy = false;
        message = $"Database is unavailable: {ex.Message}";
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
      }

      if (isHealthy)
      {
        context.Response.StatusCode = StatusCodes.Status200OK;
      }

      await context.Response.WriteAsJsonAsync(new
      {
        Health = isHealthy,
        Message = message,
        Started = ServerStartTime,
        MemoryUsage = $"Memory Usage: {memoryUsageMB:F2} MB"
      });
      await context.Response.CompleteAsync();

    }).WithName("ServerHealth")
     .Produces(StatusCodes.Status200OK)
     .Produces(StatusCodes.Status503ServiceUnavailable)
     .WithTags("Server");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
  }
}
