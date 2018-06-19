using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using gov.ncats.ginas.excel.tools.Model;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void GinasToolsConfigurationToStringTest()
        {
            GinasToolsConfiguration configuration = new GinasToolsConfiguration();
            string toStringOutput1 = configuration.ToString();
            Console.WriteLine(toStringOutput1);
            Assert.IsNotNull(toStringOutput1);
            configuration.SelectedServer = new GinasServer();
            string toStringOutput2 = configuration.ToString();
            Console.WriteLine(toStringOutput2);
            Assert.IsNotNull(toStringOutput2);

            configuration.SelectedServer.ServerUrl = "www.ginas.com";
            string toStringOutput3 = configuration.ToString();
            Console.WriteLine(toStringOutput3);
            Assert.IsNotNull(toStringOutput3);

            configuration.Servers.Add(new GinasServer());
            configuration.Servers[0].ServerUrl = "tripod.ncats.nih.gov";
            configuration.Servers[0].Username = "mmmmmmm";
            configuration.Servers[0].Token = "token";
            string toStringOutput4 = configuration.ToString();
            Console.WriteLine(toStringOutput4);
            Assert.IsNotNull(toStringOutput4);

            configuration.Servers.Add(new GinasServer());
            configuration.Servers[1].ServerUrl = "nlm.nih.gov";
            configuration.Servers[1].Username = "mmmmmmm2222";
            configuration.Servers[1].ServerName = "NLM";
            string toStringOutput5 = configuration.ToString();
            Console.WriteLine(toStringOutput5);
            Assert.IsNotNull(toStringOutput5);
        }

        [TestMethod]
        public void SetDate()
        {
            DateTime newExpirationDate = DateTime.Now.AddSeconds(120);
            string msg = string.Format("Set expiration to {0} on {1}", newExpirationDate.ToLongTimeString(),
                newExpirationDate.ToShortDateString());
            Console.WriteLine(msg);
            Assert.IsTrue(newExpirationDate > DateTime.Now);
        }
    }
}
