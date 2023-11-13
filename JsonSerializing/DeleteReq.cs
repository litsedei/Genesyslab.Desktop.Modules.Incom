namespace Genesyslab.Desktop.Modules.Incom.JsonSerializing
{
    internal class DeleteReq
    {
        public string cuid { get; internal set; }
        public string PhoneNumber { get; internal set; }
        public string agentId { get; internal set; }
        public string callUUID{ get; internal set;}
    }
}
