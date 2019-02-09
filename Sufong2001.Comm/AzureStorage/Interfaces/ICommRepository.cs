using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Sufong2001.Comm.AzureStorage.Interfaces
{
    public interface ICommRepository
    {
        CloudQueue GetQueue(string queueName);

        CloudTable GetTable(string tableName);

        CloudBlobDirectory GetBlobDirectory(string directoryPath);

        CloudBlockBlob GetBlockBlob(string filePath);
    }
}