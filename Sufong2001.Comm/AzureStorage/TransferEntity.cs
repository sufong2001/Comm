using Microsoft.WindowsAzure.Storage.Table;

namespace Sufong2001.Comm.AzureStorage
{
    public partial class TransferEntity : TableEntity
    {
        public TransferEntity(string partitionKey)
        {
            this.PartitionKey = partitionKey;
        }

        public TransferEntity() { }

        public string ManifestFile { get; set; }

    }
}