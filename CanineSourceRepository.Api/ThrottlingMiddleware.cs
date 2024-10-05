using System.Collections.Concurrent;

public class ThrottlingMiddleware
{
  private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
  private readonly RequestDelegate _next;
  private readonly TimeSpan _throttleDuration;

  private readonly TimeSpan _rateLimitDuration = TimeSpan.FromHours(1);
  private readonly int _maxRequestsPerHour = 20; // TODO: Based on license (have licensekey as part of the token)
  private readonly ConcurrentDictionary<string, DateTime> _clientsLastRequestTime;
  private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _clientsRequestTimes;


  public ThrottlingMiddleware(RequestDelegate next, TimeSpan throttleDuration)
  {
    _next = next;
    _throttleDuration = throttleDuration;
    _clientsLastRequestTime = new ConcurrentDictionary<string, DateTime>();
    _clientsRequestTimes = new ConcurrentDictionary<string, ConcurrentQueue<DateTime>>();
  }

  public async Task Invoke(HttpContext context)
  {
    semaphore.Wait();
    try
    {
      var clientId = context.Connection.RemoteIpAddress?.ToString(); // Use the IP address as a unique identifier
      
      if (clientId == null)
      {
        await _next(context); // No IP address available, skip throttling
        return;
      }
      string requestPath = context.Request.Path.ToString(); // Get the endpoint path
      string clientEndpointKey = $"{clientId}:{requestPath}";
      // Throttle check for the same endpoint
      if (_clientsLastRequestTime.TryGetValue(clientEndpointKey, out DateTime lastRequestTime))
      {
        var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
        if (timeSinceLastRequest < _throttleDuration)
        {
          context.Response.StatusCode = StatusCodes.Status429TooManyRequests; // Too Many Requests
          await context.Response.WriteAsync("Too many requests to the same endpoint. Please wait before trying again.");
          await context.Response.CompleteAsync();
          return;
        }
      }

      // Update the last request time for the endpoint
      _clientsLastRequestTime[clientEndpointKey] = DateTime.UtcNow;

      // Rate limit check for requests in the last hour
      var requestTimes = _clientsRequestTimes.GetOrAdd(clientId, _ => new ConcurrentQueue<DateTime>());
      requestTimes.Enqueue(DateTime.UtcNow);

      // Remove requests older than 1 hour
      while (requestTimes.TryPeek(out DateTime oldestRequestTime) && (DateTime.UtcNow - oldestRequestTime) > _rateLimitDuration)
      {
        requestTimes.TryDequeue(out _);
      }

      // Check the number of requests in the last hour
      if (requestTimes.Count > _maxRequestsPerHour)
      {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests; // Too Many Requests
        await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
        await context.Response.CompleteAsync();
        return;
      }

      await _next(context); // Proceed to the next middleware/endpoint
    }
    finally
    {
      semaphore.Release();
    }
  }
}
