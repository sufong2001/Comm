using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Accounting.Api.Storage.Token;
using Sufong2001.Accounting.Xero.Storage;
using Sufong2001.Accounting.Xero.Webhooks.Config;
using Sufong2001.Test.AzureFunctions;
using Xero.NetStandard.OAuth2.Config;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sufong2001.Accounting.Tests.Token
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
        public void Should_Token_Has_StoreCorrectly()
        {
            var tokenStorage = _app.ServiceProvider.GetService<TokenTable>();
            var tokenContainer = _app.ServiceProvider.GetService<TokenContainer>();

            tokenContainer.StoreToken(tokenStorage.GetStoredToken());

        }

        [Fact]
        public void Should_Token_Has_ReadCorrectly()
        {

            var tokenContainer = _app.ServiceProvider.GetService<TokenContainer>();

            var token = tokenContainer.GetStoredToken();

        }

    }
}