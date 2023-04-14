using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using Genesyslab.Desktop.Modules.Windows.Common.DimSize;

namespace Genesyslab.Desktop.Modules.ExtensionSample.MySample
{
	/// <summary>
	/// Interaction logic for MySampleView.xaml
	/// </summary>
	public partial class MySampleView : UserControl, IMyExtensionSampleView
	{
		public MySampleView(IMyExtensionSampleViewModel mySampleViewModel)
		{
			this.Model = mySampleViewModel;

			InitializeComponent();

			Width = Double.NaN;
			Height = Double.NaN;
			MinSize = new MSize() { Width = 400.0, Height = 400.0 };
		}

		#region IMySampleView Members

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public IMyExtensionSampleViewModel Model
		{
			get { return this.DataContext as IMyExtensionSampleViewModel; }
			set { this.DataContext = value; }
		}


		#endregion

		#region IView Members

		/// <summary>
		/// Gets or sets the context.
		/// </summary>
		/// <value>The context.</value>
		public object Context { get; set; }

		/// <summary>
		/// Creates this instance.
		/// </summary>
		public void Create()
		{
			ObservableCollection<IMyListItem> collection = new ObservableCollection<IMyListItem>();
			collection.Add(new MyListItem() { LastName = "Doe", FirstName = "John" });
			collection.Add(new MyListItem() { LastName = "Dupont", FirstName = "Marcel" });

			Model.MyCollection = collection;
		}

		/// <summary>
		/// Destroys this instance.
		/// </summary>
		public void Destroy()
		{
		}

		#endregion
		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion
		MSize _MinSize;
		public MSize MinSize
		{
			get { return _MinSize; }  // (MSize)base.GetValue(MinSizeProperty); }
			set
			{
				_MinSize = value; // base.SetValue(MinSizeProperty, value);
				OnPropertyChanged("MinSize");
			}
		}

	}


	class MyListItem : IMyListItem
	{

		#region IMyListItem Members

		string firstName;
		public string FirstName
		{
			get
			{
				return firstName;
			}
			set
			{
				if (value != firstName) { firstName = value; OnPropertyChanged("FirstName"); }
			}
		}

		string lastName;
		public string LastName
		{
			get
			{
				return lastName;
			}
			set
			{
				if (value != lastName) { lastName = value; OnPropertyChanged("LastName"); }
			}
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion


	}
}
