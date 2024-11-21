using CanineSourceRepository.BusinessProcessNotation.Engine;
using Task = System.Threading.Tasks.Task;

namespace EngineEvents;

public class EngineEventsBackgroundService : BackgroundService
{
  private readonly IDocumentStore _store;
  public EngineEventsBackgroundService(IDocumentStore store)
  {
    _store = store;
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    List<IEngineEvents> batch = [];

    while (!stoppingToken.IsCancellationRequested)
    {//TODO: BUG issue... we might stop logging before the system is finished, thus losing logs (example if the server is restarted)

      batch = EngineEventsQueue.DequeueEngineEvents();
      if (batch.Any())
      {
        using (var session = _store.LightweightSession())
        {
          foreach (var stream in batch.GroupBy(p=>p.CorrelationId))
          {
            var firstEvent = stream.First();
            await session.RegisterEvents(CancellationToken.None, firstEvent.CorrelationId, firstEvent.FeatureId, stream.ToArray());
          }

        }
        batch.Clear();
      } else
      {
        await Task.Delay(2500, stoppingToken);
      }
    }
  }
}
