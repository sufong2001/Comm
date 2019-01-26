using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Share.String;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Sufong2001.Comm.Tests.Base
{
    public class CommRepository
    {
        private readonly CloudBlobClient _blobClient;
        private readonly CloudTableClient _cloudTableClient;
        private readonly CloudQueueClient _cloudQueueClient;

        public CommRepository(string connectionString)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);

            _blobClient = cloudStorageAccount.CreateCloudBlobClient();

            _cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

            _cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();

        }

        public CloudQueue GetQueue(string queueName)
        {
            var cloudQueue = _cloudQueueClient.GetQueueReference(queueName);
            return cloudQueue;
        }

        public CloudTable GetTable(string tableName)
        {
            var cloudTable = _cloudTableClient.GetTableReference(tableName);
            return cloudTable;
        }

        public CloudBlobDirectory GetBlobDirectory(string directoryPath)
        {
            var folders = directoryPath.Split("/");
            var container = folders.First();
            var relativePath = folders.Skip(1).ToArray().ArrayToString("/");

            var cloudBlobDirectory = _blobClient.GetContainerReference(container).GetDirectoryReference(relativePath);

            cloudBlobDirectory.Container.CreateIfNotExistsAsync().ConfigureAwait(true);

            return cloudBlobDirectory;
        }

        public CloudBlockBlob GetBlockBlob(string filePath)
        {
            var paths = filePath.Split("/");
            var container = paths.First();
            var file = paths.Skip(1).ToArray().ArrayToString("/");

            var cloudBlockBlob = _blobClient.GetContainerReference(container)
                .GetBlockBlobReference(file);

            return cloudBlockBlob;
        }
    }
}