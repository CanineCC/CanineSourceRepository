using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features;
//TODO: if not on environment, return "not found"
//TODO: PromoteToEnvironment (i.e. cant remove, but can add)
//TODO: DeprecateOnEnvironment (i.e. return "moved" if called) 
public class ServerHealthFeature : IFeature
{
  private readonly static DateTimeOffset ServerStartTime = DateTimeOffset.UtcNow;
  public record ServerHealth
  {
    public ServerHealth(bool isHealthy, string message, DateTimeOffset serverStartedTime, int serverMemoryUsageInMegaBytes)
    {
      IsHealthy = isHealthy;
      Message = message ?? throw new ArgumentNullException(nameof(message));
      ServerStartedTime = serverStartedTime;
      ServerMemoryUsageInMegaBytes = serverMemoryUsageInMegaBytes;
    }

    [Required]
    public bool IsHealthy { get; set; }

    [Required]
    public string Message { get; set; }

    [Required]
    public DateTimeOffset ServerStartedTime { get; set; }

    [Required]
    public int ServerMemoryUsageInMegaBytes { get; set; }
  }
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet($"BpnEngine/v1/Health", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
    {
        Process currentProcess = Process.GetCurrentProcess();
        long memoryUsage = currentProcess.WorkingSet64;
        int memoryUsageMB = (int)(memoryUsage / (1024.0 * 1024.0));

        try
        {
          bool isDbReachable = await session.Query<BpnContext.BpnContextProjection.BpnContext>().AnyAsync(ct);
          if (!isDbReachable)
          {
            throw new Exception("Database query failed.");
          }
        }
        catch (Exception ex)
        {
          return Results.Problem(statusCode: 503,
            title: "Database Health Check Failure",
            detail: $"Database is unavailable: {ex.Message}",
            instance: context.Request.Path
          );
        }
      context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
      {
        Public = true,
        MaxAge = TimeSpan.FromSeconds(15)
      };
      return Results.Ok(new ServerHealth(
          isHealthy: true,
          message: "Server is OK.",
          serverStartedTime: ServerStartTime,
          serverMemoryUsageInMegaBytes: memoryUsageMB
        ));
    }).WithName("ServerHealth")
     .Produces(StatusCodes.Status200OK, typeof(ServerHealth))
     .Produces(StatusCodes.Status503ServiceUnavailable)
     .WithTags("Server");
  }
  public static void RegisterBpnEvents(StoreOptions options)
  {
  }
}
