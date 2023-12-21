using EventBus.Library.Abstractions;
using Integration.Service;
using System.Collections.Concurrent;

namespace IntegrationEvents.Events
{
    internal class ResourceTakenIntegrationEventHandler : IIntegrationEventHandler<ResourceTakenIntegrationEvent>
    {
        public IIntegrationService Service { get; set; }
        public ResourceTakenIntegrationEventHandler(IIntegrationService service)
        {
            Service = service;
        }

        public Task Handle(ResourceTakenIntegrationEvent @event)
        {
            Service.SaveItem(@event.ResourceHash);
            return Task.CompletedTask;
        }
    }
}
