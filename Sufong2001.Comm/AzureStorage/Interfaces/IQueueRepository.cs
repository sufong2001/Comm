using Microsoft.Azure.Storage.Queue;

namespace Sufong2001.Comm.AzureStorage.Interfaces
{
    public interface IQueueRepository
    {
        CloudQueue GetQueue(string queueName);
    }
}