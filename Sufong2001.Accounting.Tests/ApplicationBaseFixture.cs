using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Accounting.Api;
using Sufong2001.Accounting.Api.Storage;
using Sufong2001.Accounting.Xero;
using Sufong2001.Accounting.Xero.Storage;
using System.IO;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;

namespace Sufong2001.Accounting.Tests
{
    public class ApplicationBaseFixture
    {
        public IConfiguration Configuration { get; }
        public ServiceProvider ServiceProvider { get; }

        public ApplicationBaseFixture()
        {
            // build configuration
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                ;//.AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();

            // build service provider
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(provider =>
            {
                var xeroConfig = new XeroConfiguration();
                Configuration.GetSection("Values:" + nameof(XeroConfiguration)).Bind(xeroConfig);
                return xeroConfig;
            });
            services.AddSingleton(provider => StorageAccount.NewFromConnectionString(Configuration["Values:AzureWebJobsStorage"]));
            services.AddSingleton<StorageRepository>();

            services.AddSingleton<TokenStorage>();

            services.AddSingleton<TenantAccess>();

            new Startup().ConfigureServices(services);
        }
    }
}