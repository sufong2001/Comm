using System;
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
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Models.Storage;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class End
    {
        [FunctionName(ServiceNames.TransferEnd)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.TransferEnd + "/{session}/{filename?}")] HttpRequest req,
            string session,
            string filename,
            [Blob("comm/upload/{session}")] CloudBlobDirectory uploadDir,
            [Table("CommUpload")] CloudTable uploadTable,
            [Table("CommUpload", "temp", "{session}")] TableEntityAdapter<UploadSession> upload,
            [Queue("comm-process")] CloudQueue processQueue,
            [Inject()] App app,
            ILogger log)
        {
            if (upload == null) return new BadRequestObjectResult("Invalid Session Id.");

            await processQueue.CreateIfNotExistsAsync();

            // update the OriginalEntity.Property value has cause some funny error without cloning the object
            // System.Private.CoreLib: Exception while executing function: TransferEnd.
            // Microsoft.Azure.WebJobs.Host: Error while handling parameter upload after function returned:.
            // Microsoft.WindowsAzure.Storage: Not Found.
            //
            // upload.OriginalEntity.UploadEnd = app.DateTimeNow;

            upload = await upload.MoveTo(uploadTable, uploadSession => "completed"
                , updateOriginalEntity: uploadSession =>
                {
                    uploadSession.UploadEnd = app.DateTimeNow;
                    return uploadSession;
                }
            );

            if (filename != null)
            {
                await uploadDir.GetBlockBlobReference(filename)
                  .UploadFromStreamAsync(source: new StreamReader(req.Body).BaseStream);
            }

            await processQueue.AddMessageAsync(new CloudQueueMessage(new UploadCompleted()
                {
                    SessionId = session
                }.ToJson()
            ));

            return new OkObjectResult(new { upload.RowKey });
        }
    }
}