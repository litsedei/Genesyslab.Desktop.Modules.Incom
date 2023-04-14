using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Genesyslab.Desktop.Infrastructure;
using Genesyslab.Desktop.Infrastructure.ViewManager;
using Genesyslab.Desktop.Modules.Core.Model.Interactions;
using Genesyslab.Desktop.Modules.Windows.Views.Interactions.CaseView.DispositionCodeView;

namespace Genesyslab.Desktop.Modules.Incom.DispositionCodeEx
{
	/// <summary>
	/// Interaction logic for DispositionCodeExView.xaml
	/// </summary>
	public partial class DispositionCodeExView : UserControl, IDispositionCodeView
	{
		readonly IViewManager viewManager;
        bool isEnabledChangeAttachedData;

		public DispositionCodeExView(IDispositionCodeViewModel dispositionCodePresentationModel, IViewManager viewManager)
		{
			this.viewManager = viewManager;
			this.Model = dispositionCodePresentationModel;

            isEnabledChangeAttachedData = true;

			InitializeComponent();

			Width = Double.NaN;
			Height = Double.NaN;
		}

		#region IView Members

		public object Context { get; set; }

		public void Create()
		{
			ICase myCase  = (Context as IDictionary<string, object>).TryGetValue("Case") as ICase;

			if (myCase != null)
			{

				Model.Interaction = myCase.MainInteraction;

				if (Model.Interaction != null)
				{
					INotifyPropertyChanged notifyPropertyChanged = Model.Interaction as INotifyPropertyChanged;
					if (notifyPropertyChanged != null)
						notifyPropertyChanged.PropertyChanged += new PropertyChangedEventHandler(notifyPropertyChanged_PropertyChanged);
				}

			}
			Model.Load();
		}

		public void Destroy()
		{
            if (Model.Interaction != null)
            {
                INotifyPropertyChanged notifyPropertyChanged = Model.Interaction as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                    notifyPropertyChanged.PropertyChanged -= new PropertyChangedEventHandler(notifyPropertyChanged_PropertyChanged);
            }
			Model.Unload();
		}

		#endregion

		#region IDispositionCodeView Members

		public IDispositionCodeViewModel Model
		{
			get { return this.DataContext as IDispositionCodeViewModel; }
			set { this.DataContext = value; }
		}

		#endregion

		void notifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
            if (!Dispatcher.CheckAccess())
                Dispatcher.BeginInvoke(new PropertyChangedEventHandler(notifyPropertyChanged_PropertyChanged), sender, e);
            else
            {
				if (e.PropertyName == "IsIdle")
				{
					isEnabledChangeAttachedData = false;

					if (!Model.Interaction.IsChildInteraction)
					{
						if (Model.Interaction.DispositionCode != null)
						{
							if (Model.Interaction.DispositionCode.IsReadOnlyOnIdle)
							{
								//listOfDispositionCodeValueItemsControl.IsEnabled = false;
							}
						}
					}
				}
            }
		}

		private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            IInteraction interaction = Model.Interaction;

            if (isEnabledChangeAttachedData)
            {
                if (Model.Interaction != null)
                    if (Model.Interaction.DispositionCode.SelectedDispositionCodeValue != null)
                        interaction.SetAttachedData(interaction.DispositionCode.CodeKeyName, interaction.DispositionCode.SelectedDispositionCodeValue.Name);
                    else
                        interaction.RemoveAttachedData(interaction.DispositionCode.CodeKeyName);
            }
        }
	}
}
