using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models;

namespace Sufong2001.Comm.Configurations.Modules
{
    public class UploadModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<IdGenerator>().SingleInstance().As<IUploadIdGenerator>().As<IMessageIdGenerator>();
            builder.RegisterType<App>().SingleInstance().AsSelf();
            //builder.RegisterType<Goodbyer>().Named<IGoodbyer>("Main");
            //builder.RegisterType<AlternateGoodbyer>().Named<IGoodbyer>("Secondary");

        }
    }
}
