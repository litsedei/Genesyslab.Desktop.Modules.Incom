namespace Genesyslab.Desktop.Modules.Incom.JsonSerializing
{
    internal class CallReq
    {
        public string callToken { get; internal set; }
        public string channelType { get; internal set; }
        public string callUUID { get; internal set; }
        public string requestType { get; internal set; }
        public string phoneNumber { get; internal set; }
    }
}
