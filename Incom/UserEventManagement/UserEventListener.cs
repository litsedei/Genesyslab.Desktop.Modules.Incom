using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Genesyslab.Platform.Commons.Protocols;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Platform.Voice.Protocols.TServer.Events;
using Genesyslab.Platform.Voice.Protocols.TServer.Requests.Dn;
using Genesyslab.Platform.Commons.Collections;
using Genesyslab.Platform.ApplicationBlocks.Commons.Broker;
using Genesyslab.Platform.ApplicationBlocks.Commons.Protocols;
using Genesyslab.Desktop.Modules.Core.SDK.Protocol;
using Genesyslab.Desktop.Modules.Voice.Model.Agents;
using Genesyslab.Desktop.Modules.Voice.Model.Interactions;
using Genesyslab.Desktop.Modules.Core.Model.Interactions;
using Genesyslab.Desktop.Infrastructure.Commands;
using Genesyslab.Desktop.Infrastructure.DependencyInjection;
using Genesyslab.Desktop.Modules.Incom;
using Genesyslab.Desktop.Modules.Incom.UserEvenManagment;
using Genesyslab.Desktop.Modules.Incom.IncomUI;
using Genesyslab.Desktop.Modules.Incom.IncomConfgReader;
//using Genesyslab.Desktop.Modules.Incom.CustomCommands;
using Genesyslab.Desktop.Infrastructure.ViewManager;
using Genesyslab.Desktop.Modules.Windows.Event;
using Genesyslab.Platform.Commons.Connection;
using System.Web.UI.WebControls;


namespace Genesyslab.Desktop.Modules.Incom.UserEvenManagment
{
    class UserEventListener : IUserEventListener    {
        readonly IViewManager viewManager;
        private IInteraction interaction;
        private IIncomView incomView;
        private readonly IObjectContainer container = null;
        private readonly ILogger log = null;
        private String _agentDN = "";
        private IMediaVoice _mediaVoice = null;
        private bool handlerRegistered = false;
        private bool _isCreateEnabled;
        private bool _isDeleteEnabled;
        private bool _isVerifyEnabled;
        private readonly ICfgReader config = null;

        public UserEventListener(IObjectContainer container)
        {
            this.container = container;
            this.viewManager = this.container.Resolve <IViewManager> ();
            this.config = this.container.Resolve<ICfgReader>();
            this.incomView=this.container.Resolve<IIncomView> ();
            this.log = this.container.Resolve<ILogger>().CreateChildLogger("UserEventListener");
        }

        private void EventHandlerTServerReceived(IMessage userEvent)
        {
            try
            {
                log.Info(userEvent.Id + userEvent.Name);
                if (userEvent.Id.Equals(64))  //64 - EventEstablished
                {
                    log.Info("Incom. EventEstablished");
                    //    EventEstablished ee = (EventEstablished)userEvent;
                    //  Platform.Commons.Collections.KeyValueCollection UserData = ee.UserData as Platform.Commons.Collections.KeyValueCollection;
                    incomView.EventEstablishedListener(userEvent);
                    log.Info("Incom. EventEstablished after eventEst");
                }
                if (userEvent.Id.Equals(65)) //EventReleased
                {
                    incomView.EventReleasedListner(userEvent);
                }
                if (userEvent.Id.Equals(61)) //EventDialing
                {
                    EventDialing ed = (EventDialing)userEvent;
                    Platform.Commons.Collections.KeyValueCollection UserData = ed.UserData as Platform.Commons.Collections.KeyValueCollection;
                }
              
                this.log.Info(String.Format("{0} Ignoring event [{1}] from [{2}]", this.log, userEvent.Name, userEvent.Endpoint));
            }

            catch (Exception ex)
            {
                log.Error("EventHandlerTServerReceived ex: " + ex.Message + " " + ex.InnerException);
            }
        }
        public void SetupHandler()
        {
            this.log.Info(String.Format("{0} Setting up connection to T-Server", this.log));

            if (this._mediaVoice != null)
            {
                if (!handlerRegistered)
                {
                    // listen to all events from media voice
                    _mediaVoice.Channel.Protocol.Received += Protocol_Received;
                    this.log.Info(String.Format("{0} Registered for events sent to DN [{1}]", this.log, this._agentDN));
                    handlerRegistered = true;
                }
                // else already registered, do not register
            }
            else
                throw new Exception("T-Server connection cannot be set up: no Media Voice DN has been set");
        }
        void Protocol_Received(object sender, EventArgs e)
        {
           // MessageBox.Show((e as MessageEventArgs).Message.ToString(), sender.ToString());
            EventHandlerTServerReceived((e as MessageEventArgs).Message);
        }
        public void RemoveHandler()
        {
            this.log.Info(String.Format("{0} Remove connection to T-Server", this.log));

            if (this._mediaVoice != null)
            {
                // remove listener
                _mediaVoice.Channel.Protocol.Received -= Protocol_Received;
                _mediaVoice.Channel.RawSubscriber.Unregister(EventHandlerTServerReceived);
                this.log.Info(String.Format("{0} Un-Registered for events sent to DN [{1}]", this.log, this._agentDN));
                handlerRegistered = false;
            }
            else
                throw new Exception("T-Server connection cannot be set up: no Media Voice DN has been set");
        }
        public String AgentDN
        {
            get { return this._agentDN; }
            set { if (this._agentDN != value) { this._agentDN = value; } }
        }
        public IMediaVoice MediaVoice
        {
            get { return this._mediaVoice; }
            set { if (this._mediaVoice != value) { this._mediaVoice = value; } }
        }
        /// <summary>
        /// This delegate allows to go to the main thread.
        /// </summary>
        delegate bool ExecuteDelegate(IDictionary<string, object> parameters, IProgressUpdater progressUpdater);
    }
}
