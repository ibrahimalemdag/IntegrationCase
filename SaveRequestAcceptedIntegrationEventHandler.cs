using EventBus.Library.Abstractions;
using Integration.Common;
using IntegrationEvents.Events;

namespace IntegrationEvents.EventHandlers
{
    internal class SaveRequestAcceptedIntegrationEventHandler : IIntegrationEventHandler<SaveRequestAcceptedIntegrationEvent>
    {
        public IEventBus EventBus { get; private set; }
        public static List<Item> Resource = new List<Item>();

        public SaveRequestAcceptedIntegrationEventHandler(IEventBus eventBus)
        {
            EventBus = eventBus;
        }

        public Task Handle(SaveRequestAcceptedIntegrationEvent @event)
        {
            Monitor.Enter(@event.ResourceHash);

            if(Resource.Where(x=>x.Content.Equals(@event.ResourceHash)).Count() > 0)
            {
                return Task.CompletedTask;
            }

            Resource.Add(new Item() { Content = @event.ResourceHash, ExpireDate = DateTime.Now.AddMinutes(2) });
            EventBus.Publish(new ResourceTakenIntegrationEvent(@event.ResourceHash, @event.Id));

            Monitor.Exit(@event.ResourceHash);
            return Task.CompletedTask;
        }
    }
}
