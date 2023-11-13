namespace Genesyslab.Desktop.Modules.Incom.JsonDeserializing
{
    public class DeleteResponseJson
    {
        public DeleteResponseJson(
            ErrorInfo errorInfo
        )
        {
            this.errorInfo = errorInfo;
        }

        public ErrorInfo errorInfo { get; }
    }
}
