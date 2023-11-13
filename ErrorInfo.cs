using System;
using System.Collections.Generic;
using System.Text;

namespace Genesyslab.Desktop.Modules.Incom
{
    public class ErrorInfo
    {
        public ErrorInfo(
            string uuid,
            string code,
            string description
        )
        {
            this.uuid = uuid;
            this.code = code;
            this.description = description;
        }

        public string uuid { get; }
        public string code { get; }
        public string description { get; }
    }
}
