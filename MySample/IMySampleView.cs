using Genesyslab.Desktop.Infrastructure;
using Genesyslab.Desktop.Modules.Windows.Common.DimSize;

namespace Genesyslab.Desktop.Modules.Incom.MySample
{
	/// <summary>
	/// Interface matching the MySampleView view
	/// </summary>
	public interface IMyExtensionSampleView : IMin, IView
	{
		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		IMyExtensionSampleViewModel Model { get; set; }
	}
}
