using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace com.pubnub.api.test
{
    [TestClass]
    public class HistoryTestClass
    {
        [TestMethod]
        public void History()
        {
            var channel = "csharp_history_test";
            var pn = new Pubnub("demo", "demo", string.Empty);
            pn.Publish(channel, new { name = "test" });
            Assert.IsTrue(pn.History("csharp_history_test", 10).Count > 0);
        }

    }
}
