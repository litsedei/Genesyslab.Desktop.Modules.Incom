using System;
using System.Collections.Generic;
using System.Text;

namespace Genesyslab.Desktop.Modules.Incom
{
    public class BusinessInfo
    {
        public BusinessInfo(
            string callToken,
            string len,
            string speechLen,
            string prob,
            bool isVoiceId,
            bool isDelete
        )
        {
            this.callToken = callToken;
            this.len = len;
            this.speechLen = speechLen;
            this.prob = prob;
            this.isVoiceId = isVoiceId;
            this.isDelete = isDelete;

        }

        public string callToken { get; }
        public string len { get; }
        public string speechLen { get; }
        public string prob { get; }
        public bool isVoiceId { get; }
        public bool isDelete { get; }
        public bool disableRecording { get; internal set; }
    }

}
