using Microsoft.Azure.Storage.Blob;

namespace Sufong2001.Core.Storage.Interfaces
{
    public interface IBlobRepository
    {
        CloudBlobDirectory GetBlobDirectory(string directoryPath);

        CloudBlockBlob GetBlockBlob(string filePath);
    }
}