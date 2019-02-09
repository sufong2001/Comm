using Autofac;
using AzureFunctions.Autofac.Configuration;
using Sufong2001.Comm.Configurations.Modules;

namespace Sufong2001.Comm.Configurations.Resolvers
{
    public class DiConfig
    {
        public DiConfig(string functionName)
        {
            DependencyInjection.Initialize(
                cfg => { cfg.RegisterModule(new AppModule()); },
                functionName
            );
        }
    }
}