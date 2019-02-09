using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureFunctions.Autofac.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.Configurations;
using System;

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

                    var storageAccount = builder.Services.BuildServiceProvider().GetService<StorageAccountProvider>().GetHost();
                    var repository = new CommRepository(storageAccount);
                    cfg.RegisterInstance(repository);
                },
                Name
            );
        }
    }
}