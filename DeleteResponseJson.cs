using System;
using System.Collections.Generic;
using System.Text;

namespace Genesyslab.Desktop.Modules.Incom
{
    internal class DeleteResponseJson
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
