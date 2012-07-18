using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example
{
    public sealed class ChatMessage
    {
        public string From
        {
            get;
            set;
        }

        public Guid FromUuid
        {
            get;
            set;
        }

        public string To
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
