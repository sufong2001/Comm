using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.Configurations.Resolvers;
using System.IO;
using System.Threading.Tasks;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class TransferStart
    {
        [FunctionName(ServiceNames.TransferStart)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = ServiceNames.TransferStart + "/upload/{filename}")] HttpRequest req,
            string filename,
            [Blob("comm/upload")] CloudBlobDirectory uploadDir,
            [Table("CommUpload")] CloudTable uploadTmpTable,
            [Inject()] ITransferIdGenerator idGenerator,
            ILogger log)
        {
            await uploadDir.Container.CreateIfNotExistsAsync();
            await uploadTmpTable.CreateIfNotExistsAsync();

            var transfer = new TransferEntity("temp")
            {
                RowKey = idGenerator.TransferSessionId(),
                ManifestFile = filename,
            };

            await transfer.CreateIn(uploadTmpTable);


            await uploadDir.GetBlockBlobReference($"{transfer.RowKey}/{filename}")
                .UploadFromStreamAsync(source: new StreamReader(req.Body).BaseStream);


            return new OkObjectResult(new { transfer.RowKey});
        }
    }
}