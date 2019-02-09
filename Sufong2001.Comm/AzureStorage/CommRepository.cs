using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureStorage.Interfaces;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Share.Assembly;
using Sufong2001.Share.String;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureStorage
{
    public class CommRepository : ICommRepository, IQueueRepository, ITableRepository, IBlobRepository
    {
        public Guid Guid = Guid.NewGuid();

        private readonly CloudBlobClient _blobClient;
        private readonly CloudTableClient _cloudTableClient;
        private readonly CloudQueueClient _cloudQueueClient;

        public CommRepository(StorageAccount storageAccount)
        {
            _blobClient = storageAccount.CreateCloudBlobClient();

            _cloudTableClient = storageAccount.CreateCloudTableClient();

            _cloudQueueClient = storageAccount.CreateCloudQueueClient();
        }

        public async Task<bool[]> CreateStorageIfNotExists()
        {
            var cloudTables = typeof(TableNames).GetStaticValues()
                 .Select(n => _cloudTableClient.GetTableReference(n).CreateIfNotExistsAsync())
                 .ToArray();

            var cloudQueues = typeof(QueueNames).GetStaticValues()
                .Select(n => _cloudQueueClient.GetQueueReference(n).CreateIfNotExistsAsync())
                .ToArray();

            var cloudBlobs = typeof(BlobNames).GetStaticValues()
                .Select(n => n.Split("/").First())
                .Distinct()
                .Select(n => _blobClient.GetContainerReference(n).CreateIfNotExistsAsync())
                .ToArray();

            var tasks = cloudTables.Concat(cloudQueues).Concat(cloudBlobs);

            return await Task.WhenAll(tasks);
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