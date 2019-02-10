using Autofac;
using AzureFunctions.Autofac.Configuration;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.AzureStorage.Interfaces;
using Sufong2001.Comm.BusinessEntities;

namespace Sufong2001.Comm.Configurations.Resolvers
{
    public abstract class BaseConfig
    {
        protected BaseConfig(string functionName)
        {
            DependencyInjection.Initialize(
                cfg =>
                {
                    var repository = DependencyInjection.Resolve(typeof(CommRepository), null, AppStartup.Name, AppStartup.Guid) as CommRepository;

                    cfg.RegisterInstance(repository).As<ICommRepository>().As<IQueueRepository>().As<ITableRepository>().As<IBlobRepository>();
                    cfg.RegisterType<App>().SingleInstance().AsSelf();

                    Configuration(cfg);
                },
                functionName
            );
        }

        public abstract void Configuration(ContainerBuilder cfg);
    }
}