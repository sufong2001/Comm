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
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Share.Json;
using System.IO;
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class UploadFunctions
    {
        [FunctionName(ServiceNames.UploadStart)]
        public static async Task<IActionResult> Start(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.UploadStart + "/upload/{filename}")]
            HttpRequest req,
            string filename,
            [Blob(BlobNames.UploadDirectory)] CloudBlobDirectory uploadDir,
            [Table(TableNames.CommUpload)] CloudTable uploadTmpTable,
            [Inject()] IUploadIdGenerator idGenerator,
            [Inject()] App app,
            ILogger log)
        {

#if !DEBUG
            // uploadDir.Container cannot be mocked therefore it is excluded in the DEBUG mode
            await uploadDir.Container.CreateIfNotExistsAsync();            
#endif
            await uploadTmpTable.CreateIfNotExistsAsync();

            var uploadSession = new UploadSession
            {
                SessionId = idGenerator.UploadSessionId(),
                UploadStart = app.DateTimeNow,
                ManifestFile = filename
            };

            await uploadSession.CreateIn(uploadTmpTable, UploadSessionPartitionKeys.Temp, uploadSession.SessionId);

            var uploadTo = uploadDir.GetBlockBlobReference($"{uploadSession.SessionId}/{filename}");

            await uploadTo.UploadFromStreamAsync(source: new StreamReader(req.Body).BaseStream);

            uploadSession.LastUploadedFile = uploadTo.Name;

            return new OkObjectResult(uploadSession);
        }

        [FunctionName(ServiceNames.UploadContinue)]
        public static async Task<IActionResult> Continue(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.UploadContinue + "/{session}/{filename}")]
            HttpRequest req,
            string session,
            string filename,
            [Blob(BlobNames.UploadDirectory + "/{session}")] CloudBlobDirectory uploadDir,
            [Table(TableNames.CommUpload, UploadSessionPartitionKeys.Temp, "{session}")] UploadSession upload,
            ILogger log)
        {
            upload.LastUploadedFile = filename;

            var uploadTo = uploadDir.GetBlockBlobReference(filename);

            await uploadTo.UploadFromStreamAsync(source: new StreamReader(req.Body).BaseStream);

            upload.LastUploadedFile = uploadTo.Name;

            return new OkObjectResult(upload);
        }

        [FunctionName(ServiceNames.UploadEnd)]
        public static async Task<IActionResult> End(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route =
                ServiceNames.UploadEnd + "/{session}/{filename?}")]
            HttpRequest req,
            string session,
            string filename,
            [Blob(BlobNames.UploadDirectory + "/{session}")] CloudBlobDirectory uploadDir,
            [Table(TableNames.CommUpload)] CloudTable uploadTable,
            [Table(TableNames.CommUpload, UploadSessionPartitionKeys.Temp, "{session}")] TableEntityAdapter<UploadSession> upload,
            [Queue(QueueNames.CommProcess)] CloudQueue processQueue,
            [Inject()] App app,
            ILogger log)
        {
            if (upload == null) return new BadRequestObjectResult("Invalid Session Id.");

            await processQueue.CreateIfNotExistsAsync();

            #region unexpected behaviour notes

            /*
            update the OriginalEntity.Property value has cause some funny error without cloning the object
            System.Private.CoreLib: Exception while executing function: TransferEnd.
            Microsoft.Azure.WebJobs.Host: Error while handling parameter upload after function returned:.
            Microsoft.WindowsAzure.Storage: Not Found.

            upload.OriginalEntity.UploadEnd = app.DateTimeNow;
            */

            #endregion unexpected behaviour notes

            upload = await upload.MoveTo(uploadTable, uploadSession => UploadSessionPartitionKeys.Completed
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

            return new OkObjectResult(upload);
        }
    }
}