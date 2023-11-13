using Genesyslab.Desktop.Modules.Incom;

namespace Genesyslab.Desktop.Modules.Incom
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