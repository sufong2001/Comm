using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Share.Json;
using System.IO;
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class TransferEnd
    {
        [FunctionName(ServiceNames.TransferEnd)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.TransferEnd + "/{session}/{filename?}")] HttpRequest req,
            string session,
            string filename,
            [Blob("comm/upload/{session}")] CloudBlobDirectory uploadDir,
            [Table("CommUpload")] CloudTable uploadTable,
            [Table("CommUpload", "temp", "{session}")] TransferEntity transfer,
            [Queue("comm-process")] CloudQueue processQueue,
            ILogger log)
        {
            if (transfer == null) return new BadRequestObjectResult("Invalid Session Id.");

            await processQueue.CreateIfNotExistsAsync();

            transfer = await transfer.MarkAsCompleted(uploadTable);

            if (filename != null)
            {
                await uploadDir.GetBlockBlobReference(filename)
                  .UploadFromStreamAsync(source: new StreamReader(req.Body).BaseStream);
            }

            await processQueue.AddMessageAsync(new CloudQueueMessage(new
            {
                MessageKey = transfer.RowKey
            }.ToJson()));

            return new OkObjectResult(new { transfer.RowKey });
        }
    }
}