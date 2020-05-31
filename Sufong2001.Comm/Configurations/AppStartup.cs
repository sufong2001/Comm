using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureFunctions.Autofac.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.Configurations;
using System;
using Sufong2001.Comm.AzureStorage.Interfaces;

[assembly: WebJobsStartup(typeof(AppStartup))]

namespace Sufong2001.Comm.Configurations
{
    public class AppStartup : IWebJobsStartup
    {
        public static Guid Guid = new Guid();
        public static string Name = nameof(AppStartup);

        public void Configure(IWebJobsBuilder builder)
        {
            // Populates the Autofac container builder with the set of registered service descriptors
            DependencyInjection.Initialize(
                cfg =>
                {
                    cfg.Populate(builder.Services);

                    var serviceProvider = builder.Services.BuildServiceProvider();
                    var storageAccount = serviceProvider.GetService<StorageAccountProvider>().GetHost();
                    var repository = new CommRepository(storageAccount);
                    repository.CreateStorageIfNotExists().ConfigureAwait(false);

                    cfg.Register(context => repository);
                },
                Name
            );
        }
    }
}