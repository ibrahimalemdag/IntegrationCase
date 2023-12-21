
using Integration.Common;

namespace Integration.Service
{
    internal interface IIntegrationService
    {
        public Result TrySaveItem(string itemContent);
        public void SaveItem(string itemContent);
        public List<Item> GetAllItems();
    }
}
