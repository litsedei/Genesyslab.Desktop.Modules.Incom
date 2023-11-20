namespace Genesyslab.Desktop.Modules.Incom.JsonDeserializing
{
    public class ErrorInfo
    {
        public ErrorInfo(
            string uuid,
            int code,
            string description
        )
        {
            this.uuid = uuid;
            this.code = code;
            this.description = description;
        }

        public string uuid { get; }
        public int code { get; }
        public string description { get; }
    }
}
