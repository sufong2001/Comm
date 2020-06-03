using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Sufong2001.Accounting.Api.Storage.Names;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.Assembly;
using Sufong2001.Share.Arrays;
using Sufong2001.Share.String;

namespace Sufong2001.Accounting.Api.Storage
{
    public class StorageRepository : IStorageRepository, IQueueRepository, ITableRepository, IBlobRepository
    {
        public Guid Guid = Guid.NewGuid();

        private readonly CloudBlobClient _blobClient;
        private readonly CloudTableClient _cloudTableClient;
        private readonly CloudQueueClient _cloudQueueClient;

        public StorageRepository(StorageAccount storageAccount)
        {
            _blobClient = storageAccount.CreateCloudBlobClient();

            _cloudTableClient = storageAccount.CreateCloudTableClient();

            _cloudQueueClient = storageAccount.CreateCloudQueueClient();

            CreateStorageIfNotExists().ConfigureAwait(false);
        }

        public async Task<bool[]> CreateStorageIfNotExists()
        {
            var cloudTables = Enum.GetNames(typeof(TableName))
                 .Select(n => _cloudTableClient.GetTableReference(n).CreateIfNotExistsAsync())
                 .ToArray();

            var cloudQueues = typeof(QueueName).GetStaticValues()
                .Select(n => _cloudQueueClient.GetQueueReference(n).CreateIfNotExistsAsync())
                .ToArray();

            var cloudBlobs = typeof(BlobName).GetStaticValues()
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