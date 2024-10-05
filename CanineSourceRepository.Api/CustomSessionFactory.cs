using Marten;

public class CustomSessionFactory : ISessionFactory
{
 // private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
  private readonly IDocumentStore _store;

  // This is important! You will need to use the
  // IDocumentStore to open sessions
  public CustomSessionFactory(IDocumentStore store)
  {
    _store = store;
  }

  public IQuerySession QuerySession()
  {
   // semaphore.Wait();
    try
    {
      return _store.QuerySession();
    } finally
    {
  //    semaphore.Release();
    }
  }

  public IDocumentSession OpenSession()
  {
  //  semaphore.Wait();
    try
    {
      return _store.LightweightSession(System.Data.IsolationLevel.ReadCommitted);
    }
    finally
    {
//      semaphore.Release();
    }
  }
}
