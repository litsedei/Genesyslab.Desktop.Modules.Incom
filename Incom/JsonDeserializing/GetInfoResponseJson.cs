namespace Genesyslab.Desktop.Modules.Incom.JsonDeserializing
{
    public class GetInfoResponseJson
    {
        public GetInfoResponseJson(
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
