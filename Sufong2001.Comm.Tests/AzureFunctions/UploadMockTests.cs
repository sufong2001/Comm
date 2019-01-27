using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Sufong2001.Comm.AzureFunctions.ServIns;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Share.Json;
using Sufong2001.Test.AzureFunctions;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Sufong2001.Comm.Tests.AzureFunctions
{
    public class UploadMockTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly ApplicationBaseFixture _app;

        public UploadMockTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
        }

        [Fact(Skip = "Not Ready to use just a sample")]
        public async void UploadMockTest()
        {
            var repos = new MockRepository(MockBehavior.Strict);
            var container = repos.Create<CloudBlobContainer>(new Uri("https://localhost/"));

            var storageUri = repos.Create<StorageUri>(new Uri("https://localhost/"));

            var blob = repos.Create<CloudBlockBlob>(new Uri("https://localhost/"));
            blob.Setup(x => x.UploadFromStreamAsync(It.IsAny<Stream>())).Returns(Task.FromResult(true));

            var cloudBlobDirectory = repos.Create<CloudBlobDirectory>(storageUri, "", container);
            cloudBlobDirectory.Setup(x => x.GetBlockBlobReference(It.IsAny<string>())).Returns(() => blob.Object);

            var cloudTable = repos.Create<CloudTable>();
            cloudTable.Setup(x => x.CreateIfNotExistsAsync()).Returns(Task.FromResult(true));

            var request = TestFactory.CreateHttpRequestWithDataStream($"Data/{CommunicationManifest.FileName}");

            var response = (OkObjectResult)await UploadFunctions.Start(request,
                CommunicationManifest.FileName,
                cloudBlobDirectory.Object,
                cloudTable.Object,
                new IdGenerator(),
                new App(),
                _logger);

            Assert.NotNull(response.Value);

            repos.VerifyAll();

            _output.WriteLine(response.Value.ToJson(Formatting.Indented));
        }
    }
}