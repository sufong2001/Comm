using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Models.Storage.Partitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sufong2001.Share.AzureStorage;

namespace Sufong2001.Comm.AzureFunctions.ServOuts
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class CommDelivery
    {
    }
}