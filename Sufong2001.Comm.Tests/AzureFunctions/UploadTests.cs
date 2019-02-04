using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Share.Json;
using Sufong2001.Test.AzureFunctions;
using System;
using Sufong2001.Comm.Models.Storage.Partitions;
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
        public async void Should_UploadStart_Success_200_Manifest()
        {
            //var idGenerator = new Mock<IUploadIdGenerator>();
            //idGenerator.Setup(x => x.UploadSessionId()).Returns("test");

            var request = TestFactory.CreateHttpRequestWithDataStream($"Data/{CommunicationManifest.FileName}");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);

            // call Azure Function
            var response = (ObjectResult)await Start(request, CommunicationManifest.FileName,
                uploadDir,
                uploadTmpTable,
                new IdGenerator(), // idGenerator.Object,
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.NotEmpty(upload.ManifestFile);
            Assert.Contains(CommunicationManifest.FileName, upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadStart_Success_200_Pdf()
        {
            //var idGenerator = new Mock<IUploadIdGenerator>();
            //idGenerator.Setup(x => x.UploadSessionId()).Returns("test");

            var request = TestFactory.CreateHttpRequestWithDataStream($"Data/Sample 1.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);

            // call Azure Function
            var response = (ObjectResult)await Start(request, "Sample 1.pdf",
                uploadDir,
                uploadTmpTable,
                new IdGenerator(), // idGenerator.Object,
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.Contains("Sample 1.pdf", upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadStart_Fail_400_With_No_Data()
        {
            var request = TestFactory.CreateHttpRequest("none", "nil");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);

            // call Azure Function
            var response = (ObjectResult)await Start(request, CommunicationManifest.FileName,
                uploadDir,
                uploadTmpTable,
                new IdGenerator(), // idGenerator.Object,
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.Null(upload.LastUploadedFile);
            Assert.Equal("No file data has been uploaded\r\nParameter name: stream", upload.Errors);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadStart_Fail_400_With_No_Filename()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream($"Data/Sample 1.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);

            // call Azure Function
            var response = (ObjectResult)await Start(request, null,
                uploadDir,
                uploadTmpTable,
                new IdGenerator(), // idGenerator.Object,
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.Null(upload.LastUploadedFile);
            Assert.Equal("A filename is required.\r\nParameter name: filename", upload.Errors);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadContinue_Success_200_Pdf()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 1.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + "/test");
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);

            var tmpUploadEntity = new TableEntityAdapter<UploadSession>(
                new UploadSession { SessionId = "test" }, "test", new IdGenerator().UploadSessionId());

            // call Azure Function
            var response = (ObjectResult)await Continue(request,
                "test",
                $"{DateTime.Now:u} Sample 1.pdf",
                uploadDir,
                uploadTmpTable,
                tmpUploadEntity,
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.Contains("test", upload.SessionId);
            Assert.Contains("Sample 1.pdf", upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadContinue_Fail_400_With_No_Data()
        {
            var request = TestFactory.CreateHttpRequest("none", "nil");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + "/test");
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);

            var tmpUploadEntity = new TableEntityAdapter<UploadSession>(
                new UploadSession { SessionId = "test" }, "test", new IdGenerator().UploadSessionId());

            // call Azure Function
            var response = (ObjectResult)await Continue(request,
                "test",
                $"{DateTime.Now:u} Sample 1.pdf",
                uploadDir,
                uploadTmpTable,
                tmpUploadEntity,
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.Null(upload.LastUploadedFile);
            Assert.Equal("No file data has been uploaded\r\nParameter name: stream", upload.Errors);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadContinue_Fail_400_With_No_Filename()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream($"Data/Sample 1.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + "/test");
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);

            var tmpUploadEntity = new TableEntityAdapter<UploadSession>(
                new UploadSession { SessionId = "test" }, "test", new IdGenerator().UploadSessionId());

            // call Azure Function
            var response = (ObjectResult)await Continue(request,
                "test",
                null,
                uploadDir,
                uploadTmpTable,
                tmpUploadEntity,
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.Null(upload.LastUploadedFile);
            Assert.Equal("A filename is required.\r\nParameter name: filename", upload.Errors);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadEnd_Success_200_Pdf()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 2.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + "/test");
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);
            var queue = _app.Repository.GetQueue(QueueNames.CommProcess);

            var tmpUploadEntity = new TableEntityAdapter<UploadSession>(
                new UploadSession { SessionId = "test" }, "test", new IdGenerator().UploadSessionId());

            // call Azure Function
            var response = (ObjectResult)await End(request,
                "test",
                $"{DateTime.Now:u} Sample 2.pdf",
                uploadDir,
                uploadTmpTable,
                tmpUploadEntity,
                queue,
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.NotNull(upload.UploadEnd);
            Assert.Contains("Sample 2.pdf", upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadEnd_Success_200_Pdf_With_No_Filename()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 2.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + "/test");
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);
            var queue = _app.Repository.GetQueue(QueueNames.CommProcess);

            var tmpUploadEntity = new TableEntityAdapter<UploadSession>(
                new UploadSession { SessionId = "test" }, "test", new IdGenerator().UploadSessionId());

            // call Azure Function
            var response = (ObjectResult)await End(request,
                "test",
                null,
                uploadDir,
                uploadTmpTable,
                tmpUploadEntity,
                queue,
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.NotNull(upload.UploadEnd);
            Assert.Null(upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_UploadEnd_Fail_400_With_No_Data()
        {
            var request = TestFactory.CreateHttpRequest("none", "nil");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + "/test");
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);
            var queue = _app.Repository.GetQueue(QueueNames.CommProcess);

            var tmpUploadEntity = new TableEntityAdapter<UploadSession>(
                new UploadSession { SessionId = "test" }, "test", new IdGenerator().UploadSessionId());

            // call Azure Function
            var response = (ObjectResult)await End(request,
                "test",
                $"{DateTime.Now:u} Sample 2.pdf",
                uploadDir,
                uploadTmpTable,
                tmpUploadEntity,
                queue,
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.Null(upload.LastUploadedFile);
            Assert.Null(upload.UploadEnd);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_Upload_Completed_With_Manifest_First()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream($"Data/{CommunicationManifest.FileName}");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);
            var queue = _app.Repository.GetQueue(QueueNames.CommProcess);

            // call step1 upload start
            var response = (ObjectResult)await Start(request, CommunicationManifest.FileName,
                uploadDir,
                uploadTmpTable,
                new IdGenerator(),
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.NotNull(upload.ManifestFile);
            Assert.NotNull(upload.UploadStart);
            Assert.Null(upload.UploadEnd);
            Assert.Contains(CommunicationManifest.FileName, upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));

            // call step2 upload continue
            var sessionId = upload.SessionId;
            var sessionBlobDirectory = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + $"/{sessionId}");

            var tmpEntity = new TableEntityAdapter<UploadSession>(upload, CommUploadPartitionKeys.Temp, sessionId);
            request = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 1.pdf");

            response = (ObjectResult)await Continue(request,
                sessionId,
                $"{DateTime.Now:u} Sample 1.pdf",
                sessionBlobDirectory,
                uploadTmpTable,
                tmpEntity,
                _logger);

            upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.NotNull(upload.ManifestFile);
            Assert.NotNull(upload.UploadStart);
            Assert.Null(upload.UploadEnd);
            Assert.Contains("Sample 1.pdf", upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));

            // call step3 upload end
            tmpEntity = new TableEntityAdapter<UploadSession>(upload, CommUploadPartitionKeys.Temp, sessionId);
            request = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 2.pdf");

            response = (ObjectResult)await End(request,
                sessionId,
                $"{DateTime.Now:u} Sample 2.pdf",
                sessionBlobDirectory,
                uploadTmpTable,
                tmpEntity,
                queue,
                new App(),
                _logger);

            upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.NotNull(upload.ManifestFile);
            Assert.NotNull(upload.UploadStart);
            Assert.NotNull(upload.UploadEnd);
            Assert.Contains("Sample 2.pdf", upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_Upload_Completed_With_Manifest_Last()
        {
            var request = TestFactory.CreateHttpRequestWithDataStream($"Data/Sample 2.pdf");
            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);
            var uploadTmpTable = _app.Repository.GetTable(TableNames.CommUpload);
            var queue = _app.Repository.GetQueue(QueueNames.CommProcess);

            // call step1 upload start
            var response = (ObjectResult)await Start(request,
                $"{DateTime.Now:u} Sample 2.pdf",
                uploadDir,
                uploadTmpTable,
                new IdGenerator(),
                new App(),
                _logger);

            var upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.NotNull(upload.UploadStart);
            Assert.Null(upload.UploadEnd);
            Assert.Contains("Sample 2.pdf", upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));

            // call step2 upload continue
            var sessionId = upload.SessionId;
            var sessionBlobDirectory = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory + $"/{sessionId}");

            var tmpEntity = new TableEntityAdapter<UploadSession>(upload, CommUploadPartitionKeys.Temp, sessionId);
            request = TestFactory.CreateHttpRequestWithDataStream("Data/Sample 1.pdf");

            response = (ObjectResult)await Continue(request,
                sessionId,
                $"{DateTime.Now:u} Sample 1.pdf",
                sessionBlobDirectory,
                uploadTmpTable,
                tmpEntity,
                _logger);

            upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Null(upload.ManifestFile);
            Assert.NotNull(upload.UploadStart);
            Assert.Null(upload.UploadEnd);
            Assert.Contains("Sample 1.pdf", upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));

            // call step3 upload end
            tmpEntity = new TableEntityAdapter<UploadSession>(upload, CommUploadPartitionKeys.Temp, sessionId);
            request = TestFactory.CreateHttpRequestWithDataStream($"Data/{CommunicationManifest.FileName}");

            response = (ObjectResult)await End(request,
                sessionId,
                CommunicationManifest.FileName,
                sessionBlobDirectory,
                uploadTmpTable,
                tmpEntity,
                queue,
                new App(),
                _logger);

            upload = response.Value.IsOrMap<UploadSession>();

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.NotNull(upload.ManifestFile);
            Assert.NotNull(upload.UploadStart);
            Assert.NotNull(upload.UploadEnd);
            Assert.Contains(CommunicationManifest.FileName, upload.LastUploadedFile);

            _output.WriteLine(upload.ToJson(Formatting.Indented));
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