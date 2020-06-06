using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Accounting.Api.Storage;
using Sufong2001.Accounting.Xero;
using Sufong2001.Accounting.Xero.Storage;
using Sufong2001.Accounting.Xero.Webhooks.Config;
using System;
using System.IO;
using Sufong2001.Accounting.Api.Storage.Token;
using Sufong2001.Core.Storage.Interfaces;
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
                .AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();

            // build service provider
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddSingleton(GetXeroConfiguration);
            services.AddSingleton(GetwebhookSettings);
            services.AddSingleton(provider => StorageAccount.NewFromConnectionString(Configuration["Values:AzureWebJobsStorage"]));
            services.AddSingleton(GetCosmosClient);
            services.AddSingleton<StorageRepository>();
            services.AddSingleton<ITableRepository, StorageRepository>();
            services.AddSingleton<ICosmosDbRepository, StorageRepository>();

            services.AddSingleton<TokenStorage>();
            services.AddSingleton<TokenContainer>();
            services.AddSingleton<TokenTable>();
            services.AddSingleton<XeroClient>();

            services.AddSingleton<TenantAccess>();
        }

        private CosmosClient GetCosmosClient(IServiceProvider provider)
        {
            var connectionString = Configuration["Values:CosmosDB:ConnectionString"];
            var cosmosClientBuilder = new CosmosClientBuilder(connectionString);

            return cosmosClientBuilder.Build();
        }

        private XeroConfiguration GetXeroConfiguration(IServiceProvider provider)
        {
            var xeroConfig = new XeroConfiguration();
            Configuration.GetSection("Values:" + nameof(XeroConfiguration)).Bind(xeroConfig);
            return xeroConfig;
        }

        private WebhookSettings GetwebhookSettings(IServiceProvider provider)
        {
            var webhookSettings = new WebhookSettings();
            Configuration.GetSection($"Values:{nameof(XeroConfiguration)}:{nameof(WebhookSettings)}").Bind(webhookSettings);
            return webhookSettings;
        }
    }
}