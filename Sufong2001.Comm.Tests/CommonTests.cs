using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Dto.Messages;
using Sufong2001.Comm.Tests.Base;
using Sufong2001.Share.Assembly;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.IO;
using Sufong2001.Share.Json;
using System.Collections.Generic;
using System.Linq;
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

            var uploadDir = _app.Repository.GetBlobDirectory(BlobNames.UploadDirectory);

            uploadTo = await $"Data/{CommunicationManifest.FileName}".CreateStream()
                .UploadTo(uploadDir, $"test/{CommunicationManifest.FileName}");

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
                CommunicationReference = "CommunicationReference01",
                Title = "Test Title",
                Recipients = new List<Recipient>()
                {
                    new Recipient()
                    {
                        RecipientReference = "RecipientReference01",

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
                            Attachments = new[]
                            {
                                "Email file1.pdf",
                            },
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
                                "Postage file1.pdf",
                            },
                        },

                        FailoverOptions = new []
                        {
                            "Email","Sms", "Postage"
                        },
                        Attachments = new[]
                        {
                            "Recipient file.pdf",
                        },
                    },
                    new Recipient()
                    {
                        RecipientReference = "RecipientReference02",

                        Sms = new Sms()
                        {
                            Mobile = "0430 000 212",

                            SmsContent = "Sms02 Txt",
                        },
                        Email = new Email()
                        {
                            EmailSubject = "EmailSubject02",
                            EmailAddress = "EmailAddress02",
                            EmailContent = "EmailContent02",
                            Attachments = new[]
                            {
                                "Email2 file1.pdf",
                            },
                        },

                        Attachments = new[]
                        {
                            "Recipient2 file.pdf",
                        },
                    }
                },

                FailoverOptions = new[]
                {
                    "Sms","Email", "Postage"
                },
                Attachments = new[]
                {
                    "Common file.pdf",
                }
            };

            _output.WriteLine(cm.ToJson(Formatting.Indented));
        }

        [Fact]
        public void PrepareCommMessageTest()
        {
            var cm = $"Data/{CommunicationManifest.FileName}".ReadTo<CommunicationManifest>();

            var manifestKey = "test";
            var entities = cm.PrepareCommMessage(manifestKey, _app.IdGenerator, _app.App.DateTimeNow);

            _output.WriteLine(entities.ToJson(Formatting.Indented));
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