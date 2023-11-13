namespace Genesyslab.Desktop.Modules.Incom.JsonDeserializing
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
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





