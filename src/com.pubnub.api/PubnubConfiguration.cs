using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.pubnub.api
{
    public sealed class PubnubConfiguration
    {
        // Proxy Server
        // Key Settings


        private static PubnubConfiguration instance;
        public static PubnubConfiguration Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new PubnubConfiguration();
                    instance.GetFromFile();

                    Interlocked.CompareExchange(ref instance, instance, null);
                }
                return instance;            
            }
        }

        private void GetFromFile()
        {
            var oconfig = ConfigurationHandler.Get();
            if (oconfig == null)
                throw new Exception("Picaholic section not found in config file");

            // update the properties
            instance.SubscribeKey = oconfig.SubscribeKey;
            instance.PublishKey = oconfig.PublishKey;
            instance.SecretKey = oconfig.SecretKey;
        }

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

    internal class ConfigurationHandler : ConfigurationSection
    {
        public static ConfigurationHandler Get()
        {
            return (ConfigurationHandler)ConfigurationManager.GetSection("pubnub/settings");
        }

        [ConfigurationProperty("publishKey", IsRequired = false)]
        public string PublishKey
        {
            get
            {
                return (string)this["publishKey"];
            }
            set
            {
                this["publishKey"] = value;
            }
        }

        [ConfigurationProperty("subscribeKey", IsRequired = false)]
        public string SubscribeKey
        {
            get
            {
                return (string)this["subscribeKey"];
            }
            set
            {
                this["subscribeKey"] = value;
            }
        }

        [ConfigurationProperty("secretKey", IsRequired = false)]
        public string SecretKey
        {
            get
            {
                return (string)this["secretKey"];
            }
            set
            {
                this["secretKey"] = value;
            }
        }

        [ConfigurationProperty("enableSSL", IsRequired = false)]
        public bool EnableSSL
        {
            get
            {
                return (bool)this["enableSSL"];
            }
            set
            {
                this["enableSSL"] = value;
            }
        }

    }

}
