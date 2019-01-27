using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Share.Json;
using Sufong2001.Test.AzureFunctions;
using System;
using Moq;
using Newtonsoft.Json;
using Sufong2001.Comm.Interfaces;
using Xunit;
using Xunit.Abstractions;
using static Sufong2001.Comm.AzureFunctions.ServIns.UploadFunctions;

namespace Sufong2001.Comm.Tests.AzureFunctions
{
    public class UploadTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly ApplicationBaseFixture _app;

        public UploadTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
        }

        [Fact]
        public async void UploadStartTest()
        {
            //var idGenerator = new Mock<IUploadIdGenerator>();
            //idGenerator.Setup(x => x.UploadSessionId()).Returns("test");


            var request = TestFactory.CreateHttpRequestWithDataStream($"Data/{CommunicationManifest.FileName}");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);

            // call Azure Function
            var response = (OkObjectResult)await Start(request, CommunicationManifest.FileName,
                uploadDir,
                uploadTmpTable,
                new IdGenerator(), // idGenerator.Object,
                new App(),
                _logger);

            Assert.NotNull(response.Value);

            _output.WriteLine(response.Value.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void UploadContinueTest()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 1.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + "/test");

            // call Azure Function
            var response = (OkObjectResult)await Continue(request,
                "test",
                $"{ DateTime.Now:u} Sample 1.pdf",
                uploadDir,
                new UploadSession { SessionId = "test" },
                _logger);

            Assert.NotNull(response.Value);

            _output.WriteLine(response.Value.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void UploadEndTest()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 2.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + "/test");
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);
            var queue = _app.Repository.GetQueue(QueueNames.CommProcess);

            var tmpUploadEntity = new TableEntityAdapter<UploadSession>(
                new UploadSession { SessionId = "test" }, "test", new IdGenerator().UploadSessionId());

            // call Azure Function
            var response = (OkObjectResult)await End(request,
                "test",
                $"{ DateTime.Now:u} Sample 2.pdf",
                uploadDir,
                uploadTmpTable,
                tmpUploadEntity,
                queue,
                new App(),
                _logger);

            Assert.NotNull(response.Value);

            _output.WriteLine(response.Value.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void UploadCompletedTest()
        {
            var startRequest = TestFactory.CreateHttpRequestWithDataStream($"Data/{CommunicationManifest.FileName}");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);
            var queue = _app.Repository.GetQueue(QueueNames.CommProcess);

            // call upload start
            var startResponse = (OkObjectResult)await Start(startRequest, CommunicationManifest.FileName,
                uploadDir,
                uploadTmpTable,
                new IdGenerator(),
                new App(),
                _logger);

            var uploadSession = startResponse.Value.IsOrMap<UploadSession>();
            var sessionId = uploadSession.SessionId;

            var endRequest = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 2.pdf");
            var manifestDirectory = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + $"/{sessionId}");

            var tmpEntity = new TableEntityAdapter<UploadSession>(uploadSession, null, sessionId);

            // call upload end
            var endResponse = (OkObjectResult)await End(endRequest,
                sessionId,
                $"{ DateTime.Now:u} Sample 2.pdf",
                manifestDirectory,
                uploadTmpTable,
                tmpEntity,
                queue,
                new App(),
                _logger);

            Assert.NotNull(endResponse.Value);

            _output.WriteLine(endResponse.Value.ToJson(Formatting.Indented));
        }

        /*

        [Theory]
        [MemberData(nameof(TestFactory.Data), MemberType = typeof(TestFactory))]
        public async void Http_trigger_should_return_known_string_from_member_data(string queryStringKey, string queryStringValue)
        {
            var request = TestFactory.CreateHttpRequest(queryStringKey, queryStringValue);
            var response = (OkObjectResult)await TransferStart.Run(request, _logger);
            Assert.Equal($"Hello, {queryStringValue}", response.Value);
        }

        */
    }
}