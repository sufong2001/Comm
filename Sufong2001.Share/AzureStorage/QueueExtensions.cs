
using Sufong2001.Share.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Queue;

namespace Sufong2001.Share.AzureStorage
{
    public static class QueueExtensions
    {
        public static async Task AddMessageToAsync(this object obj, CloudQueue queue)
        {
            await queue.AddMessageAsync(new CloudQueueMessage(obj.ToJson()));
        }
    }
}