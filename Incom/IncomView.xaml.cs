using System;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Genesyslab.Desktop.Modules.Incom.IncomConfgReader;
using Genesyslab.Desktop.Infrastructure.Configuration;
using Genesyslab.Desktop.Infrastructure.DependencyInjection;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Platform.Commons.Collections;
using Genesyslab.Platform.Commons.Protocols;
using Genesyslab.Platform.Commons.Threading;
//using Genesyslab.Desktop.Modules.Incom.IncomConfgReader;
using Genesyslab.Desktop.Modules.Core;
using Genesyslab.Desktop.Modules.Core.Configurations;
using Genesyslab.Desktop.Modules.Core.Model.Interactions;
using Genesyslab.Desktop.Modules.Voice.Model.Interactions;
using Genesyslab.Platform.Voice.Protocols.TServer.Events;
using System.Collections.Generic;
using Genesyslab.Platform.Voice.Protocols.TServer;
using Genesyslab.Desktop.Modules.Windows.Views.Common.Editor;
using Genesyslab.Desktop.Modules.Windows.Views.Interactions.InteractionToolbar;
//using Genesyslab.Enterprise.Model.Interaction;
using Genesyslab.Desktop.Modules.Incom.JsonSerializing;
using RestSharp;
using System.Text.Json.Serialization;
using System.Text.Json;
using Genesyslab.Desktop.Modules.Voice.Model.Agents;
using Genesyslab.Platform.Commons.Connection;
using IMessage = Genesyslab.Platform.Commons.Protocols.IMessage;
using Genesyslab.Desktop.Modules.Core.Model.Agents;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Threading;

namespace Genesyslab.Desktop.Modules.Incom.IncomUI
{
    public partial class IncomView : UserControl, IIncomView
    {
        readonly IObjectContainer container;
        private readonly ILogger log;
        readonly IConfigManager configManager;
        private IInteraction interaction;
        private IIncomView incomView;
        private ICfgReader cfgReader;
        public DispatcherTimer timer = new DispatcherTimer();

        //incom  config wde
        bool startAutomaticAuthentication = false;
        bool agentCanAuthVP = true;  //wde role
        bool agentCanCreateVP = true; //wde role
        bool agentCanDeleteVP = true;  //wde role 
        string channelType = "IN"; //callType
        int vbioRecordingAllowed = 1; //UserData
        string agentId = "99agentId";
        string phoneNumber = "99011234567"; //getinfo response
        string cuid = "6886537";            //getinfo response
        public string callUUID = "998U87J5FGADL3QTR0F0U2LAES00RGLF";
        public string callToken = "afb97bd0-0c89-6c11-c3d4-afb5e27eb6b7"; //verify response errorinfo
       // bool isRecordStarted = false;
        int httpTimeout = 1000;
        bool btn_cr_state = false;
        string recordState;

        public IncomView(IIncomViewModel incomViewModel, IObjectContainer container, IConfigManager configManager, ILogger log, ICfgReader cfgReader)
        {
            this.Model = incomViewModel;
            this.container = container;
            this.configManager = container.Resolve<IConfigManager>();
            this.log = log.CreateChildLogger("IncomView");
            this.cfgReader = container.Resolve<ICfgReader>();
            InitializeComponent();
        }
        public IIncomViewModel Model
        {
            get { return this.DataContext as IIncomViewModel; }
            set { this.DataContext = value;
            }
        }
        public object Context { get; set; }
        //            if (iv.GetAllAttachedData().ContainsKey("CUID"))
        //            {
        //                cuid=iv.ExtractAttachedData().GetAsString("CUID");
        //                //    //iv.ExtractAttachedData().GetAsString("PHONETYPE"); //="PRIMARY_MOBILE"
        //            }
        private void voice_stackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IDictionary<string, object> contextDisctionary = Context as IDictionary<string, object>;
                object caseView;
                contextDisctionary.TryGetValue("CaseView", out caseView);
                object caseObject;
                contextDisctionary.TryGetValue("Case", out caseObject);
                ICase @case = caseObject as ICase;
                if (@case != null)
                {
                    log.Info("Incom: create incom view");

                    interaction = @case.MainInteraction as IInteraction; // MessageBox.Show("int " + interaction);
                    if (interaction != null && interaction.Type != null && interaction.Type == "InteractionVoice")
                    {
                        log.Info("IncomCreate: " + interaction.Type.ToString());
                        IInteractionVoice iv = interaction as IInteractionVoice;
                        agentId = iv.Agent.UserName;

                        IMessage mes = iv.EntrepriseLastInteractionEvent as IMessage;
                        if (mes.Name == "EventEstablished")
                        {
                            EventEstablished ee = (EventEstablished)mes;
                            callUUID = ee.CallUuid;
                        }

                        if (mes.Name == "EventDialing")
                        {
                            EventDialing ed = (EventDialing)mes; callUUID = ed.CallUuid;
                        }
                        if (mes.Name == "EventReleased")
                        {
                            if (BiometryTimerAuth.IsEnabled)
                            {
                                BiometryTimerAuth.Stop();
                                log.Info("Incom: biometry Auth timer stoped");
                            }
                            if (BiometryTimerCreate.IsEnabled)
                            {
                                BiometryTimerCreate.Stop();
                                log.Info("Incom: biometry Create timer stoped");
                            }
                        }
                        phoneNumber = iv.PhoneNumber;
                        if (iv.GetAllAttachedData().ContainsKey("CUID"))
                        {
                            cuid = iv.ExtractAttachedData().GetAsString("CUID");
                            //    //iv.ExtractAttachedData().GetAsString("PHONETYPE"); //="PRIMARY_MOBILE"
                        }
                        if (iv.GetAllAttachedData().ContainsKey("callToken"))
                        {
                            callToken = iv.ExtractAttachedData().GetAsString("callToken");
                            //    //iv.ExtractAttachedData().GetAsString("PHONETYPE"); //="PRIMARY_MOBILE"
                        }
                        if (vbioRecordingAllowed == 1)
                        {
                            log.Info("Incom: vbioRecordingAllowed: " + vbioRecordingAllowed.ToString());
                            httpTimeout = Convert.ToInt32(cfgReader.GetMainConfig("voipsniffer_timeout"));
                            log.Info("Incom: agentCanAuthVP: " + agentCanAuthVP + " ,agentCanCreateVP: " + agentCanCreateVP + " ,agentCanDeleteVP: " + agentCanDeleteVP);
                            btn_bio_verify.IsEnabled = agentCanAuthVP ? true : false;
                            btn_bio_create.IsEnabled = agentCanCreateVP ? true : false;
                            btn_bio_delete.IsEnabled = agentCanDeleteVP ? true : false;
                            var getInfoResponse = biometry_getInfo_exec(cuid, phoneNumber);
                            //callToken = getInfoResponse.businessInfo.callToken.ToString();
                            if (getInfoResponse.errorInfo.code == 1 && getInfoResponse.errorInfo.description.ToString() == "found_too_many_voicetemplates")
                            {
                                lbl_bio_status.Text = "Ошибка. Найдено несколько записей";
                            }
                            if (getInfoResponse.businessInfo.isVoiceId == true && getInfoResponse.businessInfo.disableRecording == 0 && getInfoResponse.errorInfo.code == 0)
                            {
                                btn_bio_create.IsEnabled = false;
                                // agentCanAuthVP = true ? btn_bio_virefy.IsEnabled = true : btn_bio_virefy.IsEnabled = false;
                                // agentCanDeleteVP = true ? btn_bio_delete.IsEnabled = true : btn_bio_delete.IsEnabled = false;
                                btn_bio_delete.IsEnabled = agentCanDeleteVP ? true : false;
                                lbl_bio_status.Text = "Голосовой слепок найден";
                                if (startAutomaticAuthentication)
                                {
                                    btn_bio_verify.IsEnabled = false;
                                    AuthVB(GetTimer("auth"), channelType, "auto");
                                }
                            }
                            if (getInfoResponse.businessInfo.isVoiceId == false && getInfoResponse.businessInfo.disableRecording == 1 && getInfoResponse.errorInfo.code == 0)
                            {
                                btn_bio_create.IsEnabled = false;
                                btn_bio_verify.IsEnabled = false;
                                btn_bio_delete.IsEnabled = false;
                                lbl_bio_status.Text = "Запись слепка запрещена";
                            }
                            if (getInfoResponse.businessInfo.isVoiceId == false && getInfoResponse.businessInfo.disableRecording == 0)
                            {
                                btn_bio_verify.IsEnabled = false;
                                btn_bio_delete.IsEnabled = false;
                                btn_bio_create.IsEnabled = false;
                                lbl_bio_status.Text = "Голосовой слепок отсутствует";
                                if (channelType == "IN")
                                {
                                    var callReponseCreate = biometry_callRecord_exec("RECORD", callUUID, channelType, phoneNumber, "0");
                                    log.Info("CallToken FROM VIEW");
                                    callToken = callReponseCreate.businessInfo.callToken.ToString();
                                    log.Info("incomView.SetCallToken FROM VIEW");
                                    incomView.SetCallToken(callReponseCreate.businessInfo.callToken.ToString());
                                    recordState = callReponseCreate.errorInfo.description.ToString();
                                }

                                if (recordState == "recording_started")
                                {
                                    var callReponseQ = biometry_call_exec("QUALITY", callUUID, channelType, phoneNumber, callToken);
                                    lbl_bio_status.Text = callReponseQ.errorInfo.description.ToString();
                                    CreateVB(GetTimer("create"), channelType);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Incom: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }

        public void Create()
        {
            // IDictionary<string, object> contextDictionary = Context as IDictionary<string, object>;
            // HelperToolbarFramework.SetButtonStyle(contextDictionary, Model);
            //  Model.MyCollection = collection;
            //  Model.btnCreateState = btn_cr_state;
        }
        /// <summary>
        /// Destroys this instance.
        /// </summary>
        public void Destroy()
        { log.Info("Incom: IncomView is normaly destroyed");
            try
            {
                if (BiometryTimerAuth.IsEnabled)
                {
                    BiometryTimerAuth.Stop();
                    log.Info("Incom: biometry Auth timer stoped");
                }
                if (BiometryTimerCreate.IsEnabled)
                {
                    BiometryTimerCreate.Stop();
                    log.Info("Incom: biometry Create timer stoped");
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception ex)
            {

                log.Error("Incom: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }


        }
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

        void IIncomView.EventEstablishedListener(IMessage userEvent)
        {
            EventEstablishedListener(userEvent);
        }

        
        

        void IIncomView.EventReleasedListner(IMessage userEvent)
        {
            EventReleasedListner(userEvent);
        }
        private void EventEstablishedListener(IMessage imessage)  //Id64 - EventEstablished
        {
            
            try
            {
                EventEstablished ee = (EventEstablished)imessage;
                Platform.Commons.Collections.KeyValueCollection eeUserData = ee.UserData as Platform.Commons.Collections.KeyValueCollection;
                IInteractionVoice iv = interaction as IInteractionVoice;
                log.Debug("Incom: EventEstablishedListener CallUUID: " + ee.CallUuid);
                callUUID = ee.CallUuid.ToString().ToUpper();
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Enterprise.Commons.Collections.KeyValueCollection userData = new Enterprise.Commons.Collections.KeyValueCollection();
                if (ee.CallType.ToString() == "Outbound")
                {
                    channelType = "OUT";
                    var callReponseCreate = biometry_callRecord_exec("RECORD", callUUID, channelType, ee.DNIS,"1");
                    if (callReponseCreate.errorInfo.code!=0)
                    {
                        while (callReponseCreate.errorInfo.code == 0)
                        {
                            System.Threading.Thread.Sleep(2000);
                            callReponseCreate = biometry_callRecord_exec("RECORD", callUUID, channelType, ee.DNIS, "1");
                            // iv.UserData.Add("callToken", callReponseCreate.businessInfo.callToken.ToString());
                            incomView.SetCallToken(callReponseCreate.businessInfo.callToken.ToString());
                        }
                    }
                    log.Debug("Incom: EventEstablishedListener: (CallUUID " + callUUID + " DNIS "+ee.DNIS);
                    iv.UserData.Add("callToken", callReponseCreate.businessInfo.callToken.ToString());
                }

                if (ee.CallType.ToString() == "Inbound" || ee.CallType.ToString() == "Internal")
                {
                    channelType = "IN";
                    log.Debug("Incom: EventEstablishedListener: server call type inbound || internal. Calltype: " + ee.CallType.ToString() + " ANI " + ee.ANI);
                   var callReponseCreate = biometry_callRecord_exec("RECORD", callUUID, channelType, ee.ANI, "0");
                    if (callReponseCreate.errorInfo.code != 0)
                    {   
                        while (callReponseCreate.errorInfo.code==0) 
                        {
                            System.Threading.Thread.Sleep(2000);
                            callReponseCreate = biometry_callRecord_exec("RECORD", callUUID, channelType, ee.DNIS, "0");
                            iv.UserData.Add("callToken", callReponseCreate.businessInfo.callToken.ToString());
                        }
                    }
                    iv.UserData.Add("callToken",callReponseCreate.businessInfo.callToken.ToString());
                    parameters.Add("UserData", userData);
                }
               
            }
            catch (Exception ex)
            {
                log.Debug("Incom: EventEstablishedListener ex " + ex.Message + " " + ex.InnerException);
            }
        }

        private void EventReleasedListner(IMessage imessage)
        {
            if (BiometryTimerAuth.IsEnabled)
            {
                BiometryTimerAuth.Stop();
                log.Info("Incom: biometry Auth timer stoped");
            }
            if (BiometryTimerCreate.IsEnabled)
            {
                BiometryTimerCreate.Stop();
                log.Info("Incom: biometry Create timer stoped");
            }
        }
    }
}
