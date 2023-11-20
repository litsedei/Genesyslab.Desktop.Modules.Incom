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
		public DispatcherTimer timer = new DispatcherTimer();

        //incom  config wde
        bool startAutomaticAuthentication = true;
        bool agentCanAuthVP = true;  //wde role
        bool agentCanCreateVP = true; //wde role
        bool agentCanDeleteVP = true;  //wde role 
        string channelType = "IN"; //callType
        int vbioRecordingAllowed = 1; //UserData
        string agentId = "99agentId";
        string phoneNumber = "99011234567"; //getinfo response
        string cuid = "6886537";            //getinfo response
        public string callUUID = "998U87J5FGADL3QTR0F0U2LAES00RGLF";
        public string callToken = "99ae82e9-9cf8-467a-b900-a0a344db3d36"; //verify response errorinfo
        bool isRecordStarted = false;
        int httpTimeout = 1000;

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
                            EventEstablished ee = (EventEstablished)mes; callUUID = ee.CallUuid;
                        }
                        if (mes.Name == "EventDialing")
                        {
                            EventDialing ed = (EventDialing)mes; callUUID = ed.CallUuid;
                        }
                        phoneNumber = iv.PhoneNumber;
                        if (iv.GetAllAttachedData().ContainsKey("CUID"))
                        {
                            cuid = iv.ExtractAttachedData().GetAsString("CUID");
                            //    //iv.ExtractAttachedData().GetAsString("PHONETYPE"); //="PRIMARY_MOBILE"
                        }

                        if (vbioRecordingAllowed ==1)
                        {
                            log.Info("Incom: vbioRecordingAllowed: " + vbioRecordingAllowed.ToString());
                            httpTimeout=Convert.ToInt32(cfgReader.GetMainConfig("voipsniffer_timeout"));
                            log.Info("Incom: agentCanAuthVP: " + agentCanAuthVP + " ,agentCanCreateVP: " + agentCanCreateVP + " ,agentCanDeleteVP: " + agentCanDeleteVP);
                            btn_bio_verify.IsEnabled = agentCanAuthVP ? true : false;
                            btn_bio_create.IsEnabled = agentCanCreateVP ? true : false;
                            btn_bio_delete.IsEnabled = agentCanDeleteVP ? true : false;

                            var getInfoResponse = biometry_getInfo_exec(cuid,phoneNumber);
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
                                    AuthVB(GetTimer("auth"));
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
                                var callReponseCreate = biometry_callRecord_exec("RECORD", callUUID,channelType,phoneNumber);
                                callToken = callReponseCreate.businessInfo.callToken.ToString();

                                if (callReponseCreate.errorInfo.description.ToString() == "recording_started" && callReponseCreate.errorInfo.code==0)
                                {
                                    var callReponseQ = biometry_call_exec("QUALITY", callUUID,channelType,phoneNumber,callToken);
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
                btn_bio_verify.IsEnabled = false;
                AuthVB(GetTimer("auth"));
            }
            catch (Exception ex)
            {
                log.Error("Incom: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }
    }
}
