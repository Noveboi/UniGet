using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public static class Protocol
    {
        public const byte REQUEST_GUID = 100;
        public const byte SEND_DATA = 0;
        public const byte TERMINATE_CONNECTION = 1;
    }
}
