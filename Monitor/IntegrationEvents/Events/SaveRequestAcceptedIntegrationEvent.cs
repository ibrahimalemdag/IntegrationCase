
using EventBus.Library.Events;

namespace IntegrationEvents.Events
{
    public class SaveRequestAcceptedIntegrationEvent : IntegrationEvent
    {
        public string ResourceHash;
        public SaveRequestAcceptedIntegrationEvent(string resourceHash)
        {
            ResourceHash = resourceHash;
        }
    }
}
