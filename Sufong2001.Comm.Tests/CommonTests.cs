using Microsoft.Extensions.Logging;
using Sufong2001.Comm.AzureFunctions.ServProcesses;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.Dto;
using Sufong2001.Share.Json;
using Sufong2001.Test.AzureFunctions;
using Xunit;
using Xunit.Abstractions;

namespace Sufong2001.Comm.Tests
{
    public class CommonTests
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper output;

        public CommonTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void GetCommunicationManifestJson()
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

            output.WriteLine(cm.ToJson());
        }
    }
}