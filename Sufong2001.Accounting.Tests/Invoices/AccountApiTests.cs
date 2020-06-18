using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Accounting.Xero;
using Sufong2001.Test.AzureFunctions;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sufong2001.Accounting.Tests.Invoices
{
    public class ConfigurationTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly ApplicationBaseFixture _app;
        private readonly TenantAccess _access;
        private readonly AccountingApi _accountingApi;

        public ConfigurationTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
            _access = _app.ServiceProvider.GetService<TenantAccess>();
            _accountingApi = new AccountingApi();
        }

        [Fact]
        public async void Should_AccountApi_Has_Invoices()
        {
            var (accessToken, xeroTenantId) = await _access.GetAccessToken();

            var sevenDaysAgo = DateTime.Now.AddDays(-30).ToString("yyyy, MM, dd");
            var invoicesFilter = "Date >= DateTime(" + sevenDaysAgo + ")";

            var response = await _accountingApi.GetInvoicesAsync(accessToken, xeroTenantId, null, invoicesFilter);
            var invoices = response._Invoices;
        }

        [Fact]
        public async void Should_AccountApi_Parallet_Test()
        {
            var (accessToken, xeroTenantId) = await _access.GetAccessToken();

            Parallel.For(0, 3, new ParallelOptions() { MaxDegreeOfParallelism = 2}, async i =>
                {
                    var sevenDaysAgo = DateTime.Now.AddDays(-30).ToString("yyyy, MM, dd");
                    var invoicesFilter = "Date >= DateTime(" + sevenDaysAgo + ")";

                    var response = await _accountingApi.GetInvoicesAsync(accessToken, xeroTenantId, null, invoicesFilter);
                    var invoices = response._Invoices;
                });
        }
    }
}