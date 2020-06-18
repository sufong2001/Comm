using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Accounting.Api.Functions.Authorization.Token;
using Sufong2001.Accounting.Api.Functions.Webhooks.Events;
using Sufong2001.Accounting.Api.Functions.Webhooks.Names;
using Sufong2001.Accounting.Api.Storage;
using Sufong2001.Accounting.Xero;
using Sufong2001.Accounting.Xero.Webhooks.Models;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using Sufong2001.Test.AzureFunctions;
using Xero.NetStandard.OAuth2.Api;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sufong2001.Accounting.Tests.Webhook
{
    public class WebhookTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly ApplicationBaseFixture _app;
        private readonly TenantAccess _access;
        private readonly AccountingApi _accountingApi;
        private readonly StorageRepository _storageRepository;

        public WebhookTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
            _access = _app.ServiceProvider.GetService<TenantAccess>();
            _accountingApi = new AccountingApi();
            _storageRepository = _app.ServiceProvider.GetService<StorageRepository>();
        }

        [Fact]
        public async void Should_WebhookPayload_Has_StoreCorrectly()
        {
            var storage = _app.ServiceProvider.GetService<PayloadTable>();
            await storage.Store(new Payload(){ Entropy = "TESTRANDOM"}.ToJson());
        }
    }
}