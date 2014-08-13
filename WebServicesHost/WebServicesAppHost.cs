using Common;
using Funq;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServices
{
    /// <summary>
    /// ServiceStack http listener application with a singleton AppHost.
    /// </summary> 
    public class WebServicesAppHost : AppSelfHostBase
    {
        #region Constructor
        /// <summary>
        /// Constructor. Initializes a new instance of ServiceStack application, with the specified name and assembly containing the services.
        /// </summary>
        public WebServicesAppHost()
            : base("Web Services Host", typeof(WebToAmpqMapperService).Assembly)
        {
        } 
        #endregion

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
        } 
        #endregion
    }
}