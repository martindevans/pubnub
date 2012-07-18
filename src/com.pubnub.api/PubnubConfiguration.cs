using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.pubnub.api
{
    public sealed class PubnubConfiguration
    {
        // Proxy Server
        // Key Settings

        public string PublishKey
        {
            get;
            set;
        }

        public string SubscribeKey
        {
            get;
            set;
        }

        public string SecretKey
        {
            get;
            set;
        }

        public bool EnableSsl
        {
            get;
            set;
        }

    }

}
