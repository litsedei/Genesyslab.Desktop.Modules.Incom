using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesyslab.Desktop.Modules.Incom.JsonSerializing
{
    internal class CallReqRecord
    {
        public string channelType { get; internal set; }
        public string callUUID { get; internal set; }
        public string requestType { get; internal set; }
        public string phoneNumber { get; internal set; }
    }
}

