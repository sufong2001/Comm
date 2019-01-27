using Microsoft.WindowsAzure.Storage.Blob;
using Sufong2001.Share.String;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sufong2001.Share.AzureStorage
{
    public static class BlobExtensions
    {
        public static async Task<CloudBlockBlob> UploadTo(this Stream steam, CloudBlobDirectory directory, string filename)
        {
            if (filename.IsNullOrEmpty()) return null;

            if (steam.Length == 0) throw new ArgumentException("No file data has been uploaded", nameof(steam));

            var cloudBlockBlob = directory.GetBlockBlobReference(filename);

            // Upload will override any existing file by default
            await cloudBlockBlob.UploadFromStreamAsync(source: steam);

            return cloudBlockBlob;
        }
    }
}