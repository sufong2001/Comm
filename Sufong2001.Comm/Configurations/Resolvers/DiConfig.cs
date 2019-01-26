using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using AzureFunctions.Autofac.Configuration;
using Sufong2001.Comm.Configurations.Modules;

namespace Sufong2001.Comm.Configurations.Resolvers
{
    public class DiConfig
    {
        public DiConfig(string functionName)
        {
            DependencyInjection.Initialize(builder =>
            {
                builder.RegisterModule(new UploadModule());
            }, functionName);
        }
    }
}
