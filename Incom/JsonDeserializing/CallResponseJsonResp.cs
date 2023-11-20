namespace Genesyslab.Desktop.Modules.Incom.JsonDeserializing
{
    public class CallResponseJson
    {
        public CallResponseJson(
            BusinessInfo businessInfo,
            ErrorInfo errorInfo
        )
        {
            this.businessInfo = businessInfo;
            this.errorInfo = errorInfo;
        }

        public BusinessInfo businessInfo { get; }
        public ErrorInfo errorInfo { get; }
    }
}





