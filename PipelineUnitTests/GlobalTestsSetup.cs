using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using WebServices;
using ApplicationServicesHost;
using ServiceStack;
using Services.Dtos;
using ServiceStack.Configuration;
using System.Diagnostics;

namespace PipelineUnitTests
{
    [SetUpFixture]
    public class GlobalTestsSetup
    {
        public static GlobalTestsSetup Setup { get; private set; }

        public AppDomain WebServerDomain { get; private set; }
        public AppDomain AppServerDomain { get; private set; }
        public AppSettings AppSettings { get; private set; }
        public string HttpServerUrl { get; private set; }

        private static AppDomain CreateAppDomainHelper(string name)
        {
            var appDomainSetup = AppDomain.CurrentDomain.SetupInformation;
            var appDomain = AppDomain.CreateDomain(name, null, appDomainSetup);
            return appDomain;
        }

        [SetUp]
        public void Initialize()
        {
            this.WebServerDomain = CreateAppDomainHelper("WebServerDomain");
            this.AppServerDomain = CreateAppDomainHelper("AppServerDomain");

            try
            {
                this.WebServerDomain.DoCallBack(() => WebServicesAppHost.CreateAndStart());
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Unable to start web server: " + exception.Message);
                throw;
            }

            try
            {
                this.AppServerDomain.DoCallBack(() => RabbitMqAppHost.CreateAndStart());
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Unable to start application server: " + exception.Message);
                throw;
            }

            this.AppSettings = new AppSettings();
            var httpPort = this.AppSettings.Get<string>("HttpListenPort", null);
            Assert.IsNotNullOrEmpty(httpPort);
            this.HttpServerUrl = "http://localhost:" + httpPort;

            // Set the static property so other tests can access AppSettings, web server URL among other things.
            GlobalTestsSetup.Setup = this;
        }

        [TearDown]
        public void Dispose()
        {
            AppDomain.Unload(this.WebServerDomain);
            AppDomain.Unload(this.AppServerDomain);
        }
    }
}
