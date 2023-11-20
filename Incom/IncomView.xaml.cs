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
        private int bioScore = 0;
		public DispatcherTimer timer = new DispatcherTimer();
		int tick = 4;

        //incom  config wde
        int minSpeechLenCreate = 24;
        int minSpeechLenAuthentification = 20;
        int lowProb = 30; //Convert.ToInt32(cfgReader.GetMainConfig("waitAuthentificationCheck"));
        int highProb = 90;//Convert.ToInt32(cfgReader.GetMainConfig("waitAuthentificationCheck"));
        bool startAutomaticAuthentication = true;
        bool agentCanAuthVP = true;  //wde role
        bool agentCanCreateVP = true; //wde role
        bool agentCanDeleteVP = true;  //wde role 
        string channelType = "IN"; //callType
        int vbioRecordingAllowed = 1; //UserData
        string agentId = "agentId";
        string phoneNumber = "99011234567"; //getinfo response
        string cuid = "9986537";            //getinfo response
        public string callUUID = "998U87J5FGADL3QTR0F0U2LAES00RGLF";
        public string callToken = "99ae82e9-9cf8-467a-b900-a0a344db3d36"; //verify response errorinfo
        public bool isRecordingStarted = false;
        double len = 24.00275; //verify response errorinfo
        double speechLen = 24.0; //verify response errorinfo
        double prob = 95.989999; //verify response bussinessinfo

        public IncomView(IIncomViewModel incomViewModel, IObjectContainer container, IConfigManager configManager, ILogger log, ICfgReader cfgReader)
        {
            this.Model = incomViewModel;
            this.container = container;
            this.configManager = container.Resolve<IConfigManager>();
            this.log = log.CreateChildLogger("IncomView");
            this.cfgReader=container.Resolve<ICfgReader>();
        InitializeComponent();
        }
        public IIncomViewModel Model
        {
            get { return this.DataContext as IIncomViewModel; }
            set { this.DataContext = value; 
            }
        }
        public object Context { get; set; }
        //private void voice_stackPanel_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //client.Options.BaseUrl
        // //   userEventListener = new UserEventListener(this.container);
        //    IDictionary<string, object> contextDisctionary = Context as IDictionary<string, object>;
        //    object caseView;
        //    contextDisctionary.TryGetValue("CaseView", out caseView);
        //    object caseObject;
        //    contextDisctionary.TryGetValue("Case", out caseObject);
        //    ICase @case = caseObject as ICase;
        //    if (@case != null)
        //    {
        //        log.Info("create incom view");

        //        interaction = @case.MainInteraction as IInteraction; // MessageBox.Show("int " + interaction);
        //        if (interaction != null && interaction.Type != null && interaction.Type == "InteractionVoice")
        //        {
        //            log.Info("IncomCreate: " + interaction.Type.ToString());
        //            IInteractionVoice iv = interaction as IInteractionVoice;
        //            agentId = iv.Agent.UserName;
        //            IMessage mes = iv.EntrepriseLastInteractionEvent as IMessage;
        //         //   MessageBox.Show(mes.Name.ToString()+mes.Contains("CallUuid"));

        //            if (mes.Name== "EventEstablished")
        //            {
        //                EventEstablished ee = (EventEstablished)mes;  callUUID = ee.CallUuid;
        //            }
        //            if (mes.Name== "EventDialing")
        //            {
        //                EventDialing ed = (EventDialing)mes; callUUID = ed.CallUuid;
        //            }
        //            if (true)
        //            {
        //            }
        //            phoneNumber = iv.PhoneNumber;
        //            // btn_bio_create.IsEnabled = userEvent.isCreateEnable;
        //            //      string callType = iv.GetIWCallType();
        //            //  string calluuid = iv.AttachedDataInformation.
        //            if (iv.GetAllAttachedData().ContainsKey("CUID"))
        //            {
        //                cuid=iv.ExtractAttachedData().GetAsString("CUID");
        //                //    //iv.ExtractAttachedData().GetAsString("PHONETYPE"); //="PRIMARY_MOBILE"
        //            }

        //            if (iv.ExtractAttachedData().GetAsString("cosentVoice") != "0")
        //            {
        //                //btn_bio_create.IsEnabled = userEvent.isCreateEnable;
        //                btn_bio_delete.IsEnabled = true;
        //                btn_bio_virefy.IsEnabled = true;
        //            }
        //        }
        //    }
        //}
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
                        //   MessageBox.Show(mes.Name.ToString()+mes.Contains("CallUuid"));

                        if (mes.Name == "EventEstablished")
                        {
                            EventEstablished ee = (EventEstablished)mes; callUUID = ee.CallUuid;
                        }
                        if (mes.Name == "EventDialing")
                        {
                            EventDialing ed = (EventDialing)mes; callUUID = ed.CallUuid;
                        }
                        phoneNumber = iv.PhoneNumber;
                        // btn_bio_create.IsEnabled = userEvent.isCreateEnable;
                        //      string callType = iv.GetIWCallType();
                        //  string calluuid = iv.AttachedDataInformation.
                        if (iv.GetAllAttachedData().ContainsKey("CUID"))
                        {
                            cuid = iv.ExtractAttachedData().GetAsString("CUID");
                            //    //iv.ExtractAttachedData().GetAsString("PHONETYPE"); //="PRIMARY_MOBILE"
                        }

                        if (vbioRecordingAllowed)
                        {
                            log.Info("Incom: vbioRecordingAllowed: " + vbioRecordingAllowed.ToString());
                            log.Info("Incom: agentCanAuthVP: " + agentCanAuthVP + " ,agentCanCreateVP: " + agentCanCreateVP + " ,agentCanDeleteVP: " + agentCanDeleteVP);
                            btn_bio_virefy.IsEnabled = agentCanAuthVP ? true : false;
                            btn_bio_create.IsEnabled = agentCanCreateVP ? true : false;
                            btn_bio_delete.IsEnabled = agentCanDeleteVP ? true : false;


                            GetInfoReq getInfoReqBody = new GetInfoReq();
                            getInfoReqBody.cuid = cuid;
                            getInfoReqBody.phoneNumber = phoneNumber;
                            string getinfoReq = JsonConvert.SerializeObject(getInfoReqBody);
                            // string getinfoReq = JsonSerializer.Serialize(getInfoReqBody);
                            var getInfoResponse = biometry_getInfo_exec(getinfoReq);
                            if (getInfoResponse.errorInfo.code.ToString() == "1" && getInfoResponse.errorInfo.description.ToString() == "Ошибка. Найдено несколько записей")
                            {
                                lbl_bio_status.Text = "Ошибка. Найдено несколько записей";
                            }
                            if (getInfoResponse.businessInfo.isVoiceId == true && getInfoResponse.businessInfo.disableRecording == false && getInfoResponse.errorInfo.code.ToString() == "0")
                            {
                                btn_bio_create.IsEnabled = false;
                                // agentCanAuthVP = true ? btn_bio_virefy.IsEnabled = true : btn_bio_virefy.IsEnabled = false;
                                // agentCanDeleteVP = true ? btn_bio_delete.IsEnabled = true : btn_bio_delete.IsEnabled = false;
                                btn_bio_delete.IsEnabled = agentCanDeleteVP ? true : false;
                                lbl_bio_status.Text = "Голосовой слепок найден";
                                if (startAutomaticAuthentication)
                                {
                                    btn_bio_virefy.IsEnabled = false;
                                    AuthVB(GetTimer("auth"));
                                }
                            }
                            if (getInfoResponse.businessInfo.isVoiceId == false && getInfoResponse.businessInfo.disableRecording == true && getInfoResponse.errorInfo.code.ToString() == "0")
                            {
                                btn_bio_create.IsEnabled = false;
                                btn_bio_virefy.IsEnabled = false;
                                btn_bio_delete.IsEnabled = false;
                                lbl_bio_status.Text = "Запись слепка запрещена";
                            }
                            if (getInfoResponse.businessInfo.isVoiceId == false && getInfoResponse.businessInfo.disableRecording == false)
                            {
                                btn_bio_virefy.IsEnabled = false;
                                btn_bio_delete.IsEnabled = false;
                                btn_bio_create.IsEnabled = false;
                                lbl_bio_status.Text = "Голосовой слепок отсутствует";
                                CallReqRecord callReq = new CallReqRecord();
                                callReq.requestType = "RECORD";
                                callReq.callUUID = callUUID;
                                callReq.channelType = channelType;
                                callReq.phoneNumber = phoneNumber;

                                string bodyCall = JsonConvert.SerializeObject(callReq);
                                //string bodyCall = JsonSerializer.Serialize(callReq);
                                var callReponseCreate = biometry_call_exec(bodyCall);
          //                      callToken = callReponseCreate.businessInfo.callToken.ToString();

                                if (callReponseCreate.errorInfo.description.ToString() == "recording_started" && callReponseCreate.errorInfo.code.ToString() == "0")
                                {
                                    CallReq callReqQ = new CallReq();
                                    callReqQ.requestType = "QUALITY";
                                    callReqQ.callUUID = callUUID;
                                    callReqQ.channelType = channelType;
                                    callReqQ.callToken = callToken;
                                    callReqQ.phoneNumber = phoneNumber;
                                    string bodyCallQ = JsonConvert.SerializeObject(callReqQ);
                                    // string bodyCallQ = JsonSerializer.Serialize(callReqQ);
                                    var callReponseQ = biometry_call_exec(bodyCallQ);
                                    lbl_bio_status.Text = callReponseQ.errorInfo.description.ToString();
                                    CreateVB(GetTimer("create"));
                                }
                            }
                        }
                    } 
                }
            }
            catch (Exception ex)
            {
                log.Error("Incom: "+System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }

        public void Create()
		{
            // IDictionary<string, object> contextDictionary = Context as IDictionary<string, object>;
            // HelperToolbarFramework.SetButtonStyle(contextDictionary, Model);
            //myAgent = container.Resolve<IAgent>();
            //cfgReader = container.Resolve<ICfgReader>();
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
                    log.Info("Incom biometry Auth timer stoped");
                }
                if (BiometryTimerCreate.IsEnabled)
                {
                    BiometryTimerCreate.Stop();
                    log.Info("Incom biometry Create timer stoped");
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception ex)
            {

                log.Error("Incom: "+System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
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

        private void btn_bioVerify_click(object sender, RoutedEventArgs e)
        {

            try
            {
                lbl_bio_status.Text = null;
                btn_bio_virefy.IsEnabled = false;
                AuthVB(GetTimer("auth"));
            }
            catch (Exception ex)
            {
                log.Error("Incom: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }
    }
}
