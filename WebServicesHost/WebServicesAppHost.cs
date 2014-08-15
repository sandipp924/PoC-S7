﻿using Common;
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
    }
}