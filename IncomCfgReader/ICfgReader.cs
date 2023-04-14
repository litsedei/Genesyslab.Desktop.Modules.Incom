using Genesyslab.Platform.Commons.Protocols;

namespace Genesyslab.Desktop.Modules.Incom.CrmConfgReader
{
    /// <summary>
    /// Interface definition for CfgReader.
    /// </summary>
    public interface ICfgReader
    {
        string GetMainConfig(string keyName);
    }
}
