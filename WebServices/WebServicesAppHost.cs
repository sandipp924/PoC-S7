using Funq;
using ServiceStack;
using ServiceStack.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AG.WebServices
{
    /// <summary>
    /// Create your ServiceStack http listener application with a singleton AppHost.
    /// </summary> 
    public class WebServicesAppHost : AppHostHttpListenerBase
    {
        /// <summary>
        /// Initializes a new instance of your ServiceStack application, with the specified name and assembly containing the services.
        /// </summary>
        public WebServicesAppHost() : base("AG Web Services Host", typeof(WebToAmpqMapperService).Assembly) { }

        /// <summary>
        /// Configure the container with the necessary routes for your ServiceStack application.
        /// </summary>
        /// <param name="container">The built-in IoC used with ServiceStack.</param>
        public override void Configure(Container container)
        {
        }
    }
}