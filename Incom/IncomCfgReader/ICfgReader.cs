using Genesyslab.Platform.Commons.Protocols;

namespace Genesyslab.Desktop.Modules.Incom.IncomConfgReader
{
    /// <summary>
    /// Interface definition for CfgReader.
    /// </summary>
    public interface ICfgReader
    {
        string GetMainConfig(string keyName);
    }
}
