using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sufong2001.Accounting.Api;
using Sufong2001.Accounting.Xero.Webhooks;
using Sufong2001.Accounting.Xero.Webhooks.Config;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Sufong2001.Accounting.Api
{
    /// <summary>
    /// This is the native azure function dependency injection implementation example
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddLogging();

            builder.Services.AddOptions<WebhookSettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("Xero:WebhookSettings").Bind(settings);
                });

            builder.Services.AddSingleton<ISignatureVerifier>((s) =>
                {
                    var settings = s.GetService<IOptions<WebhookSettings>>().Value;
                    return new SignatureVerifier(settings);
                }
            );
        }
    }
}