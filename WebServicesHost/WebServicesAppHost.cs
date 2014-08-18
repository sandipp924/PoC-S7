using Common;
using Funq;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace WebServices
{
    /// <summary>
    /// ServiceStack http listener application with a singleton AppHost.
    /// </summary> 
    public class WebServicesAppHost : AppSelfHostBase
    {
        #region Member Vars

        private WebToAmqpMapperService _webToAmqpMapperService; 

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor. Initializes a new instance of ServiceStack application, with the specified name and assembly containing the services.
        /// </summary>
        public WebServicesAppHost() 
            : base("Web Services Host", typeof(WebServicesAppHost).Assembly)
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
            // AppSettings reads from the app.config, which is configured to include settings from the settings.xml
            // at the root solution folder level.
            IAppSettings settings = new AppSettings();
            Utilities.InitializeLogFactory(settings);

            //Set JSON web services to return idiomatic JSON camelCase properties   
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;

            //Change the default ServiceStack configuration
            Feature disableFeatures = Feature.Jsv | Feature.Soap;
            this.SetConfig(new HostConfig
            {
                EnableFeatures = Feature.All.Remove(disableFeatures), //all formats except of JSV and SOAP
                DebugMode = true, //Show StackTraces in service responses during development
                WriteErrorsToResponse = false, //Disable exception handling
                AllowJsonpRequests = true, //Enable JSONP requests
            });

            // Add no-cache response to direct the browser to not cache the responses.
            GlobalResponseFilters.Add((request, response, responseDto) =>
            {
                response.AddHeader("Cache-Control", "no-cache");
            });

            // WebToAmpqMapperService is the gateway to the AMQP services. It sends the requests to the MQ broker and
            // waits for responses.
            _webToAmqpMapperService = new WebToAmqpMapperService();

            // Register service proxies for DTO's of services that are configured to be run.
            this.RegisterDtosForServices(settings);
        }

        #endregion 

        #endregion

        #region Properties

        #region ListenUrl
        /// <summary>
        /// Gets the listen url that this server host is listening on.
        /// </summary>
        public string ListenUrl { get; private set; }
        #endregion 

        #endregion

        #region Methods

        #region Public Methods

        #region CreateAndStart
        /// <summary>
        /// Creates a new instance and starts the server.
        /// </summary>
        /// <param name="listenUrlOverride">If specified then this address is used for listening. Otherwise will listen on 
        /// localhost:port where the port is retrieved from 'HttpListenPort' setting in the application settings.</param>
        /// <returns>New instance of server host.</returns>
        public static WebServicesAppHost CreateAndStart(string listenUrlOverride = null)
        {
            IAppSettings settings = new AppSettings();

            var webServicesHost = new WebServicesAppHost();

            webServicesHost.ListenUrl = !string.IsNullOrEmpty(listenUrlOverride)
                ? listenUrlOverride
                : string.Format("http://localhost:{0}/", settings.Get<int>("HttpListenPort", 8080));

            webServicesHost.Init().Start(webServicesHost.ListenUrl);

            return webServicesHost;
        }

        #endregion  

        #endregion

        #region Private Methods

        #region GetCorrespondingResponseType
        /// <summary>
        /// Gets the typeof of object that is expected when a request of specified DTO type is processed by the associated service.
        /// </summary>
        /// <param name="dtoType">Request DTO type.</param>
        /// <returns>Corresponding DTO response type. This is based on IReturn<> implementation of DTO type or if that interface is
        /// not implemented by the DTO type then typeof(object) is returned.</returns>
        private static Type GetCorrespondingResponseType(Type dtoType)
        {
            Type responseType = typeof(object);
            if (dtoType.IsAssignableFrom(typeof(IReturn)))
            {
                var returnInterfaceType = dtoType.GetTypeWithGenericTypeDefinitionOf(typeof(IReturn<>));
                if (returnInterfaceType != null)
                    responseType = returnInterfaceType.GetGenericArguments().First();
            }

            return responseType;
        }
        #endregion 

        #region RegisterDtosForService
        /// <summary>
        /// Creates and registers a service for each DTO for the specified service.
        /// </summary>
        /// <param name="service">DTO's for this service will be processed.</param>
        /// <returns>True if the registration is successful. False otherwise.</returns>
        public bool RegisterDtosForService(string service)
        {
            var assemblyName = new AssemblyName("Services." + service + ".Dtos");
            Assembly serviceDtosAssembly = null;
            try
            {
                serviceDtosAssembly = Assembly.Load(assemblyName);
            }
            catch (Exception exception)
            {
                this.GetType().ErrorFormat("Unable to load assembly {0}.\r\n\t{1}", assemblyName, exception.Message);
                return false;
            }

            var requestDtoTypes = serviceDtosAssembly.ExportedTypes
                .Where(t => !t.IsAbstract && !t.IsInterface && t.GetCustomAttributes<RouteAttribute>().Any());

            foreach (var requestDtoType in requestDtoTypes)
            {
                var responseType = GetCorrespondingResponseType(requestDtoType);
                if (responseType != null)
                {
                    var serviceProxy = ServiceProxyFactory.Create(requestDtoType, responseType, _webToAmqpMapperService);
                    this.ServiceController.RegisterService(serviceProxy.GetType());
                    this.Container.Register(serviceProxy, serviceProxy.GetType());

                    this.GetType().DebugFormat("Registered service for {0} DTO.", requestDtoType.Name);
                }
            }

            return true;
        }
        #endregion

        #region RegisterDtosForServices
        /// <summary>
        /// Registers services based on what's specified in application configuration.
        /// </summary>
        /// <param name="settings"></param>
        private void RegisterDtosForServices(IAppSettings settings)
        {
            var servicesToRun = settings.GetList("ServicesToRun");
            foreach (var iiService in servicesToRun)
            {
                this.RegisterDtosForService(iiService);
            }
        }
        #endregion

        #endregion

        #endregion
    }
}