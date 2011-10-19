using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.pubnub.api
{
    /// <summary>
    /// Captures the event arguments
    /// </summary>
    public sealed class PubNubEventArgs : EventArgs
    {
        /// <summary>
        /// The channel the message was received on
        /// </summary>
        public string Channel
        {
            get;
            set;
        }

        /// <summary>
        /// The raw JSON of the message recieved 
        /// </summary>
        public string Message
        {
            get;
            set;
        }
    }

}
