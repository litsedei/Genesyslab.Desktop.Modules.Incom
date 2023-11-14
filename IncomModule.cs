using Genesyslab.Desktop.Infrastructure.Commands;
using Genesyslab.Desktop.Infrastructure.DependencyInjection;
using Genesyslab.Desktop.Infrastructure.ViewManager;
using Genesyslab.Desktop.Modules.Incom.UserEvenManagment;
using Genesyslab.Desktop.Modules.Incom.IncomUI;
using Genesyslab.Desktop.Modules.Incom;
using Genesyslab.Desktop.Modules.Incom.IncomConfgReader;
using Genesyslab.Desktop.Infrastructure;
using Genesyslab.Desktop.Modules.Windows.Event;
using System;
using Genesyslab.Desktop.Infrastructure.Configuration;
using Genesyslab.Platform.Commons.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Genesyslab.Desktop.Modules.Core.Configurations;
using Genesyslab.Desktop.Modules.Incom.CustomCommands;

namespace Genesyslab.Desktop.Modules.Incom
{
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
        private readonly ILogger log;
       
        /// <summary>
		/// Initializes a new instance of the <see cref="IncomModule"/> class.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="viewManager">The view manager.</param>
		/// <param name="commandManager">The command manager.</param>

        public IncomModule(IObjectContainer container, IViewManager viewManager, ICommandManager commandManager, IViewEventManager viewEventManager, IConfigManager configManager)
		{
			this.container = container;
			this.viewManager = viewManager;
			this.commandManager = commandManager;
		//	this.viewEventManager = viewEventManager;
            log = container.Resolve<ILogger>();
            this.configManager = container.Resolve<IConfigManager>();
           
           
        }

        public void Initialize()
		{
            try
            {
            log.Info("Incom: IncomModule Initialize start");
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            log.Info("Incom: IncomModule  file version : " + versionInfo.FileVersion);

            CfgReader config = new CfgReader(container);
            container.RegisterInstance<ICfgReader>(config);
                container.RegisterType<IIncomView, IncomView>();
                container.RegisterType<IIncomViewModel, IncomViewModel>();
                log.Info("Incom: IncomModule ActivateViewsWithRegions() start");
                viewManager.ViewsByRegionName["InteractionVoiceCustomButtonRegion"].Add
                        (new ViewActivator() { ViewType = typeof(IIncomView), ViewName = "IncomView", ActivateView = true });
                log.Info("Incom: IncomModule ActivateViewsWithRegions() end");

                UserEventListener userEventListener = new UserEventListener(container);
                container.RegisterInstance<IUserEventListener>(userEventListener);

                string host = config.IncomOptions.GetAsString("incom.voipsniffer_host");

                    //if (configManager.Task("InteractionWorkspace.incom.canUse") == true)
                    //{
                    //    log.Debug("Incom View is enabled");
                    //    container.RegisterType<IncomView, IncomView>();
                    //    log.Debug(String.Format("{0} container.RegisterType<IIincomaView, IncomView>();", this.log));
                    //    container.RegisterType<IIncomViewModel, IncomViewModel>();
                    //    log.Debug(String.Format("{0} container.RegisterType<IIncomViewModel, IncomViewModel>();", this.log));
                    //   // Put the Incom view in the region "InteractionWorksheetRegion"
                    //    viewManager.ViewsByRegionName["InteractionWorksheetRegion"].Add(
                    //            new ViewActivator() { ViewType = typeof(IIncomView), ViewName = "MyInteractionSample", ActivateView = true });
                    // //   Here we register the view(GUI) "IMySampleButtonView"
                    //    container.RegisterType<IIncomButtonView, IncomButtonView>();
                    //   // Put the MySampleMenuView view in the region "CaseViewSideButtonRegion"(The case toggle button in the interaction windows)
                    //    viewManager.ViewsByRegionName["CaseViewSideButtonRegion"].Add(
                    //        new ViewActivator() { ViewType = typeof(IIncomButtonView), ViewName = "MySampleButtonView", ActivateView = true });
                    //}
                    ///////////////////////
            RegisterCommands();
            log.Info("Incom: IncomModule Initialize finished");
            }
            catch (Exception ex)
            {
                log.Error ("Incom: IncomModule Initialize error: " + ex.Message);
            }
        }
        private void RegisterCommands()
        {
            log.Info("Incom: IncomModule RegisterCommands() start");
            List<CommandActivatorData> customCommandList = new List<CommandActivatorData>();
            customCommandList.Add(new CommandActivatorData() { chain = "MediaVoiceLogOn", afterCommand = "LogOn ", type = typeof(AfterMediaVoiceLogOnCommand), name = "AfterMediaVoiceLogOnCommand" });
            foreach (CommandActivatorData thisCommand in customCommandList)
            {
                this.log.Info(String.Format("{0} Inserting custom command [{1}] in command chain", this.log, thisCommand.name));
                IList<CommandActivator> insertedList = new List<CommandActivator>();
                insertedList.Add(new CommandActivator() { CommandType = thisCommand.type, Name = thisCommand.name });
                this.commandManager.InsertCommandToChainOfCommandAfter(thisCommand.chain, thisCommand.afterCommand, insertedList);
            }
            log.Info("Incom: IncomModule RegisterCommands() end");
        }
    }
}
