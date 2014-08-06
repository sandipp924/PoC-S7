using Funq;
using ServiceStack;
using ServiceStack.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServices
{
    /// <summary>
    /// Create your ServiceStack http listener application with a singleton AppHost.
    /// </summary> 
    public class WebServicesAppHost : AppSelfHostBase
    {
        /// <summary>
        /// Initializes a new instance of your ServiceStack application, with the specified name and assembly containing the services.
        /// </summary>
        public WebServicesAppHost() : base("Web Services Host", typeof(WebToAmpqMapperService).Assembly) { }

        /// <summary>
        /// Configure the container with the necessary routes for your ServiceStack application.
        /// </summary>
        /// <param name="container">The built-in IoC used with ServiceStack.</param>
        public override void Configure(Container container)
        {
            //Set JSON web services to return idiomatic JSON camelCase properties   
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;

            //Change the default ServiceStack configuration
            Feature disableFeatures = Feature.Jsv | Feature.Soap;
            this.SetConfig(new HostConfig
            {
                EnableFeatures = Feature.All.Remove(disableFeatures), //all formats except of JSV and SOAP
                DebugMode = true, //Show StackTraces in service responses during development
                WriteErrorsToResponse = false, //Disable exception handling
                //DefaultContentType = MimeTypes.Json, //Change default content type
                AllowJsonpRequests = true, //Enable JSONP requests
            });
        }
    }
}