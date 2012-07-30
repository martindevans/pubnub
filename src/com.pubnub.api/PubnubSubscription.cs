using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.pubnub.api
{
    public sealed class PubnubSubscription
    {
        public PubnubSubscription()
        {
            Errors = new List<Exception>();
        }

        public string Channel
        {
            get;
            set;
        }

        public long TimeToken
        {
            get;
            set;
        }

        public List<Exception> Errors
        {
            get;
            set;
        }
    }

}
