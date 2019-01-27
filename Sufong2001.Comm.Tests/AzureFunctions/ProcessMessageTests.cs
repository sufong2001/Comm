using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Share.IO;
using Sufong2001.Share.Json;
using Sufong2001.Test.AzureFunctions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using static Sufong2001.Comm.AzureFunctions.ServProcesses.CommProcessors;

namespace Sufong2001.Comm.Tests.AzureFunctions
{
    public class ProcessMessageTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly ApplicationBaseFixture _app;

        public ProcessMessageTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
        }

        [Fact]
        public async void ProcessMessageTest()
        {
            var id = new IdGenerator();
            var uploadCompleted = $"Data/UploadCompleted.json".ReadTo<UploadCompleted>();

            uploadCompleted.SessionId = id.UploadSessionId();
            var uploadBlob = _app.Repository.GetBlockBlob(BlobNames.UploadDirectory + "/test/" + CommunicationManifest.FileName);
            var messageTable = _app.Repository.GetTable(TableNames.CommMessage);

            var durableOrchestrationContextBase = new Mock<DurableOrchestrationContextBase>();
            durableOrchestrationContextBase
                .Setup(x => x.GetInput<UploadCompleted>()).Returns(uploadCompleted);

            durableOrchestrationContextBase
                .Setup(x => x.CallActivityAsync<IList<TableResult>>(ActivityNames.ProcessManifest, It.IsAny<UploadCompleted>()))
                .Returns(ProcessManifestActivity(uploadCompleted, uploadBlob, messageTable, id, _logger));

            // call Orchestrator function
            var result = await ProcessMessageOrchestrator(durableOrchestrationContextBase.Object, _logger);

            Assert.NotNull(result);

            _output.WriteLine(result.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void ProcessManifestActivityTest()
        {
            var uploadCompleted = $"Data/UploadCompleted.json".ReadTo<UploadCompleted>();
            var uploadBlob = _app.Repository.GetBlockBlob(BlobNames.UploadDirectory + "/test/" + CommunicationManifest.FileName);
            var messageTable = _app.Repository.GetTable(TableNames.CommMessage);

            var id = new IdGenerator();

            uploadCompleted.SessionId = id.UploadSessionId();

            var communicationManifest = await ProcessManifestActivity(
                uploadCompleted,
                uploadBlob,
                messageTable,
                id,
                _logger);

            Assert.NotNull(communicationManifest);

            _output.WriteLine(communicationManifest.ToJson(Formatting.Indented));
        }
    }
}