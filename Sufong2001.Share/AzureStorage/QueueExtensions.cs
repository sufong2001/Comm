using Microsoft.Azure.Storage.Queue;
using Sufong2001.Share.Json;
using System.Threading.Tasks;

namespace Sufong2001.Share.AzureStorage
{
    public static class QueueExtensions
    {
        public static async Task AddMessageToAsync(this object obj, CloudQueue queue)
        {
            var msg = obj is string s ? s : obj.ToJson();

            await queue.AddMessageAsync(new CloudQueueMessage(msg));
        }
    }
}