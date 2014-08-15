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
using System.Net;

namespace PipelineUnitTests
{
    [TestFixture]
    public class HttpTests
    {
        [Test]
        public void GetRootFromWebServer()
        {
            var client = new JsonServiceClient(GlobalTestsSetup.Setup.HttpServerUrl);
            var httpResponse = client.Get(string.Empty);
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void GetSymbologyInfo()
        {
            var client = new JsonServiceClient(GlobalTestsSetup.Setup.HttpServerUrl);
            const string SYCODE = "SY-CODE-123";
            var symbologyInfo = client.Get<SymbologyInfo>("/Reference/Symbology/" + SYCODE);

            Assert.That(symbologyInfo, Is.Not.Null);
            Assert.That(symbologyInfo.SyCode, Is.EqualTo(SYCODE));
        }

        [Test]
        public void TestCamelCaseJSON()
        {
            var client = new JsonServiceClient(GlobalTestsSetup.Setup.HttpServerUrl);
            const string SYCODE = "SY-CODE-123";
            var jsonText = client.Get<string>("/Reference/Symbology/" + SYCODE);

            Assert.IsTrue(jsonText.Contains("syCode"));
        }
    }
}
