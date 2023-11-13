namespace Genesyslab.Desktop.Modules.Incom.JsonDeserializing
{
    public class VerifyResponseJson
    {
        public VerifyResponseJson(
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
