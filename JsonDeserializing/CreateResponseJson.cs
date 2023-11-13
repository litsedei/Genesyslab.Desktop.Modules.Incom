namespace Genesyslab.Desktop.Modules.Incom.JsonDeserializing
{
    public class CreateResponseJson
    {
        public CreateResponseJson(
            ErrorInfo errorInfo
        )
        {
            this.errorInfo = errorInfo;
        }

        public ErrorInfo errorInfo { get; }
    }
}