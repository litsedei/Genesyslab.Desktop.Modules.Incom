using Genesyslab.Desktop.Infrastructure.Commands;
using Genesyslab.Desktop.Infrastructure.DependencyInjection;
using Genesyslab.Desktop.Infrastructure.ViewManager;
using Genesyslab.Desktop.Modules.Incom.CustomCommand;
using Genesyslab.Desktop.Modules.Incom.DispositionCodeEx;
using Genesyslab.Desktop.Modules.Incom.MySample;
using Genesyslab.Desktop.Modules.Windows.Views.Interactions.CaseView.DispositionCodeView;
//using Microsoft.Practices.Composite.Modularity;
using Genesyslab.Desktop.Infrastructure;
using Genesyslab.Desktop.Modules.Windows.Event;
using System.Windows;
using System;
using Genesyslab.Desktop.Infrastructure.Configuration;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Desktop.Modules.Incom.CrmConfgReader;
using Genesyslab.Desktop.Modules.Gms.CallbackInvitation.CustomCommands;
using System.Collections.Generic;

namespace Genesyslab.Desktop.Modules.Incom
{
    /// <summary>
    /// This class is a sample module which shows several ways of customization
    /// </summary>
    public struct CommandActivatorData
    {
        public String name;
        public String chain;
        public String afterCommand;
        public Type type;
    }


    public class IncomModule : IModule
	{
		readonly IObjectContainer container;
		readonly IViewManager viewManager;
		readonly ICommandManager commandManager;
		readonly IViewEventManager viewEventManager;
        readonly IConfigManager configManager;
        ILogger log;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncomModule"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="viewManager">The view manager.</param>
        /// <param name="commandManager">The command manager.</param>
        public IncomModule(IObjectContainer container, IViewManager viewManager, ICommandManager commandManager, IViewEventManager viewEventManager)
		{
			this.container = container;
			this.viewManager = viewManager;
			this.commandManager = commandManager;
			this.viewEventManager = viewEventManager;
            this.log = container.Resolve<ILogger>();  // Initialize the trace system
            this.log = log.CreateChildLogger("ExtensionSample");   // Create a child trace section
            this.configManager = container.Resolve<IConfigManager>();
        }

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public void Initialize()
		{
            log.Info("Initialize()");
            CfgReader config = new CfgReader(container);
            //container.RegisterInstance<ICfgReader>(config);
            //if (configManager.Task("InteractionWorkspace.omilia.canUse") == true)
            //{
            //    log.Debug("Omilia View is Enabled");
            //    container.RegisterType<IMyOmiliaView, MyOmiliaView>();
            //    log.Debug(String.Format("{0} container.RegisterType<IMyOmiliaView, MyOmiliaView>();", this.log));
            //    container.RegisterType<IMyOmiliaViewModel, MyOmiliaViewModel>();
            //    log.Debug(String.Format("{0} container.RegisterType<IMyOmiliaViewModel, MyOmiliaViewModel>();", this.log));
            //    // Put the MySample view in the region "InteractionWorksheetRegion"
            //    viewManager.ViewsByRegionName["InteractionWorksheetRegion"].Add(
            //            new ViewActivator() { ViewType = typeof(IMyOmiliaView), ViewName = "MyInteractionSample", ActivateView = true });
            //    // Here we register the view (GUI) "IMySampleButtonView"
            //    container.RegisterType<IMyOmiliaButtonView, MyOmiliaButtonView>();
            //    // Put the MySampleMenuView view in the region "CaseViewSideButtonRegion" (The case toggle button in the interaction windows)
            //    viewManager.ViewsByRegionName["CaseViewSideButtonRegion"].Add(
            //        new ViewActivator() { ViewType = typeof(IMyOmiliaButtonView), ViewName = "MySampleButtonView", ActivateView = true });
            //}



            ///////////////////////
            // GUI customization case 1
            // Replacing an already existing view "DispositionCodeView" in the Interaction Voice (not the behavior)

            // Associate the existing "IDispositionCodeView" with the new "DispositionCodeExView" implementation
            container.RegisterType<IDispositionCodeView, DispositionCodeExView>();


			///////////////////////
			// GUI customization case 2
			// Add a view in the workspace in the main tool bar

			// Here we register the view (GUI) "IMySampleView" and its behavior counterpart "IMySamplePresentationModel"
			container.RegisterType<IMyExtensionSampleView, MySampleView>();
			//container.RegisterType<IMyExtensionSampleViewModel, MySampleViewModel>();

			//// Put the MySample view in the region "ToolbarWorkplaceRegion" (The TabControl in the main toolbar)
			//viewManager.ViewsByRegionName["InteractionDetailsDispositionsRegion"].Insert(0,
			//	new ViewActivator() { ViewType = typeof(IMyExtensionSampleView), ViewName = "MySample" });

					// Here we register the view (GUI) "IMySampleMenuView"
			//container.RegisterType<IMySampleMenuView, MySampleMenuView>();

			// Put the MySampleMenuView view in the region "WorkspaceMenuRegion" (The Workspace toggle button in the main toolbar)
		//	viewManager.ViewsByRegionName["WorkspaceMenuRegion"].Insert(0,
		//		new ViewActivator() { ViewType = typeof(IMySampleMenuView), ViewName = "MySampleMenu", ActivateView = true });

		//	viewManager.ViewsByRegionName["CustomBundlePartyRegion"].Insert(0,
		//		new ViewActivator() { ViewType = typeof(IMySampleMenuView), ViewName = "MySampleMenu", ActivateView = true });

		//	viewManager.ViewsByRegionName["InteractionVoiceCustomButtonRegion"].Insert(0,
		//		new ViewActivator() { ViewType = typeof(IMySampleMenuView), ViewName = "MySampleMenu", ActivateView = true });


			viewManager.ViewsByRegionName["CustomInfoOnInteractionsBundleRegion"].Insert(0,
		new ViewActivator() { ViewType = typeof(IMyExtensionSampleView), ViewName = "MySample", ActivateView = true });


		//	viewManager.ViewsByRegionName["InteractionDetailsDispositionsRegion"].Insert(0,
		//new ViewActivator() { ViewType = typeof(IMySampleMenuView), ViewName = "MySampleMenu", ActivateView = true });





			///////////////////////
			// Command customization
			// Add a command before the release call
			//commandManager.CommandsByName["InteractionVoiceReleaseCall"].Insert(0,
			//	new CommandActivator() { CommandType = typeof(BeforeReleaseCallCommand) });


			///////////////////////
			// Event registration
			// Subscribe to the post login / post logout events
			viewEventManager.Subscribe(MyEventHandler);
            // Registering custom commands
            List<CommandActivatorData> customCommandList = new List<CommandActivatorData>();
            //   customCommandList.Add(new CommandActivatorData() { chain = "ApplicationClose", afterCommand = "IsPossibleToClose ", type = typeof(BeforeApplicationCloseCommand), name = "BeforeApplicationCloseCommand" });
            //   customCommandList.Add(new CommandActivatorData() { chain = "ApplicationClose", afterCommand = "CloseApplicationCommand ", type = typeof(BeforeApplicationCloseCommand), name = "BeforeApplicationCloseCommand" });
            customCommandList.Add(new CommandActivatorData() { chain = "MediaVoiceLogOn", afterCommand = "LogOn ", type = typeof(AfterMediaVoiceLogOnCommand), name = "AfterMediaVoiceLogOnCommand" });
            foreach (CommandActivatorData thisCommand in customCommandList)
            {
                this.log.Info(String.Format("{0} Inserting custom command [{1}] in command chain", this.log, thisCommand.name));
                IList<CommandActivator> insertedList = new List<CommandActivator>();
                insertedList.Add(new CommandActivator() { CommandType = thisCommand.type, Name = thisCommand.name });
                this.commandManager.InsertCommandToChainOfCommandAfter(thisCommand.chain, thisCommand.afterCommand, insertedList);
            }
        }

		void MyEventHandler(object eventObject)
		{
			string eventMessage = eventObject as string;
			if (eventMessage != null)
			{
				MessageBox.Show("EventHandler " + eventMessage);
				switch (eventMessage)
				{
					case "Loggin":
						MessageBox.Show("Post logged in");
						break;
					case "Logout":
						MessageBox.Show("Post logged out");
						viewEventManager.Unsubscribe(MyEventHandler);
						break;
				}
			}
		}

	}
}
