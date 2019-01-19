using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sufong2001.Comm.AzureFunctions.ServIns;
using Sufong2001.Test.AzureFunctions;
using Xunit;

namespace Sufong2001.Comm.Tests.AzureFunctions
{
    public class TransferTests
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        [Fact]
        public async void TransferStartTest()
        {
            var request = TestFactory.CreateHttpRequest("name", "Bill");
            var response = (OkObjectResult)await TransferStart.Run(request, _logger);
            Assert.Equal("Hello, Bill", response.Value);
        }

        [Fact]
        public async void TransferContinueTest()
        {
            var request = TestFactory.CreateHttpRequest("name", "Bill");
            var response = (OkObjectResult)await TransferContinue.Run(request, _logger);
            Assert.Equal("Hello, Bill", response.Value);
        }

        [Fact]
        public async void TransferEndTest()
        {
            var request = TestFactory.CreateHttpRequest("name", "Bill");
            var response = (OkObjectResult)await TransferEnd.Run(request, _logger);
            Assert.Equal("Hello, Bill", response.Value);
        }

        [Theory]
        [MemberData(nameof(TestFactory.Data), MemberType = typeof(TestFactory))]
        public async void Http_trigger_should_return_known_string_from_member_data(string queryStringKey, string queryStringValue)
        {
            var request = TestFactory.CreateHttpRequest(queryStringKey, queryStringValue);
            var response = (OkObjectResult)await TransferStart.Run(request, _logger);
            Assert.Equal($"Hello, {queryStringValue}", response.Value);
        }
    }
}