using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using com.pubnub.api;

namespace com.pubnub.api.test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class PublishTestClass
    {
        [TestMethod]
        public void PublishMessage()
        {            
            var pn = new Pubnub("demo", "demo", string.Empty);
            Assert.IsTrue(pn.Publish("csharp_unit_test", new
            {
                Name = "Example Unit Test Message"
            }));
        }

        /// <summary>
        /// Check if this is a threadsafe implemenation
        /// </summary>
        [TestMethod]
        public void PublishMessage_Parallel()
        {
            var pn = new Pubnub("demo", "demo", string.Empty);

            Parallel.For(0, 5, (i) =>
            {
                pn.Publish("csharp_unit_test", new
                {

                });
            });

        }

    }
}
