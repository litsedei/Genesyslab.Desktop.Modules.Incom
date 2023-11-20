using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Genesyslab.Desktop.Infrastructure.Commands;
using Genesyslab.Desktop.Infrastructure.DependencyInjection;
using Genesyslab.Desktop.Modules.Core.Model.Interactions;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Desktop.Modules.Voice.Model.Interactions;

namespace Genesyslab.Desktop.Modules.Incom.CustomCommand
{
	/// <summary>
	/// Custom command which prompts a confirmation dialog before executing the ReleaseCall command
	/// </summary>
	class BeforeReleaseCallCommand : IElementOfCommand
	{
		readonly IObjectContainer container;
		ILogger log;

		/// <summary>
		/// Initializes a new instance of the <see cref="BeforeReleaseCallCommand"/> class.
		/// </summary>
		/// <param name="container">The container.</param>
		public BeforeReleaseCallCommand(IObjectContainer container)
		{
			this.container = container;

			// Initialize the trace system
			this.log = container.Resolve<ILogger>();

			// Create a child trace section
			this.log = log.CreateChildLogger("BeforeReleaseCallCommand");
		}

		/// <summary>
		/// Gets the name of the command. This is optional.
		/// </summary>
		/// <value>The command name.</value>
		public string Name { get { return "BeforeReleaseCall"; } set { } }

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		/// <param name="progress">The progress.</param>
		/// <returns></returns>
		public bool Execute(IDictionary<string, object> parameters, IProgressUpdater progress)
		{
			// To go to the main thread
			if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
			{
				object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new ExecuteDelegate(Execute), parameters, progress);
				return (bool)result;
			}
			else
			{
				// Ok, we are in the main thread
				log.Info("Execute");
				// Get the parameter
				IInteractionVoice interactionVoice = parameters["CommandParameter"] as IInteractionVoice;
				// Prompt the alert dialog
				return MessageBox.Show("Do you really want to release this call?\r\nThe call",
					"Release the call?", MessageBoxButton.YesNo) == MessageBoxResult.No;
			}
		}

		/// <summary>
		/// This delegate allows to go to the main thread.
		/// </summary>
		delegate bool ExecuteDelegate(IDictionary<string, object> parameters, IProgressUpdater progressUpdater);
	}
}
