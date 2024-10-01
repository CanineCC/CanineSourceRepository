namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore;

public interface IFeature
{
  public abstract static void RegisterBpnEventStore(WebApplication app);
  public abstract static void RegisterBpnEvents(StoreOptions options);
}
