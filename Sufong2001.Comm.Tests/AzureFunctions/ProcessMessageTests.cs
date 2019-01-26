using Microsoft.Extensions.Logging;
using Sufong2001.Comm.AzureFunctions.ServProcesses;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Share.IO;
using Sufong2001.Share.Json;
using Sufong2001.Test.AzureFunctions;
using Xunit;
using Xunit.Abstractions;

namespace Sufong2001.Comm.Tests.AzureFunctions
{
    public class ProcessMessageTests : IClassFixture<AppicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly AppicationBaseFixture _app;

        public ProcessMessageTests(ITestOutputHelper output, AppicationBaseFixture app)
        {
            this._output = output;
            _app = app;
        }

        [Fact]
        public async void ProcessManifestActivityTest()
        {
            var uploadCompleted = $"Data/UploadCompleted.json".ReadTo<UploadCompleted>();
            var uploadBlob = _app.Repository.GetBlockBlob(BlobNames.UploadDirectory + "/test/" + CommunicationManifest.FileName);
            var messageTable = _app.Repository.GetTable(TableNames.CommMessage);

            var id = new IdGenerator();

            uploadCompleted.SessionId = id.UploadSessionId();

            var communicationManifest = await CommProcessors.ProcessManifestActivity(
                uploadCompleted,
                uploadBlob,
                messageTable,
                id,
                _logger);

            Assert.NotNull(communicationManifest);

            _output.WriteLine(communicationManifest.ToJson());
        }
    }
}