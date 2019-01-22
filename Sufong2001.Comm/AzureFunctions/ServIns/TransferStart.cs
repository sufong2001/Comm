using System;
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
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models.Storage;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class Start
    {
        [FunctionName(ServiceNames.TransferStart)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = ServiceNames.TransferStart + "/upload/{filename}")] HttpRequest req,
            string filename,
            [Blob("comm/upload")] CloudBlobDirectory uploadDir,
            [Table("CommUpload")] CloudTable uploadTmpTable,
            [Inject()] ITransferIdGenerator idGenerator,
            [Inject()] App app,
            ILogger log)
        {
            await uploadDir.Container.CreateIfNotExistsAsync();
            await uploadTmpTable.CreateIfNotExistsAsync();

            var uploadSession = new UploadSession()
            {
                SessionId = idGenerator.TransferSessionId(),
                UploadStart = app.DateTimeNow,
                ManifestFile = filename,
            };

            await uploadSession.CreateIn(uploadTmpTable, "temp", uploadSession.SessionId);


            await uploadDir.GetBlockBlobReference($"{uploadSession.SessionId}/{filename}")
                .UploadFromStreamAsync(source: new StreamReader(req.Body).BaseStream);


            return new OkObjectResult(new { uploadSession.SessionId });
        }
    }
}