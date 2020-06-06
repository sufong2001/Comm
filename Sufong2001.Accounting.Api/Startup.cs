using Autofac;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using AutoMapper;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sufong2001.Accounting.Api;
using Sufong2001.Accounting.Api.Functions;
using Sufong2001.Accounting.Api.Storage;
using Sufong2001.Accounting.Xero;
using Sufong2001.Accounting.Xero.Webhooks;
using Sufong2001.Accounting.Xero.Webhooks.Config;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using ContainerBuilder = Autofac.ContainerBuilder;

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
            ConfigureServices(builder.Services);

            builder
                .UseAppSettings() // this is optional, this will bind IConfiguration in the container.
                .UseAutofacServiceProviderFactory(ConfigureAutofacContainer);
        }

        /// <summary>
        /// DI Configure using Autofac container
        /// </summary>
        /// <param name="builder"></param>
        private void ConfigureAutofacContainer(ContainerBuilder builder)
        {
            builder.Register(activator =>
                {
                    var xeroConfig = new XeroConfiguration();

                    var config = activator.Resolve<IConfiguration>();
                    config.GetSection(nameof(XeroConfiguration)).Bind(xeroConfig);

                    return xeroConfig;
                })
                .AsSelf()
                .SingleInstance();

            // register azure storage account
            builder.Register(activator =>
                {
                    var storageAccount = activator.Resolve<StorageAccountProvider>().GetHost();

                    return storageAccount;
                })
                .AsSelf()
                .SingleInstance();

            // register Cosmos DB
            builder.Register(activator =>
                {
                    var config = activator.Resolve<IConfiguration>();
                    var connectionString = config["CosmosDB:ConnectionString"];
                    var cosmosClientBuilder = new CosmosClientBuilder(connectionString);

                    return cosmosClientBuilder.Build();
                })
                .AsSelf()
                .SingleInstance();

            // Register all functions that resides in a given namespace
            // The function class itself will be created using autofac
            builder
                .RegisterAssemblyTypes(typeof(Startup).Assembly)
                .InNamespace(typeof(IAzFunc).Namespace ?? string.Empty)
                .AsSelf() // Azure Functions core code resolves a function class by itself.
                .InstancePerTriggerRequest(); // This will scope nested dependencies to each function execution

            builder
                .RegisterAssemblyTypes(typeof(Startup).Assembly)
                .InNamespace(typeof(IStorage).Namespace ?? string.Empty)
                .AsImplementedInterfaces()
                .InstancePerTriggerRequest(); // This will scope nested dependencies to each function execution
        }

        /// <summary>
        /// DI Configure using Azure.Functions.Extensions.DependencyInjection
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddLogging();
            services.AddAutoMapper(typeof(Startup).Assembly);

            //services.AddOptions<XeroConfiguration>()
            //    .Configure<IConfiguration>((settings, configuration) =>
            //    {
            //        configuration.GetSection("XeroConfiguration").Bind(settings);
            //    });

            services.AddOptions<WebhookSettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("XeroConfiguration:WebhookSettings").Bind(settings);
                });

            services.AddSingleton<ISignatureVerifier>((s) =>
            {
                var settings = s.GetService<IOptions<WebhookSettings>>().Value;
                return new SignatureVerifier(settings);
            });

            services.AddSingleton<XeroClient>();
            //services.AddSingleton<TokenStorage>();
            services.AddScoped<TenantAccess>();
        }
    }
}