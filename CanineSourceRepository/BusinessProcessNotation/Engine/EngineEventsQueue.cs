using System.Collections.Concurrent;

namespace EngineEvents;

public static class EngineEventsQueue
{
  private static ConcurrentQueue<IEngineEvents> currentQueue = new ConcurrentQueue<IEngineEvents>();
  private static ConcurrentQueue<IEngineEvents> processingQueue = new ConcurrentQueue<IEngineEvents>();

  public static void EnqueueEngineEvents(IEngineEvents jobData)
  {
    currentQueue.Enqueue(jobData);
  }
  public static List<IEngineEvents> DequeueEngineEvents()
  {
    List<IEngineEvents> eventsList = new List<IEngineEvents>();

    var tempQueue = processingQueue;
    processingQueue = currentQueue;
    currentQueue = tempQueue;

    while (processingQueue.TryDequeue(out var job))
    {
      eventsList.Add(job);
    }

    return eventsList;
  }
}

