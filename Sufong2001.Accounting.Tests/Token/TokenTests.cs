using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Accounting.Api.Functions.Authorization.Token;
using Sufong2001.Test.AzureFunctions;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sufong2001.Accounting.Tests.Token
{
    public class WebhookTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly ApplicationBaseFixture _app;

        public WebhookTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
        }

        [Fact]
        public async void Should_Token_Has_StoreCorrectly()
        {
            var tokenStorage = _app.ServiceProvider.GetService<TokenTable>();
            var token = await tokenStorage.GetStoredToken();

            var tokenContainer = _app.ServiceProvider.GetService<TokenContainer>();

            await tokenContainer.StoreToken(token);
        }

        [Fact]
        public async void Should_Token_Has_ReadCorrectly()
        {
            var tokenContainer = _app.ServiceProvider.GetService<TokenContainer>();

            var token = await tokenContainer.GetStoredToken();
        }
    }
}