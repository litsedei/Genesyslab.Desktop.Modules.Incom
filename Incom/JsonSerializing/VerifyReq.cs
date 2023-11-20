namespace Genesyslab.Desktop.Modules.Incom.JsonSerializing
{
    internal class VerifyReq
    {
        public string callToken { get; internal set; }

        public string cuid { get; internal set; }
        public string phoneNumber { get; internal set; }
       // public string callType { get; internal set; }
        public string agentId { get; internal set; }
        public string callUUID { get; internal set;}
    }
}