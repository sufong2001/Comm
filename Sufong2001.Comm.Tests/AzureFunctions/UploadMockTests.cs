using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Sufong2001.Comm.AzureFunctions.ServIns;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Test.AzureFunctions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sufong2001.Comm.Tests.AzureFunctions
{
    public class UploadMockTests : IClassFixture<AppicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly AppicationBaseFixture _app;

        public UploadMockTests(ITestOutputHelper output, AppicationBaseFixture app)
        {
            this._output = output;
            _app = app;
        }

        [Fact]
        public async void UploadMockTest()
        {
            var repos = new MockRepository(MockBehavior.Strict);
            var container = repos.Create<CloudBlobContainer>(new Uri("https://localhost/"));
            container.Setup(x => x.CreateIfNotExistsAsync()).Returns(Task.FromResult(true));

            var cloudBlobDirectory = repos.Create<CloudBlobDirectory>();
            cloudBlobDirectory.Setup(x => x.Container).Returns<CloudBlobContainer>((c) => c);
            cloudBlobDirectory.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()).UploadFromStreamAsync(It.IsAny<Stream>())).Returns(Task.FromResult(true));

            var cloudTable = repos.Create<CloudTable>();
            cloudTable.Setup(x => x.CreateIfNotExistsAsync()).Returns(Task.FromResult(true));

            var request = TestFactory.CreateHttpRequest("name", "Bill");
            var response = (OkObjectResult)await UploadFunctions.Start(request, "manifest.js",
                cloudBlobDirectory.Object,
                cloudTable.Object,
                new IdGenerator(),
                new App(),
                _logger);

            repos.Verify();
        }
    }
}