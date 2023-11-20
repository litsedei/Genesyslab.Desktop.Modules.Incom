namespace Genesyslab.Desktop.Modules.Incom.JsonDeserializing
{
    public class BusinessInfo
    {
        public BusinessInfo(
            string callToken,
            string len,
            string speechLen,
            string prob,
            bool isVoiceId,
            int disableRecording
        )
        {
            this.callToken = callToken;
            this.len = len;
            this.speechLen = speechLen;
            this.prob = prob;
            this.isVoiceId = isVoiceId;
            this.disableRecording = disableRecording;

        }

        public string callToken { get; }
        public string len { get; }
        public string speechLen { get; }
        public string prob { get; }
        public bool isVoiceId { get; }
        public int disableRecording { get; }
    }

}
