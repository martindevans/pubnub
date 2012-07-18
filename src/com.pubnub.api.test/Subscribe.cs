using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using com.pubnub.api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace com.pubnub.api.test
{
    [TestClass]
    public class SubscribeTestClass
    {
        [TestMethod]
        public void Subscribe()
        {
        }

        [TestMethod]
        public void Subscribe_MultipleChannels()
        {
        }

        /// <summary>
        /// Test a high throughput an reliablity scenario, ensure client 
        /// copes with a higher demand.
        /// </summary>
        [TestMethod]
        public void Subscribe_HighThroughPut()
        {
            var handle = new ManualResetEvent(false);
            var pn = new Pubnub("demo", "demo", string.Empty);
            var list = new Dictionary<int, object>();
            int count = 10;
            var sync = new object();

            pn.MessageRecieved += (s, e) =>
            {
                Task.Factory.StartNew(() =>
                {
                    if (e.Channel == "csharp_throughput_test")
                    {
                        
                        var o = JObject.Parse(e.Message);
                        System.Diagnostics.Debug.WriteLine(o["ID"].ToString());
                            
                        lock (sync)
                            list.Remove(Convert.ToInt32(o["ID"].ToString()));

                        if (Interlocked.Decrement(ref count) == 0)
                        {
                            System.Threading.Thread.Sleep(1000);
                            handle.Set();
                        }
                    }
                });
            };
            pn.Subscribe("csharp_throughput_test");

            System.Threading.Thread.Sleep(10000);

            Parallel.For(0, 10, (i) =>
            {
                lock (sync)
                    list.Add(i, null);

                // System.Diagnostics.Debug.WriteLine(i);
                var test = (pn.Publish("csharp_throughput_test", new
                {
                    ID = i
                }));


                if (!test)
                {
                    lock (sync)
                        list.Remove(i);
                    System.Diagnostics.Debug.WriteLine("Failed: " + i);
                }

            });

            handle.WaitOne(Convert.ToInt32(new TimeSpan(0, 1, 0).TotalMilliseconds));
            Assert.IsTrue(count == 0);
        }
    }
}
