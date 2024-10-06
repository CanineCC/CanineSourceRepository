using CanineSourceRepository.BusinessProcessNotation.Engine;

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

      var data = EngineEventsQueue.DequeueEngineEvents();//add to list, while list length < 50 or until data==null.... to bulk insert...
      if (data != null)
      {
        batch.Add(data);
      }
      
      if (batch.Count > 50 || (data == null && batch.Count > 0))
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
      }

      if (data == null)
      {
        await Task.Delay(2500, stoppingToken);
      }
    }
  }
}
