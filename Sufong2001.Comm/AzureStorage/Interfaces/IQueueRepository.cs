using Microsoft.WindowsAzure.Storage.Queue;

namespace Sufong2001.Comm.AzureStorage.Interfaces
{
    public interface IQueueRepository
    {
        CloudQueue GetQueue(string queueName);
    }
}