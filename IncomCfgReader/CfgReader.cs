using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Genesyslab.Desktop.Infrastructure.Configuration;
using Genesyslab.Desktop.Infrastructure.DependencyInjection;
using Genesyslab.Desktop.Modules.Core.SDK.Configurations;
using Genesyslab.Platform.ApplicationBlocks.ConfigurationObjectModel.CfgObjects;
using Genesyslab.Platform.Commons.Collections;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Platform.Commons.Protocols;
using Genesyslab.Platform.Configuration.Protocols.Types;
using Genesyslab.Desktop.Modules.Incom.DispositionCodeEx;
//using System.Windows.Forms;
using System.Reflection;


namespace Genesyslab.Desktop.Modules.Incom.CrmConfgReader
{
    /// <summary>
    /// Public class which retrieves all custom configuration for the customization from Configuration Layer
    /// </summary>
    public class CfgReader : ICfgReader
    {
        private readonly IObjectContainer container = null;
        private readonly IConfigManager config = null;
        private readonly IConfigurationService configurationService = null;
        private readonly ILogger log = null;
        public KeyValueCollection configOptions = new KeyValueCollection();    // Variables for storing option values
        private string LOG_PREFIX = "#CRM_CustomConfig#";
        //CRM  custom config
        private string CFG_MAIN_PREFIX = "sdbo";
        private string CFG_URI_CRM = "CRM_URI";
        private string CFG_CRM_URI_TEST = "CRM_URI_TEST";
        private string CFG_URI_CKC = "CRM_URI_CKC";
        private string CFG_URI_CKC_OB = "CRM_URI_CKC_OB";
        private string CFG_URI_CKC_CHAT = "CRM_URI_CKC_CHAT";
        private string CFG_URI_CKC_CHAT_TEST = "CRM_URI_CKC_CHAT_TEST";
        private string CFG_URI_CKC_EMAIL = "CRM_URI_CKC_EMAIL";
        private string CFG_URI_CKC_EMAIL_TEST = "CRM_URI_CKC_EMAIL_TEST";
        private string CFG_URI_CKC_TEST = "CRM_URI_CKC_TEST";
        private string CFG_URI_CKC_OB_TEST = "CRM_URI_CKC_OB_TEST";
        private string CFG_CRM_URI_KAZN = "CRM_URI_KAZN";
        private string CFG_CRM_URI_KAZN_TEST = "CRM_URI_KAZN_TEST";
        private string CFG_URI_SOCIAL_TEST = "CRM_URI_SOCIAL_TEST";
        private string CFG_URI_SOCIAL = "CRM_URI_SOCIAL";
        private string CFG_CRM_SOCIAL = "CRM_SOCIAL_ENABLED";
        private string CFG_CRM_SOCIAL_TEST = "CRM_SOCIAL_ENABLED_TEST";
        // APPS MC2
        private string CFG_APPS_MC2_URI = "APPS_MC2_URI";
        private string CFG_APPS_MC2_URILB = "APPS_MC2_URILB";
        private string CFG_APPS_MC2_URI2 = "APPS_MC2_URI2";
        private string CFG_APPS_MC2_TEST = "APPS_MC2_TEST";
        //APPS CRM
        private string CFG_APICRM_ENABLED = "APICRM_ENABLED";
        private string CFG_APICRM_URI = "APICRM_URI";
        private string CFG_APICRM_TOKEN = "APICRM_TOKEN";
        private string CFG_APICRM_CHANNEL = "APICRM_CHANNEL";

        private string CFG_APICRM_ENABLED_TEST = "APICRM_ENABLED_TEST";
        private string CFG_APICRM_URI_TEST = "APICRM_URI_TEST";
        private string CFG_APICRM_TOKEN_TEST = "APICRM_TOKEN_TEST";
        private string CFG_APICRM_CHANNEL_TEST = "APICRM_CHANNEL_TEST";
        //Omilia viiew custom config
        private string CFG_OMILIA_TEST = "OMILIA_TEST";
        private string CFG_OMILIA1 = "OMILIA1";
        private string CFG_OMILIA2 = "OMILIA2";
        private string CFG_SURVEY = "survey-dest";
        private string CFG_OMILIA_VIEW = "omilia_view_enabled";
        


        public CfgReader(IObjectContainer container)
        {
            this.container = container;
            this.config = container.Resolve<IConfigManager>();
            this.configurationService = this.container.Resolve<IConfigurationService>();

            // Initialize logging
            log = container.Resolve<ILogger>();
            log = log.CreateChildLogger("CfgReader");
            ReadFullConfiguration();
        }

        #region Validation Predicate functions
        static bool ValidateString(string value)
        {
            value = value.ToLower();
            return ((value != null));
        }
        #endregion
        #region Individual Option Reader Methods
        private void ReadIWOption(String keyName, String defaultValue, Predicate<String> validateFunc)
        {
            String configValue = (string)config.GetValue(keyName);
            if (String.IsNullOrEmpty(configValue))
            {
                configValue = defaultValue;
                log.Warn(String.Format("{0} Customization configuration [{1}] is not set: Default value [{2}] will be used", LOG_PREFIX, keyName, defaultValue));
            }
            else
            {
                Boolean isValidValue = (validateFunc != null) ? validateFunc(configValue) : true;
                if (!isValidValue)
                {
                    log.Warn(String.Format("{0} Invalid customization configuration [{1}]: Invalid value of [{2}]. " + "Default value of [{3}] is used", LOG_PREFIX, keyName, configValue, defaultValue));
                }
                else
                {
                    // Locate where the option has been set. Only take the ~first~ occurrence found, as this takes the highest option precedence
                    IConfigDictionary foundDictionary = null;
                    foreach (IConfigDictionary thisDictionary in config.DictionaryChain)
                    {
                        if ((thisDictionary[keyName] != null) && (foundDictionary == null)) foundDictionary = thisDictionary;
                    }
                    log.Debug(String.Format("{0} Customization configuration [{1}] with value [{2}] (found at {3} level)", LOG_PREFIX, keyName, configValue, foundDictionary.Name));
                }
                configValue = (isValidValue) ? configValue : defaultValue;
            }

            configOptions.Add(keyName, configValue);
        }
        #endregion
        #region Public Methods for Exposing Configuration
        public string GetMainConfig(string keyName)
        {
            string fullKeyName = String.Format("{0}.{1}", CFG_MAIN_PREFIX, keyName);
            if (configOptions.ContainsKey(fullKeyName))
            {
                return configOptions.GetAsString(fullKeyName);
            }
            else
                throw new Exception(String.Format("GetMainConfig: key [{0}] not found", fullKeyName));
        }
        #endregion
        /// <summary>
        /// Reads the options from CME for this customization
        /// </summary>
        /// <returns></returns>
        private void ReadFullConfiguration()
        {
            log.Info(String.Format("{0} Reading configuration from Configuration Layer...", LOG_PREFIX));
            try
            {
                // Generic options: sdbo.* options (on IW options)
             //   Singleton.Instance().ucsHost = configurationService.GetAvailableConnection(CfgAppType.CFGContactServer).ConnectionParameters[0].ServerInformation.Host.Name;
             //   Singleton.Instance().ucsPort = int.Parse(configurationService.GetAvailableConnection(CfgAppType.CFGContactServer).ConnectionParameters[1].ServerInformation.Port);
             //   Singleton.Instance().ucsHost_b = configurationService.GetAvailableConnection(CfgAppType.CFGContactServer).ConnectionParameters[1].ServerInformation.Host.Name;
            //    Singleton.Instance().ucsPort_b = int.Parse(configurationService.GetAvailableConnection(CfgAppType.CFGContactServer).ConnectionParameters[1].ServerInformation.Port);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CRM), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CKC), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_CRM_URI_KAZN), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APPS_MC2_URI), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APPS_MC2_URI2), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APPS_MC2_URILB), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_OMILIA1), "EmtyConfigString", ValidateString);

                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APICRM_ENABLED), "false", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APICRM_URI), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APICRM_TOKEN), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APICRM_CHANNEL), "EmtyConfigString", ValidateString);

                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APICRM_ENABLED_TEST), "false", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APICRM_URI_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APICRM_TOKEN_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APICRM_CHANNEL_TEST), "EmtyConfigString", ValidateString);

                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_OMILIA2), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CKC_OB), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CKC_CHAT), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CKC_EMAIL), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_SOCIAL), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_OMILIA_VIEW), "false", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_SURVEY), "246318", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_CRM_URI_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CKC_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_CRM_URI_KAZN_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_APPS_MC2_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_OMILIA_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CKC_OB_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CKC_CHAT_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_CKC_EMAIL_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_URI_SOCIAL_TEST), "EmtyConfigString", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_CRM_SOCIAL), "false", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_CRM_SOCIAL_TEST), "false", ValidateString);
            }
            catch (Exception ex)
            {
                log.Error(LOG_PREFIX + ex.Message, ex);
            }

            log.Info(String.Format("{0} Finished configuration from Configuration Layer", LOG_PREFIX));
        }
    }
}
