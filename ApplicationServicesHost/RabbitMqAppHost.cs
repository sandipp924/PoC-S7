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
using RabbitMQ.Client;
using System.Threading;
using Common;
using ServiceStack.Logging;

namespace ApplicationServicesHost
{
    public class RabbitMqAppHost : BasicAppHost
    {
        #region Member Vars

        private readonly Dictionary<IRabbitMqMessageProcessor, Thread> _rabbitMqMessageProcessors = new Dictionary<IRabbitMqMessageProcessor, Thread>(); 

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor. Initializes a new instance of your RabbitMQ Services host application, with the specified name and assembly containing the services.
        /// </summary>
        public RabbitMqAppHost()
            : base("RabbitMQ Services Host", typeof(RabbitMqAppHost).Assembly)
        {
        } 
        #endregion

        #region Base Overrides

        #region Configure
        /// <summary>
        /// Configure the container with the necessary routes for your ServiceStack application.
        /// </summary>
        /// <param name="container">The built-in IoC used with ServiceStack.</param>
        public override void Configure(Container container)
        {
            //Set JSON web services to return idiomatic JSON camelCase properties
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;

            //Change the default ServiceStack configuration
            this.SetConfig(new HostConfig
            {
                EnableFeatures = Feature.Json,
                DefaultContentType = MimeTypes.Json,
                PreferredContentTypes = new List<string> { MimeTypes.Json }
            });

            // AppSettings reads from the app.config, which is configured to include settings from the settings.xml
            // at the root solution folder level.
            IAppSettings settings = new AppSettings();
            Utilities.InitializeLogFactory(settings);
            this.RegisterServices(settings);

            this.Run();
        }
        #endregion

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion 

        #endregion

        #region Methods

        #region Public Methods

        #region CreateAndStart
        /// <summary>
        /// Creates a new instance and starts the server.
        /// </summary>
        /// <returns>New instance of server host.</returns>
        public static RabbitMqAppHost CreateAndStart()
        {
            var appHost = new RabbitMqAppHost();
            appHost.Init();

            return appHost;
        }

        #endregion 

        #region RegisterService
        /// <summary>
        /// Registers a service.
        /// </summary>
        /// <param name="service">Name of the service to configure. Conventions established for naming are as the following. The name of the service
        /// corresponds to the name of the service interface without 'I'. The service class implementation will be of the same
        /// name. Mock service implementation will have 'Mock' prefix. The assembly name will be the 'Services.X' where X is
        /// the class name of the service. Example, IReferenceService is interface, ReferenceService and MockReferenceService
        /// are class names and Services.ReferenceService is assembly. The namespace is Services.</param>
        /// <param name="serviceConfig"></param>
        /// <returns>True if the registration is successful. False otherwise.</returns>
        public bool RegisterService(string service, ServiceConfigurationSettings serviceConfig)
        {
            var serviceInterfaceName = "I" + service;
            var serviceClassName = (serviceConfig.UseMock.Value ? "Mock" : "") + service;
            var assemblyName = new AssemblyName("Services." + serviceClassName);
            var nameSpace = "Services";
            Assembly serviceAssembly = null;
            try
            {
                serviceAssembly = Assembly.Load(assemblyName);
            }
            catch (Exception exception)
            {
                this.GetType().ErrorFormat("Unable to load assembly {0}.\r\n\t{1}", assemblyName, exception.Message);
                return false;
            }

            Type serviceInterfaceType = null, serviceClassType = null;

            try
            {
                serviceClassType = serviceAssembly.GetType(nameSpace + "." + serviceClassName);
                serviceInterfaceType = serviceClassType.GetInterfaces().FirstOrDefault(t => t.Name == serviceInterfaceName);
            }
            catch (Exception exception)
            {
                this.GetType().ErrorFormat("Unable to load service interface {0} and class {1} types from assembly {2}.\r\n\t{3}",
                    serviceInterfaceName, serviceClassName, assemblyName, exception.Message);
                return false;
            }

            if (serviceInterfaceName == null || serviceInterfaceType == null)
            {
                this.GetType().ErrorFormat("Unable to load service interface {0} and class {1} types from assembly {2}.", serviceInterfaceName, serviceClassName, assemblyName);
                return false;
            }

            if (!typeof(IService).IsAssignableFrom(serviceClassType))
            {
                this.GetType().ErrorFormat("Service {0} class from {1} assembly does not implement IService interface.", serviceClassType.Name, serviceAssembly.FullName);
                return false;
            }

            this.ServiceController.RegisterService(serviceClassType);
            this.Container.Register(Activator.CreateInstance(serviceClassType), serviceInterfaceType);
            this.GetType().DebugFormat("Registered {0} service.", serviceClassType.Name);

            return true;
        }
        #endregion

        #endregion

        #region Private Methods

        #region RegisterServices
        /// <summary>
        /// Registers services based on what's specified in application configuration.
        /// </summary>
        /// <param name="settings"></param>
        private void RegisterServices(IAppSettings settings)
        {
            var servicesToRun = settings.GetList("ServicesToRun");
            foreach (var iiService in servicesToRun)
            {
                var serviceConfig = settings.Get(iiService, new ServiceConfigurationSettings());
                serviceConfig.UseMock = serviceConfig.UseMock ?? false;

                this.RegisterService(iiService, serviceConfig);
            }
        }
        #endregion

        #region Run
        /// <summary>
        /// Listens for AMQP messages and responds to them.
        /// </summary>
        private void Run()
        {
            IAppSettings settings = new AppSettings();
            var connectionFactory = Utilities.CreateMqConnectionFactory(settings);
            var mqConfiguration = Utilities.GetMqConfiguration(settings);

            var operations = this.Metadata.Operations.ToArray();
            foreach (var op in operations)
            {
                var messageProcessor = RabbitMqMessageProcessorFactory.Create(
                    op.RequestType,
                    op.ResponseType,
                    connectionFactory,
                    mqConfiguration,
                    requestDto => this.ServiceController.Execute(requestDto, new BasicRequest { Verb = HttpMethods.Get }));

                var serverThread = new Thread(messageProcessor.Run) { IsBackground = true };
                _rabbitMqMessageProcessors.Add(messageProcessor, serverThread);
                serverThread.Start();
            }
        }
        #endregion

        #endregion 

        #endregion
    }
}
