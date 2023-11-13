using System;
using System.Collections.Generic;
using System.Text;


namespace Genesyslab.Desktop.Modules.Incom
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
