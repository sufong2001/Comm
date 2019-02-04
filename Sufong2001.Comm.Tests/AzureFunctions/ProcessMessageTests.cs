using Microsoft.Azure.WebJobs;
using Moq;
using Newtonsoft.Json;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Share.IO;
using Sufong2001.Share.Json;
using Sufong2001.Test.AzureFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static Sufong2001.Comm.AzureFunctions.ServProcesses.CommProcessors;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
        public async void Should_ProcessMessageTest_Created_2_MessageSchedules()
        {
            var id = _app.IdGenerator;
            var app = _app.App;
            var uploadCompleted = $"Data/UploadCompleted.json".ReadTo<UploadCompleted>();

            uploadCompleted.SessionId = id.UploadSessionId();
            var uploadBlob = _app.Repository.GetBlockBlob(BlobNames.UploadDirectory + "/test/" + CommunicationManifest.FileName);
            var messageTable = _app.Repository.GetTable(TableNames.CommMessage);
            var scheduleTable = _app.Repository.GetTable(TableNames.CommSchedule);

            var durableOrchestrationContextBase = new Mock<DurableOrchestrationContextBase>();
            durableOrchestrationContextBase
                .Setup(x => x.GetInput<UploadCompleted>()).Returns(uploadCompleted);

            // ProcessManifest
            durableOrchestrationContextBase
                .Setup(x => x.CallActivityAsync<IEnumerable<Message>>(ActivityNames.ProcessManifest, It.IsAny<UploadCompleted>()))
                .Returns(ProcessManifestActivity(uploadCompleted, uploadBlob, messageTable, id, app, _logger));

            // CreateSchedule
            durableOrchestrationContextBase
                .Setup(x => x.CallActivityAsync<IEnumerable<MessageSchedule>>(ActivityNames.CreateSchedule, It.IsAny<IEnumerable<Message>>()))
                .Returns((string name, IEnumerable<Message> messages) => CreateScheduleActivity(messages, scheduleTable, id, app, _logger));

            // call Orchestrator function
            var result = await ProcessMessageOrchestrator(durableOrchestrationContextBase.Object, _logger);

            durableOrchestrationContextBase.Verify();

            Assert.Equal(2, result.Count());
            Assert.True(result.All(r => r.DeliverySchedule.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date));

            _output.WriteLine(result.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_ProcessManifestActivity_Created_5_Messages()
        {
            var uploadCompleted = $"Data/UploadCompleted.json".ReadTo<UploadCompleted>();
            var uploadBlob = _app.Repository.GetBlockBlob(BlobNames.UploadDirectory + "/test/" + CommunicationManifest.FileName);
            var messageTable = _app.Repository.GetTable(TableNames.CommMessage);

            var id = _app.IdGenerator;
            var app = _app.App;

            uploadCompleted.SessionId = id.UploadSessionId();

            var result = await ProcessManifestActivity(
                uploadCompleted,
                uploadBlob,
                messageTable,
                id,
                app,
                _logger);

            Assert.Equal(5, result.Count());

            _output.WriteLine(result.ToJson(Formatting.Indented));
        }

        [Fact]
        public async void Should_CreateScheduleActivity_Created_2_Schedules()
        {
            var messages = $"Data/CommMessages.json".ReadTo<Message[]>();
            var scheduleTable = _app.Repository.GetTable(TableNames.CommSchedule);

            var id = _app.IdGenerator;
            var app = _app.App;

            var result = await CreateScheduleActivity(
                messages,
                scheduleTable,
                id,
                app,
                _logger);

            Assert.Equal(2, result.Count());
            //Assert.True(result.All(r => r.DeliverySchedule.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date));

            _output.WriteLine(result.ToJson(Formatting.Indented));
        }
    }
}