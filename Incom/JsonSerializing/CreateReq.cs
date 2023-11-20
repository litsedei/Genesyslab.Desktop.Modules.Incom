namespace Genesyslab.Desktop.Modules.Incom.JsonSerializing
{
    internal class CreateReq
    {
        public string cuid { get; internal set; }
        public string callUUID { get; internal set; }
        public string phoneNumber { get; internal set; }
        public object agentId { get; internal set; }
        public string callToken { get; internal set; }
    }
}
