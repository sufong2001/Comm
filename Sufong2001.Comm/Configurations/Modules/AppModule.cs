using Autofac;
using AzureFunctions.Autofac.Configuration;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.AzureStorage.Interfaces;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.Configurations.Modules
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var repository = DependencyInjection.Resolve(typeof(CommRepository), null, AppStartup.Name,
                AppStartup.Guid) as CommRepository;

            builder.RegisterInstance(repository).As<ICommRepository>().As<IQueueRepository>().As<ITableRepository>().As<IBlobRepository>();
            builder.RegisterType<IdGenerator>().SingleInstance().As<IUploadIdGenerator>().As<IMessageIdGenerator>().As<IScheduleIdGenerator>();
            builder.RegisterType<App>().SingleInstance().AsSelf();
            //builder.RegisterType<Goodbyer>().Named<IGoodbyer>("Main");
            //builder.RegisterType<AlternateGoodbyer>().Named<IGoodbyer>("Secondary");
        }
    }
}