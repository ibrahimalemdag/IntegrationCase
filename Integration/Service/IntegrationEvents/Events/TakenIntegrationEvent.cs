using EventBus.Library.Events;

namespace IntegrationEvents.Events
{
    internal class ResourceTakenIntegrationEvent : IntegrationEvent
    {
        public string ResourceHash;
        public Guid AcknowledgeId;
        public ResourceTakenIntegrationEvent(string resourceHash, Guid acknowledgeId)
        {
            ResourceHash = resourceHash;
            AcknowledgeId = acknowledgeId;
        }
    }
}
