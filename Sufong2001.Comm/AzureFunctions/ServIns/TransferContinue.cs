using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sufong2001.Comm.AzureFunctions.Names;
using System.IO;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.AzureFunctions.ServIns
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class TransferContinue
    {
        [FunctionName(ServiceNames.TransferContinue)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = ServiceNames.TransferContinue + "/{session}/{filename}")] HttpRequest req,
            string session,
            string filename,
            [Blob("comm/upload/{session}")] CloudBlobDirectory uploadDir,
            [Table("CommUpload", "temp", "{session}")] TransferEntity transfer,
            ILogger log)
        {


            await uploadDir.GetBlockBlobReference(filename)
                .UploadFromStreamAsync(source: new StreamReader(req.Body).BaseStream);


            return new OkObjectResult(new { transfer.RowKey });
        }
    }
}