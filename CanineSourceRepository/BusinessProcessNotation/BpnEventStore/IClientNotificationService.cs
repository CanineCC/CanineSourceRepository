namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore;

public interface IClientNotificationService
{
  Task UpdateBpnContext();
  Task UpdateBpnFeature(Guid bpnFeatureId);
}