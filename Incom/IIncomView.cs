using Genesyslab.Desktop.Infrastructure;
using Genesyslab.Desktop.Modules.Windows.Common.DimSize;
using Genesyslab.Platform.Commons.Protocols;

namespace Genesyslab.Desktop.Modules.Incom.IncomUI
{
	public interface IIncomView : IView
	{
		IIncomViewModel Model { get; set; }
        //string Header { get; set; }

        void EventEstablishedListener(IMessage userEvent);

        //  void EventReleasedListner(IMessage userEvent);
    }
}