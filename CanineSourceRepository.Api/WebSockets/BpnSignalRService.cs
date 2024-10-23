using Npgsql;
using CanineSourceRepository.BusinessProcessNotation.BpnEventStore;
using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;

public class BpnSignalRService : IClientNotificationService, IHostedService
{
  private readonly IHubContext<BpnHub> _hubContext;
  private Timer _timer;
  private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(500);
  private readonly string _connectionString;
  private readonly Dictionary<string, long> _versions = new Dictionary<string, long>();

  public BpnSignalRService(IHubContext<BpnHub> hubContext, IConfiguration configuration)
  {
    _hubContext = hubContext;
    _connectionString = new NpgsqlConnectionStringBuilder
    {
      Host = configuration.GetSection("Marten:Host").Value,
      Port = int.Parse(configuration.GetSection("Marten:Port").Value!),
      Database = configuration.GetSection("Marten:Database").Value,
      Username = configuration.GetSection("Marten:Username").Value,
      Password = configuration.GetSection("Marten:Password").Value,
      MaxPoolSize = int.Parse(configuration.GetSection("Marten:MaxPoolSize").Value!),
      Pooling = bool.Parse(configuration.GetSection("Marten:Pooling").Value!)
    }.ConnectionString;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    // Start the timer when the service starts
    _timer = new Timer(async _ => await CheckForUpdates(cancellationToken), null, TimeSpan.Zero, _pollingInterval);
    return Task.CompletedTask;
  }
  private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

  private async Task CheckForUpdates(CancellationToken cancellationToken)
  {
    await _semaphore.WaitAsync(cancellationToken);
    try
    {
      await SendNewEvents(cancellationToken);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
    }
    finally { _semaphore.Release(); }
  }
  private async Task SendNewEvents(CancellationToken stoppingToken)
  {
    await using var conn = new NpgsqlConnection(_connectionString);
    await conn.OpenAsync(stoppingToken);

    // Command to select all relevant records
    //await using var cmd = new NpgsqlCommand("SELECT id, type, version FROM public.mt_streams;", conn);
    await using var cmd = new NpgsqlCommand(@"
SELECT id, mt_version, 'bpndraftfeature' as name FROM public.mt_doc_bpndraftfeatureprojection_bpndraftfeature
union
SELECT id, mt_version, 'bpncontext' as name FROM public.mt_doc_bpncontextprojection_bpncontext
union
SELECT id, mt_version, 'bpnfeature' as name FROM public.mt_doc_bpnfeatureprojection_bpnfeature
", conn);
    await using var reader = await cmd.ExecuteReaderAsync(stoppingToken);

    // We are using a flag to know if this is the first run
    bool firstRun = _versions.Count == 0;

    // Iterate over the result set
    while (await reader.ReadAsync(stoppingToken))
    {
      var id = await reader.GetFieldValueAsync<Guid>(0, stoppingToken);
      var version = await reader.GetFieldValueAsync<long>(1, stoppingToken);
      var typeName = reader.IsDBNull(1) ? null : await reader.GetFieldValueAsync<string>(2, stoppingToken);
      var key = $"{typeName}_{id}";

      if (firstRun)
      {
        // On the first run, populate the _versions dictionary
        _versions.Add(key, version); // Initialize version
      }
      else
      {
        // Check if this entry exists in _versions
        if (_versions.TryGetValue(key, out long existingVersion))
        {
          // Compare versions to see if we have an update
          if (existingVersion < version)
          {
            _versions[key] = version; // Update version in dictionary

            // Notify of updates based on type
            if (typeName == "bpncontext")
            {
              await UpdateBpnContext();
            }
            else if (typeName == "bpndraftfeature")
            {
              await UpdateBpnFeature(id);
            }
            else if (typeName == "bpnfeature")
            {
              //TODO: await UpdateBpnFeature(id);
            }
          }
        }
        else
        {
          // If the entry is not in _versions, add it
          _versions.Add(key, version); // Initialize new entry
        }
      }
    }
  }



  public async Task UpdateBpnContext()
  {
    await _hubContext.Clients.Group($"BpnContext")
        .SendAsync("ReceiveBpnContextUpdate", "The context has been updated.");
  }

  public async Task UpdateBpnFeature(Guid bpnFeatureId)
  {
    await _hubContext.Clients.Group($"BpnFeature-{bpnFeatureId}")
        .SendAsync("ReceiveBpnFeatureUpdate", bpnFeatureId, "The feature has been updated.");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    // Dispose of the timer when the service stops
    _timer?.Change(Timeout.Infinite, 0);
    _timer?.Dispose();
    return Task.CompletedTask;
  }
}