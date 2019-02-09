using Autofac;
using AzureFunctions.Autofac.Configuration;
using Sufong2001.Comm.AzureStorage.Interfaces;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.Configurations.Modules
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var repository = DependencyInjection.Resolve(typeof(ICommRepository), null, AppStartup.Name,
                AppStartup.Guid) as ICommRepository;

            builder.RegisterInstance(repository).As<IQueueRepository>().As<ITableRepository>().As<IBlobRepository>();
            builder.RegisterType<IdGenerator>().SingleInstance().As<IUploadIdGenerator>().As<IMessageIdGenerator>().As<IScheduleIdGenerator>();
            builder.RegisterType<App>().SingleInstance().AsSelf();
            //builder.RegisterType<Goodbyer>().Named<IGoodbyer>("Main");
            //builder.RegisterType<AlternateGoodbyer>().Named<IGoodbyer>("Secondary");
        }
    }
}