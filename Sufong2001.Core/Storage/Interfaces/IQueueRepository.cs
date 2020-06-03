using Microsoft.Azure.Storage.Queue;

namespace Sufong2001.Core.Storage.Interfaces
{
    public interface IQueueRepository
    {
        CloudQueue GetQueue(string queueName);
    }
}