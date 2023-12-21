using Integration.Common;
using Integration.Backend;
using EventBus.Library.Abstractions;
using IntegrationEvents.Events;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Integration.Service;

public sealed class ItemIntegrationService : IIntegrationService
{
    IEventBus EventBus;

    public ItemIntegrationService(IEventBus eventBus)
    {
        EventBus = eventBus;
    }

    //This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

    // This is called externally and can be called multithreaded, in parallel.
    // More than one item with the same content should not be saved. However,
    // calling this with different contents at the same time is OK, and should
    // be allowed for performance reasons.
    public Result TrySaveItem(string itemContent)
    {
        Monitor.Enter(itemContent);
        // Check the backend to see if the content is already saved.
        if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
        {
            return new Result(false, $"Duplicate item received with content {itemContent}.");
        }

        EventBus.Publish(new SaveRequestAcceptedIntegrationEvent(itemContent));

        Monitor.Exit(itemContent);
        return new Result(true, $"Item with content {itemContent} saved");
    }

    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }

    public void SaveItem(string itemContent)
    {
        if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
        {
            return;
        }

        ItemIntegrationBackend.SaveItem(itemContent);
    }
}