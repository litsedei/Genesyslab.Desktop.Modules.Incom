using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesyslab.Desktop.Infrastructure.DependencyInjection;
using Genesyslab.Desktop.Modules.Core.SDK.Protocol;
using Genesyslab.Platform.ApplicationBlocks.Commons.Broker;
using Genesyslab.Platform.ApplicationBlocks.Commons.Protocols;
using Genesyslab.Platform.Commons.Protocols;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Platform.Voice.Protocols.TServer.Requests.Dn;
//using Genesyslab.Platform.Voice.Protocols.TServer.Events;
using Genesyslab.Platform.Commons.Collections;
//using Genesyslab.Desktop.Modules.Gms.CallbackInvitation.Generic;
using Genesyslab.Desktop.Modules.Incom.CrmConfgReader;
using System.Windows;
using System.Windows.Threading;
using Genesyslab.Desktop.Infrastructure.Commands;
using Genesyslab.Desktop.Modules.Voice.Model.Agents;
//using Genesyslab.Desktop.Modules.Gms.CallbackInvitation.CallbackDisposition;
using Genesyslab.Desktop.Modules.Incom.DispositionCodeEx;
using Genesyslab.Desktop.Modules.Incom;
//using Genesyslab.Desktop.Modules.ExtensionSample.MyCRM;
using Genesyslab.Platform.Voice.Protocols.TServer.Events;
using Genesyslab.Desktop.Modules.Voice.Model.Interactions;
using Genesyslab.Desktop.Modules.Core.Model.Interactions;

namespace Genesyslab.Desktop.Modules.Incom.DispositionCodeEx
{
    class UserEventListener: IUserEventListener {
        private IInteraction interaction;
        private readonly IObjectContainer container = null;
        private readonly ILogger log = null;
        private String _agentDN = "";
        private IMediaVoice _mediaVoice = null;

        public string agentId, placeId, cPhone, callid, proposId = "0", transactId = "0", callSTC = "0";
        private bool handlerRegistered = false;
        private readonly ICfgReader config = null;

        public UserEventListener(IObjectContainer container) {
            this.container = container;
            this.config = this.container.Resolve<ICfgReader>();
            this.log = this.container.Resolve<ILogger>().CreateChildLogger("UserEventListener");
        }
        private void EventHandlerTServerReceived(IMessage userEvent)
        {
            try
            {
                MessageBox.Show(userEvent.Name+ userEvent.Id);
            log.Info(userEvent.Id + userEvent.Name);
                    if (userEvent.Id.Equals(64))  //64 - EventEstablished
                    {
                      //  crmView.EventEstablishedListener(userEvent);
                }
                    if (userEvent.Id.Equals(65)) //EventReleased
                    {
                    // crmView.EventReleasedListner(userEvent);
                }
                    if (userEvent.Id.Equals(61)) //EventDialing
                    {
                        //-------------- WebcallbackStart------------///
                        EventDialing ed = (EventDialing)userEvent;
                        Platform.Commons.Collections.KeyValueCollection UserData = ed.UserData as Platform.Commons.Collections.KeyValueCollection;
                            if (UserData.Equals("GMS_Service_ID"))
                            {
                               UserData.GetValues("GMS_Service_ID").First().ToString();
                               DateTime utcDate = DateTime.Parse(UserData.GetAsString("GMS_Cb_Desired_Time"));
                               var localTime = utcDate.ToLocalTime();
                               string callid = ed.ConnID.ToString();
                               string wcbid = UserData.GetValues("_CB_SERVICE_ID").First().ToString();
                               string cPhone = UserData.GetAsString("GMS_Customer_Number");
                               string cType = UserData.GetAsString("GMS_Cb_Type");
                               string cServiceName = UserData.GetAsString("GMS_ServiceName");
                               string cCLName = UserData.GetAsString("firstname");
                               string cCFName = UserData.GetAsString("lastname");
                               string cSubject = UserData.GetAsString("subject");
                               string cServiceId = UserData.GetAsString("_CB_SERVICE_ID");
                               
                               string sctime = localTime.Hour.ToString() + localTime.Minute.ToString();
                            }
                        //------------WebcallbackEnd-----------------////
                    }
                    if (userEvent.Id.Equals(96))  //96 - Event96
                    {
                       // crmView.Event96Listener(userEvent);
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
                   _mediaVoice.Channel.RawSubscriber.Register(EventHandlerTServerReceived);
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
            MessageBox.Show((e as MessageEventArgs).Message.ToString(),sender.ToString());
            //EventHandlerTServerReceived((e as MessageEventArgs).Message);
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
        public String AgentDN {
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
