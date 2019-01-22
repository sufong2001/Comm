using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.Configurations.Resolvers;
using System.IO;
using System.Threading.Tasks;
using Sufong2001.Comm.Models.Storage;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class Continue
    {
        [FunctionName(ServiceNames.TransferContinue)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.TransferContinue + "/{session}/{filename}")] HttpRequest req,
            string session,
            string filename,
            [Blob("comm/upload/{session}")] CloudBlobDirectory uploadDir,
            [Table("CommUpload", "temp", "{session}")] UploadSession upload,
            ILogger log)
        {
            await uploadDir.GetBlockBlobReference(filename)
                .UploadFromStreamAsync(source: new StreamReader(req.Body).BaseStream);

            return new OkObjectResult(new { upload.SessionId });
        }
    }
}