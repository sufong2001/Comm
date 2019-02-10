using Autofac;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.Configurations.Modules
{
    public class CommonModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<IdGenerator>().SingleInstance().As<IUploadIdGenerator>().As<IMessageIdGenerator>().As<IScheduleIdGenerator>();
        }
    }
}