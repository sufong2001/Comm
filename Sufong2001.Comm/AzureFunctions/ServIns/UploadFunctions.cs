using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Models.Storage.Partitions;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.String;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    [DependencyInjectionConfig(typeof(CommConfig))]
    public static class UploadFunctions
    {
        [FunctionName(ServiceNames.UploadStart)]
        public static async Task<IActionResult> Start(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.UploadStart + "/upload/{filename}")]
            HttpRequest req,
            string filename,
            [Blob(BlobNames.UploadDirectory)] CloudBlobDirectory uploadDir,
            [Table(TableNames.CommUpload)] CloudTable uploadTmpTable,
            [Inject] IUploadIdGenerator idGenerator,
            [Inject] App app,
            ILogger log
            )
        {
            try
            {
                var sessionId = idGenerator.UploadSessionId();

                if (filename.IsNullOrEmpty()) throw new ArgumentException("A filename is required.", nameof(filename));


                //var provider = new MultipartMemoryStreamProvider();
                //var uploadTo = req.Content.ReadAsMultipartAsync(provider);// .Body.UploadTo(uploadDir, $"{sessionId}/{filename}");

                //var contents = uploadTo.Result.Contents;

                //await contents.First().ReadAsStreamAsync().Result.UploadTo(uploadDir, $"{sessionId}/{filename}");


                await req.Body.UploadTo(uploadDir, $"{sessionId}/{filename}");

                var uploadSession = new UploadSession
                {
                    SessionId = sessionId,
                    UploadStart = app.DateTimeNow,
                    ManifestFile = filename.IsIfManifest(),
                    LastUploadedFile = filename
                };

                var result = await uploadSession.CreateIn(uploadTmpTable, CommUploadPartitionKeys.Temp, sessionId);

                return new OkObjectResult(uploadSession);
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                return new BadRequestObjectResult(new UploadSession { Errors = ex.Message });
            }
        }

        [FunctionName(ServiceNames.UploadContinue)]
        public static async Task<IActionResult> Continue(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.UploadContinue + "/{session}/{filename}")]
            HttpRequest req,
            string session,
            string filename,
            [Blob(BlobNames.UploadDirectory + "/{session}")]CloudBlobDirectory uploadDir,
            [Table(TableNames.CommUpload, CommUploadPartitionKeys.Temp, "{session}")] TableEntityAdapter<UploadSession> upload,
            [Table(TableNames.CommUpload)] CloudTable uploadTable,
            ILogger log)
        {
            try
            {
                if (filename.IsNullOrEmpty()) throw new ArgumentException("A filename is required.", nameof(filename));

                var uploadTo = await req.Body.UploadTo(uploadDir, filename);

                upload.OriginalEntity.ManifestFile = new[] { upload.OriginalEntity.ManifestFile, filename }.IsIfManifest();
                upload.OriginalEntity.LastUploadedFile = filename;

                var result = await upload.UpdateIn(uploadTable);

                return new OkObjectResult(upload.OriginalEntity);
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                return new BadRequestObjectResult(new UploadSession { Errors = ex.Message });
            }
        }

        [FunctionName(ServiceNames.UploadEnd)]
        public static async Task<IActionResult> End(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.UploadEnd + "/{session}/{filename?}")]
            HttpRequest req,
            string session,
            string filename,
            [Blob(BlobNames.UploadDirectory + "/{session}")] CloudBlobDirectory uploadDir,
            [Table(TableNames.CommUpload, CommUploadPartitionKeys.Temp, "{session}")] TableEntityAdapter<UploadSession> upload,
            [Table(TableNames.CommUpload)] CloudTable uploadTable,
            [Queue(QueueNames.CommProcess)] CloudQueue processQueue,
            [Inject] App app,
            ILogger log)
        {
            try
            {
                if (session.IsNullOrEmpty())
                    throw new ArgumentException("The Session Id is required.", nameof(session));

                // save the file
                var uploadTo = await req.Body.UploadTo(uploadDir, filename);

                #region unexpected behaviour notes

                /*
                update the OriginalEntity.Property value has cause some funny error without cloning the object
                System.Private.CoreLib: Exception while executing function: TransferEnd.
                Microsoft.Azure.WebJobs.Host: Error while handling parameter upload after function returned:.
                Microsoft.WindowsAzure.Storage: Not Found.

                upload.OriginalEntity.UploadEnd = app.DateTimeNow;
                */

                #endregion unexpected behaviour notes

                UploadSession UpdateOriginalEntity(UploadSession uploadSession)
                {
                    uploadSession.UploadEnd = app.DateTimeNow;
                    uploadSession.ManifestFile = new[] { upload.OriginalEntity.ManifestFile, filename }.IsIfManifest();
                    uploadSession.LastUploadedFile = new[] { filename, uploadSession.LastUploadedFile }.FirstIsNotEmpty();
                    return uploadSession;
                }

                // close the upload session
                upload = await upload.MoveTo(uploadTable
                    , uploadSession => CommUploadPartitionKeys.Completed
                    , updateOriginalEntity: UpdateOriginalEntity
                );

                // raise the UploadCompleted event
                await new UploadCompleted { SessionId = session }.AddMessageToAsync(processQueue);

                return new OkObjectResult(upload.OriginalEntity);
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                return new BadRequestObjectResult(new UploadSession { Errors = ex.Message });
            }
        }
    }
}