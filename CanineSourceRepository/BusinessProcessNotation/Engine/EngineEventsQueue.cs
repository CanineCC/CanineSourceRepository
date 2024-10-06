namespace EngineEvents;

public class EngineEventsQueue
{
  private static System.Collections.Queue jobs = [];

  public static void EnqueueEngineEvents(IEngineEvents jobData)
  {
    jobs.Enqueue(jobData);
  }
  public static IEngineEvents? DequeueEngineEvents()
  {
    if (jobs.Count == 0) return null;

    return (IEngineEvents?)jobs.Dequeue(); 
  }

  public static bool EmptyQueue()
  {
    return jobs.Count == 0;
  }

}
