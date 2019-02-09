using Microsoft.WindowsAzure.Storage.Blob;

namespace Sufong2001.Comm.AzureStorage.Interfaces
{
    public interface IBlobRepository
    {
        CloudBlobDirectory GetBlobDirectory(string directoryPath);

        CloudBlockBlob GetBlockBlob(string filePath);
    }
}