using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sufong2001.Accounting.Xero;
using Sufong2001.Accounting.Xero.Webhooks.Config;
using Sufong2001.Test.AzureFunctions;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sufong2001.Accounting.Tests.Configuration
{
    public class TokenTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly ApplicationBaseFixture _app;

        public TokenTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
        }

        [Fact]
        public void Should_Webhooks_Has_SigningKey()
        {
            var webhookSettings = _app.ServiceProvider.GetService<WebhookSettings>();

            Assert.Equal("kr0JWjsaOPYV0MgZgBA0eckWmSaiyvTEsl+sOvW+XJEj/Onmb7vOgWJ9iLAkBTQWr2/E0BFDjRyc8lhPftyP4g==", webhookSettings.SigningKey);
        }

        [Fact]
        public void Should_XeroConfiguration_Has_ClientId()
        {
            var config = _app.ServiceProvider.GetService<XeroConfiguration>();

            Assert.Equal("D1937706FCCD4021BEE9BA4F3122E2D7", config.ClientId);
        }

        [Fact]
        public void Should_LoadConfiguration_Has_Success()
        {
            var cosmosClient = _app.ServiceProvider.GetService<CosmosClient>();

            Assert.True(cosmosClient.ClientOptions.Serializer != null);
            Assert.Equal("localhost:8081", cosmosClient.Endpoint.Authority);
        }
    }
}