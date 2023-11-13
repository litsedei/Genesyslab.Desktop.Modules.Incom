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


        //private static readonly string getInfoPath = "/api/v1/biometry/getInfo";
        //private static readonly string verifypath = "/api/v1/biometry/voice/verify";
        //private static readonly string createPath = "/api/v1/biometry/voice/create";
        //private static readonly string deletePath = "/api/v1/biometry/voice/delete";
        //private static readonly string callPath = "/api/v1/biometry/call";


        private static readonly string getInfoPath = "/getinfo";
        private static readonly string verifypath = "/verify";
        private static readonly string createPath = "/create";
        private static readonly string deletePath = "/delete";
        private static readonly string callPath = "/call";

        //incom  config wde
        int minSpeechLenCreate = 24;
        int minSpeechLenAuthentification = 20;
        int waitVoiceQualityCheck = 4;
        int waitAuthentificationCheck = 4;
        int lowProb = 30;
        int highProb = 50;
        bool startAutomaticAuthentication = true;
        bool agentCanAuthVP = true;  //wde role
        bool agentCanCreateVP = true; //wde role
        bool agentCanDeleteVP = true;  //wde role 
        string channelType = "IN"; //callType
        bool vbioRecordingAllowed = true; //UserData
        string agentId = "agentId";
        string phoneNumber = "99011234567"; //getinfo response
        string cuid = "9986537";            //getinfo response
        public string callUUID = "998U87J5FGADL3QTR0F0U2LAES00RGLF";
        string callToken = "99ae82e9-9cf8-467a-b900-a0a344db3d36"; //verify response errorinfo
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
        private void voice_stackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //client.Options.BaseUrl
         //   userEventListener = new UserEventListener(this.container);
            IDictionary<string, object> contextDisctionary = Context as IDictionary<string, object>;
            object caseView;
            contextDisctionary.TryGetValue("CaseView", out caseView);
            object caseObject;
            contextDisctionary.TryGetValue("Case", out caseObject);
            ICase @case = caseObject as ICase;
            if (@case != null)
            {
                log.Info("create incom view");

                interaction = @case.MainInteraction as IInteraction; // MessageBox.Show("int " + interaction);
                if (interaction != null && interaction.Type != null && interaction.Type == "InteractionVoice")
                {
                    log.Info("IncomCreate: " + interaction.Type.ToString());
                    IInteractionVoice iv = interaction as IInteractionVoice;
                    agentId = iv.Agent.UserName;
                    IMessage mes = iv.EntrepriseLastInteractionEvent as IMessage;
                 //   MessageBox.Show(mes.Name.ToString()+mes.Contains("CallUuid"));
                    
                    if (mes.Name== "EventEstablished")
                    {
                        EventEstablished ee = (EventEstablished)mes;  callUUID = ee.CallUuid;
                    }
                    if (mes.Name== "EventDialing")
                    {
                        EventDialing ed = (EventDialing)mes; callUUID = ed.CallUuid;
                    }
                    if (true)
                    {
                    }
                    phoneNumber = iv.PhoneNumber;
                    // btn_bio_create.IsEnabled = userEvent.isCreateEnable;
                    //      string callType = iv.GetIWCallType();
                    //  string calluuid = iv.AttachedDataInformation.
                    if (iv.GetAllAttachedData().ContainsKey("CUID"))
                    {
                        cuid=iv.ExtractAttachedData().GetAsString("CUID");
                        //    //iv.ExtractAttachedData().GetAsString("PHONETYPE"); //="PRIMARY_MOBILE"
                    }

                    if (iv.ExtractAttachedData().GetAsString("cosentVoice") != "0")
                    {
                        //btn_bio_create.IsEnabled = userEvent.isCreateEnable;
                        btn_bio_delete.IsEnabled = true;
                        btn_bio_virefy.IsEnabled = true;
                    }
                }
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
		{ log.Info("IncomView is normaly destroyed");
            try
            {
                if (BiometryTimerAuth.IsEnabled)
                {
                    BiometryTimerAuth.Stop();
                }
                if (BiometryTimerCreate.IsEnabled)
                {
                    BiometryTimerCreate.Stop();
                }
            }
            catch (Exception)
            {

                throw;
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
                log.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }
    }
}
