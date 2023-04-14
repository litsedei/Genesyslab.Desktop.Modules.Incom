// Copyright © 2017 Genesys. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesyslab.Desktop.Infrastructure.Commands;
using Genesyslab.Desktop.Infrastructure.DependencyInjection;
//using Genesyslab.Desktop.Modules.Gms.CallbackInvitation.Generic;
using Genesyslab.Platform.Commons.Logging;
using System.Windows;
using System.Windows.Threading;
using Genesyslab.Desktop.Modules.Core.Model.Agents;
using Genesyslab.Enterprise.Model.Device;
//using Genesyslab.Desktop.Modules.Gms.CallbackInvitation.CallbackInvitation;
using System.Collections.ObjectModel;
using Genesyslab.Desktop.Modules.Voice.Model.Agents;
using Genesyslab.Desktop.Modules.Incom.CrmConfgReader;
using Genesyslab.Platform.ApplicationBlocks.ConfigurationObjectModel.CfgObjects;
using Genesyslab.Desktop.Modules.Incom.DispositionCodeEx;

namespace Genesyslab.Desktop.Modules.Gms.CallbackInvitation.CustomCommands {
    class AfterMediaVoiceLogOnCommand : IElementOfCommand {
        private readonly IObjectContainer container;
        private readonly ICfgReader config;
        private readonly ILogger log;
        private string _commandName = "AfterMediaVoiceLogOnCommand";

        public AfterMediaVoiceLogOnCommand(IObjectContainer container) {
            this.container = container;
            this.config = this.container.Resolve<ICfgReader>();


            // Initialize the trace system
            this.log = this.container.Resolve<ILogger>().CreateChildLogger(this.Name);
        }

        #region IElementOfCommand Members

        public string Name {
            get { return this._commandName; }
            set { if (this._commandName != value) { this._commandName = value; } }
        }

        public bool Execute(IDictionary<string, object> parameters, IProgressUpdater progress) {
            // To go to the main thread
            if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess()) {
                object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new ExecuteDelegate(Execute), parameters, progress);
                return (bool)result;
            }
            else {
                // Ok, we are in the main thread
                this.log.Info(String.Format("{0} Starting custom {1} functionality", this.log, this.Name));

                IAgent agent = this.container.Resolve<IAgent>();
                List<String> loginDNs = agent.LoginDNs;
                ObservableCollection<IMedia> agentMedias = agent.Place.ListOfMedia;
                IMediaVoice mediaVoice = null;
                String agentDN = "";
                if (loginDNs.Count > 0)
                {
                    agentDN = loginDNs[0].Substring(0, loginDNs[0].IndexOf('@'));
                    this.log.Info(String.Format("{0} Agent [{1}] is logged into [{2}]. Extracted DN = [{3}]", this.log, agent.UserName, loginDNs[0], agentDN));
                }
                List<String> alreadyProcessedDNList = new List<String>();
                foreach (var media in agentMedias)
                {
                    // get only media voice, cast into the mediavoice, result can be null
                    IMediaVoice testMediaVoice = media as IMediaVoice;
                    if (testMediaVoice != null && !testMediaVoice.IsOutOfService) // && !testMediaVoice.IsLogOff) 
                    {
                        this.log.Info(String.Format("{0} current Media is {1} ", this.log, testMediaVoice.LongName));
                        // get the right media !!!
                        mediaVoice = testMediaVoice;
                        this.log.Info(String.Format("{0} current Media IsLogOff is {1} ", this.log, testMediaVoice.IsLogOff));
                        this.log.Info(String.Format("{0} current Media IsOutOfService is {1} ", this.log, testMediaVoice.IsOutOfService));
                        this.log.Info(String.Format("{0} register and listen to mediaVoice {1}", this.log, mediaVoice.Name));
                        // try to register this listener for this DN, nly one time ?!
                        if (! alreadyProcessedDNList.Contains(mediaVoice.SwitchName)) {
                            // check that there is not already an instance in the container
                            IUserEventListener userEventListener = null;
                            foreach (var aUserEventListener in this.container.ResolveAll<IUserEventListener>())
                            {
                                if (aUserEventListener.MediaVoice.LongName.Equals(mediaVoice.LongName))
                                {
                                    // that's the good one and there can only be one
                                    userEventListener = aUserEventListener;
                                }
                            }

                            // if it does not exist, create it
                            if (userEventListener == null)
                            {
                                // Set up the listener to the DN
                                userEventListener = new UserEventListener(this.container);
                                userEventListener.AgentDN = agentDN;
                                userEventListener.MediaVoice = mediaVoice;
                                userEventListener.SetupHandler();
                                // we register the DN and add this listener instance into the container
                                this.container.RegisterInstance<IUserEventListener>(mediaVoice.Name, userEventListener);
                                // add this switch as already processed ?! (not sure that this is the right way to do it)
                                alreadyProcessedDNList.Add(mediaVoice.SwitchName);
                                this.log.Info(String.Format("{0} final Media is {1} ", this.log, mediaVoice.Name));
                            }
                            else
                            {
                                // exist in container
                                // Set up the listener to the DN, the listener on DN could have been removed
                                // so activate it again
                                userEventListener.AgentDN = agentDN;
                                userEventListener.MediaVoice = mediaVoice;
                                userEventListener.SetupHandler();
                                // add this switch as already processed ?! (not sure that this is the right way to do it)
                                alreadyProcessedDNList.Add(mediaVoice.SwitchName);
                                this.log.Info(String.Format("{0} final Media is {1} ", this.log, mediaVoice.Name));
                            }
                        }
                        else
                        {
                            // do not register again
                            this.log.Info(String.Format("{0} already registered and listened to mediaVoice {1}", this.log, mediaVoice.Name));
                        }
                    }
                }
                this.log.Info(String.Format("{0} Finished custom {1} functionality", this.log, this.Name));

                // Always allow remaining elements in the command chain to execute: set return value to false
                return false;
            }
        }

        /// <summary>
        /// This delegate allows to go to the main thread.
        /// </summary>
        delegate bool ExecuteDelegate(IDictionary<string, object> parameters, IProgressUpdater progressUpdater);
        #endregion
    }
}
