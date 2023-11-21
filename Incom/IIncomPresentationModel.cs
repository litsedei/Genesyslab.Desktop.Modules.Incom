using Genesyslab.Desktop.Modules.Core.Model.Interactions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Genesyslab.Desktop.Modules.Incom.IncomUI
{

	public interface IIncomViewModel
	{
        bool btnCreateState { get; set; }
        // IIncomViewModel Model { get; set; }

        //ICase Case { get; set; }
    }
}


//public interface IMyExtensionSampleViewModel
//{
//    /// <summary>
//    /// Gets or sets the header to set in the parent view.
//    /// </summary>
//    /// <value>The header.</value>
//    string Header { get; set; }

//    /// <summary>
//    /// Gets or sets my collection.
//    /// </summary>
//    /// <value>
//    /// My collection.
//    /// </value>
//    ObservableCollection<IMyListItem> MyCollection { get; set; }
//}
