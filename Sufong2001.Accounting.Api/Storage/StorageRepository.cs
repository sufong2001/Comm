using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Sufong2001.Accounting.Api.Functions.Authorization.Names;
using Sufong2001.Accounting.Api.Functions.Webhooks.Names;
using Sufong2001.Accounting.Api.Storage.Names;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.Assembly;
using Sufong2001.Share.String;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sufong2001.Accounting.Api.Storage
{
    public class StorageRepository : IStorageRepository, IQueueRepository, ITableRepository, IBlobRepository, ICosmosDbRepository
    {
        public Guid Guid = Guid.NewGuid();

        private readonly CloudBlobClient _blobClient;
        private readonly CloudTableClient _cloudTableClient;
        private readonly CloudQueueClient _cloudQueueClient;
        private readonly CosmosClient _cosmosClient;

        public StorageRepository(StorageAccount storageAccount, CosmosClient cosmosClient)
        {
            _blobClient = storageAccount.CreateCloudBlobClient();

            _cloudTableClient = storageAccount.CreateCloudTableClient();

            _cloudQueueClient = storageAccount.CreateCloudQueueClient();

            _cosmosClient = cosmosClient;

            var result = CreateStorageIfNotExists().Result;
        }

        public async Task<bool[]> CreateStorageIfNotExists()
        {
            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(nameof(DatabaseName.Sufong2001));

            var containers = new[]
            {
                database.CreateContainerIfNotExistsAsync(nameof(ContainerName.AccoTokens), $"/{nameof(PartitionKeyName.pk)}"),
                database.CreateContainerIfNotExistsAsync(nameof(WebhookContainerName.WebhookPayloads), $"/{nameof(PartitionKeyName.pk)}"),
                database.CreateContainerIfNotExistsAsync(nameof(CommonContainerName.DataList), $"/{nameof(PartitionKeyName.pk)}")
            };

            await Task.WhenAll(containers);

            var cloudTables = Enum.GetNames(typeof(ContainerName))
                 .Select(n => _cloudTableClient.GetTableReference(n).CreateIfNotExistsAsync())
                 .ToArray();

            var webhookTables = Enum.GetNames(typeof(WebhookContainerName))
                .Select(n => _cloudTableClient.GetTableReference(n).CreateIfNotExistsAsync())
                .ToArray();

            var webhookQueues = typeof(WebhookQueueNames).GetStaticValues()
                .Select(n => _cloudQueueClient.GetQueueReference(n).CreateIfNotExistsAsync())
                .ToArray();

            //var cloudQueues = typeof(QueueName).GetStaticValues()
            //    .Select(n => _cloudQueueClient.GetQueueReference(n).CreateIfNotExistsAsync())
            //    .ToArray();

            //var cloudBlobs = typeof(BlobName).GetStaticValues()
            //    .Select(n => n.Split("/").First())
            //    .Distinct()
            //    .Select(n => _blobClient.GetContainerReference(n).CreateIfNotExistsAsync())
            //    .ToArray();

            var tasks = cloudTables.Concat(webhookTables).Concat(webhookQueues); // .Concat(cloudQueues).Concat(cloudBlobs);

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

        public Container GetContainer(string containerName)
        {
            return _cosmosClient.GetContainer(nameof(DatabaseName.Sufong2001), containerName);
        }
    }
}