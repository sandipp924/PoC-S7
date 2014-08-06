using Funq;
using ServiceStack;
using ServiceStack.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Services;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;
using Services.Dtos;
using ServiceStack.Configuration;

namespace ApplicationServicesHost
{
    public class BasicAppHost : ServiceStackHost
    {
        public BasicAppHost(string serviceName, params Assembly[] serviceAssemblies)
            : base(serviceName, serviceAssemblies)
        {
            this.ExcludeAutoRegisteringServiceTypes = new HashSet<Type>();
        }

        public override void Configure(Container container)
        {
            if (ConfigureContainer != null)
                ConfigureContainer(container);
        }

        public Action<Container> ConfigureContainer { get; set; }

        public Action<BasicAppHost> ConfigureAppHost { get; set; }

        public Action<HostConfig> ConfigFilter { get; set; }

        public Func<BasicAppHost, ServiceController> UseServiceController
        {
            set { ServiceController = value(this); }
        }

        public override void OnBeforeInit()
        {
            if (ConfigureAppHost != null)
                ConfigureAppHost(this);

            base.OnBeforeInit();
        }

        public override void OnConfigLoad()
        {
            base.OnConfigLoad();

            if (ConfigFilter != null)
                ConfigFilter(Config);
        }
    }
}
