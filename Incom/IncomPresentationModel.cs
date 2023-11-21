using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using System.Windows.Input;
using System.Windows.Threading;

namespace Genesyslab.Desktop.Modules.Incom.IncomUI
{
	/// <summary>
	/// The behabior of the IncomView class
	/// </summary>
	public class IncomViewModel : IIncomViewModel, INotifyPropertyChanged
	{
        // Field variables
        /// <summary>
        /// Initializes a new instance of the <see cref="IncomViewModel"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        public IncomViewModel()
		{
			
		}

        #region IIncomPresentationModel Members

        /// <summary>
        /// Gets or sets the header to set in the parent view.
        /// </summary>
        /// <value>The calluuid.</value>
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
