using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace com.pubnub.api
{

    /// <summary>
    /// Multithreaded Pubnub Client, can handle multiple subscriptions (will enforce not letting them 
    /// overlap - only within this one instance, not globally), will elegantly fail without any 
    /// exceptions (uses status codes). 
    /// </summary>
    public sealed class Pubnub
    {

        #region // Constructors //
        public Pubnub()
        {
            _subscriptions = new Dictionary<string, PubnubSubscription>();
        }

        public Pubnub(PubnubConfiguration configuration)
            : this()
        {
            PubnubConfiguration = configuration;
        }

        public Pubnub(string publishKey, string subscribeKey, string secretKey)
            : this()
        {
            PubnubConfiguration = new PubnubConfiguration
            {
                PublishKey = publishKey,
                SubscribeKey = subscribeKey,
                SecretKey = secretKey,
                EnableSsl = false
            };
        }

        public Pubnub(string publishKey, string subscribeKey, string secretKey, bool enableSsl)
            : this()
        {
            PubnubConfiguration = new PubnubConfiguration
            {
                PublishKey = publishKey,
                SubscribeKey = subscribeKey,
                SecretKey = secretKey,
                EnableSsl = enableSsl
            };
        }
        #endregion

        #region // Properties //
        private readonly object _sync = new object();
        private Dictionary<string, PubnubSubscription> _subscriptions;

        public PubnubConfiguration PubnubConfiguration
        {
            get;
            set;
        }
        #endregion

        public bool Publish(string channel, object obj)
        {
            return Publish(channel, JsonConvert.SerializeObject(obj));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json">A valid json string, if this is invalid we cannot be held accountable for the outcome...</param>
        /// <returns></returns>
        public bool Publish(string channel, string json)
        {
            var url = PubnubRequest.BuildUrl(PubnubConfiguration.EnableSsl,
                                             "publish",
                                             PubnubConfiguration.PublishKey,
                                             PubnubConfiguration.SubscribeKey,
                                             PubnubRequest.GetSignature(PubnubConfiguration.PublishKey,
                                                                        PubnubConfiguration.SubscribeKey,
                                                                        PubnubConfiguration.SecretKey,
                                                                        channel,
                                                                        json),
                                             channel,
                                             "0",
                                             json);

            var request = new PubnubRequest();
            var reply = string.Empty;
            request.Execute(url, out reply);

            // so apparently S & D are valid response codes, but I need to check this with SB
            // I also need to find out what an error message looks like
            var o = JArray.Parse(reply);
            return (int.Parse(o[0].ToString()) == 1);
        }

        public List<object> History(string channel, int limit)
        {
            var url = PubnubRequest.BuildUrl(PubnubConfiguration.EnableSsl,
                                             "history",
                                             PubnubConfiguration.SubscribeKey,
                                             channel,
                                             "0",
                                             limit.ToString());

            var request = new PubnubRequest();
            var json = string.Empty;
            request.Execute(url, out json);

            var list = JsonConvert.DeserializeObject<List<object>>(json);
            return list;
        }

        /// <summary>
        /// Get's a timestamp from the pubnub server cluster
        /// </summary>
        /// <returns></returns>
        public long Time()
        {
            var url = PubnubRequest.BuildUrl(PubnubConfiguration.EnableSsl,
                                             "time",
                                             "0");

            var request = new PubnubRequest();
            var json = string.Empty;
            request.Execute(url, out json);

            return Convert.ToInt64(JArray.Parse(json)[0].ToString());
        }

        /// <summary>
        /// Starts a looping subscription request which runs on it's own
        /// thread.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool Subscribe(string channel)
        {
            // Check if there a subscribtion to this channel already
            if (_subscriptions.ContainsKey(channel))
                return false;

            // Add a new one
            lock (_sync)
                _subscriptions.Add(channel, new PubnubSubscription
                {
                    Channel = channel,
                    TimeToken = 0
                });

            StartSubscription(_subscriptions[channel]);
            return true;
        }

        public bool Unsubscribe(string channel)
        {
            return Unsubscribe(channel, false);
        }

        /// <summary>
        /// Unsubscribe 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public bool Unsubscribe(string channel, bool force)
        {
            if (!_subscriptions.ContainsKey(channel))
                return false;

            lock (_sync)
                _subscriptions.Remove(channel);

            // send a blank message to the channel so the 
            // loop cycles to find the unsubcribe...
            if (force)
                Publish(channel, new { });

            return true;
        } 

        private void StartSubscription(PubnubSubscription subscription)
        {
            Task.Factory.StartNew(() =>
            {
                int failureCount = 0;

                // check if the subscription still exists in the dictionary
                // if not, then end this task, otherwise, repeat.
                using (var handle = new System.Threading.ManualResetEventSlim(false))
                {
                    while (_subscriptions.ContainsKey(subscription.Channel))
                    {
                        try
                        {
                            var url = PubnubRequest.BuildUrl(PubnubConfiguration.EnableSsl,
                                                             "subscribe",
                                                             PubnubConfiguration.SubscribeKey,
                                                             subscription.Channel,
                                                             "0",
                                                             subscription.TimeToken.ToString());

                            var request = new PubnubRequest();
                            var json = string.Empty;
                            request.Execute(url, out json);

                            var result = JsonConvert.DeserializeObject<List<object>>(json);
#if DEBUG
                            System.Diagnostics.Debug.WriteLine(json);
#endif
                            if (result[0] is JArray && result[0].ToString() != "[]")
                            {
                                // loop through each message and fire individually                            
                                for (var i = 0; i < ((JArray)result[0]).Count; i++)
                                {
                                    var message = ((JArray)result[0])[i].ToString();
                                    if (MessageRecieved != null && !string.IsNullOrEmpty(message))
                                        try
                                        {
                                            MessageRecieved(null, new PubNubEventArgs
                                            {
                                                Channel = subscription.Channel,
                                                Message = message
                                            });
                                        }
                                        catch (Exception exp)
                                        {
                                            // adding this try catch because if we have multiple messages and one of 
                                            // them encouters an unhandled exception it should not impact the others 
                                            // or the subscription time token.
                                            System.Diagnostics.Debug.WriteLine("MessageRecievedException: " + exp.Message);
                                        }
                                }
                            }

                            // update the time token
                            lock (_sync)
                                if (_subscriptions.ContainsKey(subscription.Channel))
                                    _subscriptions[subscription.Channel].TimeToken = Convert.ToInt64(result[1].ToString());

                            // reset the failure count
                            failureCount = 0;
                        }
                        catch (Exception exp)
                        {                            
                            System.Diagnostics.Debug.WriteLine("SubscriptionException: " + exp.Message);
                            
                            failureCount++;
                            handle.Wait(GetWaitTimeForErrorCount(failureCount));

                            // rather than throwing the errors, we collect them for 
                            // periodic analysis, the idea is to enhance this with error limits
                            // and a backoff strategy incase there is a problem with Pubnub
                            // or the local connection to pubnub
                            lock (_sync)
                                if (_subscriptions.ContainsKey(subscription.Channel))
                                    _subscriptions[subscription.Channel].Errors.Add(exp);
                        }
                    }
                }

            });
        }

        /// <summary>
        /// Manages the backoff based on the current error count, this prevents high 
        /// CPU usage in the case of a network interuption to either pubnub or 
        /// our local server's connectivity.
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        private int GetWaitTimeForErrorCount(int errors)
        {
            if (errors < 5)
                return 0;
            
            if (errors < 10)
                return 1000;

            if (errors < 20)
                return 2000;

            if (errors < 30)
                return 3000;

            if (errors < 40)
                return 4000;

            if (errors < 50)
                return 5000;

            if (errors < 100)
                return 10000;

            return 15000;
        }

        public event EventHandler<PubNubEventArgs> MessageRecieved;

    }
}
