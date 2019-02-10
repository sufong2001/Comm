using Autofac;
using Sufong2001.Comm.Configurations.Modules;

namespace Sufong2001.Comm.Configurations.Resolvers
{
    public class CommConfig : BaseConfig
    {
        public CommConfig(string functionName) : base(functionName)
        {
        }

        public override void Configuration(ContainerBuilder cfg)
        {
            cfg.RegisterModule(new CommonModule());
        }
    }
}