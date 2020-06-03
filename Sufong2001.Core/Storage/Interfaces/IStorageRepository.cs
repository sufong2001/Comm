using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;

namespace Sufong2001.Core.Storage.Interfaces
{
    public interface IStorageRepository
    {
        CloudQueue GetQueue(string queueName);

        CloudTable GetTable(string tableName);

        CloudBlobDirectory GetBlobDirectory(string directoryPath);

        CloudBlockBlob GetBlockBlob(string filePath);
    }
}