using System;
using System.Collections.Generic;
using System.Text;

namespace Genesyslab.Desktop.Modules.Incom
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





