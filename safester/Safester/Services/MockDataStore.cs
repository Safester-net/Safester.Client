using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Safester.Models;

namespace Safester.Services
{
    public class MockDataStore : IDataStore<Message>
    {
        List<Message> items;

        public MockDataStore()
        {
            items = new List<Message>();
        }

        public async Task<bool> AddItemAsync(Message item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Message item)
        {
            var oldItem = items.Where((Message arg) => arg.messageId == item.messageId).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var oldItem = items.Where((Message arg) => arg.messageId == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Message> GetItemAsync(int id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.messageId == id));
        }

        public async Task<IEnumerable<Message>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}