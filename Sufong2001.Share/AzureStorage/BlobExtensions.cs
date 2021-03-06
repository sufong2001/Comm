﻿using Microsoft.Azure.Storage.Blob;
using Sufong2001.Share.String;
using System;
using System.IO;
using System.Threading.Tasks;
using Sufong2001.Share.Json;

namespace Sufong2001.Share.AzureStorage
{
    public static class BlobExtensions
    {
        public static async Task<CloudBlockBlob> UploadTo(this Stream stream, CloudBlobDirectory directory, string filename)
        {
            if (filename.IsNullOrEmpty()) return null;

            if (stream == null || stream.Length == 0) throw new ArgumentException("No file data has been uploaded.", nameof(stream));

            var cloudBlockBlob = directory.GetBlockBlobReference(filename);

            // Upload will override any existing file by default
            await cloudBlockBlob.UploadFromStreamAsync(source: stream);

            return cloudBlockBlob;
        }

        public static async Task<T> DownloadTextAsAsync<T>(this CloudBlockBlob blob)
        {
            var str = await blob.DownloadTextAsync();

            return str.To<T>();
        }
    }
}