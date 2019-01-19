using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureStorage;
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    public static class StorageExtensions
    {
        public static async Task CreateIn(this TransferEntity transfer, CloudTable cloudTable)
        {
            var insertOperation = TableOperation.Insert(transfer);

            await cloudTable.ExecuteAsync(insertOperation);
        }

        public static async Task<TransferEntity> MarkAsCompleted(this TransferEntity transfer, CloudTable cloudTable)
        {
            var completedTransfer = transfer.CloneTo("completed");

            var deleteOperation = TableOperation.Delete(transfer);
            var insertOperation = TableOperation.Insert(completedTransfer);

            var ops = new[]
            {
                cloudTable.ExecuteAsync(deleteOperation),
                cloudTable.ExecuteAsync(insertOperation),
            };

            await Task.WhenAll(ops);

            return completedTransfer;
        }

        public static TransferEntity CloneTo(this TransferEntity transfer, string partitionKey)
        {
            return new TransferEntity(partitionKey)
            {
                RowKey = transfer.RowKey,
                ManifestFile = transfer.ManifestFile
            };
        }
    }
}