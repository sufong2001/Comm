using Microsoft.WindowsAzure.Storage.Blob;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Share.Assembly;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.IO;
using Sufong2001.Share.Json;
using System.Linq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Sufong2001.Comm.Tests
{
    public class CommonTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ITestOutputHelper _output = null;
        private readonly ApplicationBaseFixture _app = null;

        public CommonTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
        }

        [Fact]
        public async void RepositoryCreateStorageIfNotExistsTest()
        {
            var results = await _app.Repository.CreateStorageIfNotExists();

            CloudBlockBlob uploadTo = null;

            if (results.Any(r => r))
            {
                var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);

                uploadTo = await $"Data/{CommunicationManifest.FileName}".CreateStream()
                    .UploadTo(uploadDir, $"test/{CommunicationManifest.FileName}");
            }

            _output.WriteLine(new
            {
                results,
                uploadTo?.Name
            }.ToJson(Formatting.Indented));
        }

        [Fact]
        public void GetCommunicationManifestJsonTest()
        {
            var cm = new CommunicationManifest()
            {
                Reference = "00001",
                Title = "Test Title",
                Sms = new Sms()
                {
                    Mobile = "0430 121 212",

                    SmsContent = "Sms Txt",
                },
                Email = new Email()
                {
                    EmailSubject = "EmailSubject",
                    EmailAddress = "EmailAddress",
                    EmailContent = "EmailContent",
                },

                Postage = new Postage()
                {
                    FirstName = "FirstName",
                    LastName = "LastName",
                    Company = "Company",
                    Address = new Address()
                    {
                        Line1 = "Line1",
                        Line2 = "Line2",
                        Line3 = "Line3",
                        Suburb = "Melbourne",
                        State = "VIC",
                        Postcode = "3000",
                        Country = "AU"
                    },
                    Attachments = new[]
                    {
                        "file1-1.pdf" ,
                    },
                },

                Attachments = new[]
                {
                    "file1.pdf" ,
                    "file2.pdf" ,
                    "file3.pdf" ,
                    "file5.pdf" ,
                },
            };

            var manifestKey = "test";
            var entities = cm.PrepareCommMessage(manifestKey);

            _output.WriteLine(cm.ToJson(Formatting.Indented));
        }

        [Fact]
        public void GetStaticFieldInfosTest()
        {
            var values = typeof(TableNames).GetStaticFieldInfos()
                .Select(f => f.GetValue(null).ToString())
                .ToArray();

            _output.WriteLine(values.ToJson(Formatting.Indented));
        }
    }
}