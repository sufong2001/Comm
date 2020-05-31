using System;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.AzureStorage.Interfaces;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Configurations;
using Sufong2001.Comm.Configurations.Modules;

[assembly: FunctionsStartup(typeof(FunctionAppStartup))]

namespace Sufong2001.Comm.Configurations
{
    public class FunctionAppStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register MyServiceA as transient.
            // A new instance will be returned every
            // time a service request is made
            //builder.Services.AddTransient<MyServiceA>();

            // Register MyServiceB as scoped.
            // The same instance will be returned
            // within the scope of a function invocation
            //builder.Services.AddScoped<MyServiceB>();

            // Register ICommonIdProvider as scoped.
            // The same instance will be returned
            // within the scope of a function invocation
            //builder.Services.AddScoped<ICommonIdProvider, CommonIdProvider>();

            // Register IGlobalIdProvider as singleton.
            // A single instance will be created and reused
            // with every service request
            //builder.Services.AddSingleton<IGlobalIdProvider, CommonIdProvider>();

            builder.Services.AddAutofac(containerBuilder =>
                {

                   var v =  containerBuilder;
                }
            );

            ConfigureServices(builder.Services);

        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {

            // Create an Autofac Container and push the framework services
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);
            containerBuilder.RegisterModule(new CommonModule());

            // Register your own services within Autofac
            var storageAccount = services.BuildServiceProvider().GetService<StorageAccountProvider>().GetHost();
            var repository = new CommRepository(storageAccount);
            repository.CreateStorageIfNotExists().ConfigureAwait(false);

            containerBuilder.RegisterInstance(repository).As<ICommRepository>().As<IQueueRepository>().As<ITableRepository>().As<IBlobRepository>();
            containerBuilder.RegisterType<App>().SingleInstance().AsSelf();

            // Build the container and return an IServiceProvider from Autofac
            var container = containerBuilder.Build();
            var serviceProvider =  container.Resolve<IServiceProvider>();

            return serviceProvider;
        }
    }
}