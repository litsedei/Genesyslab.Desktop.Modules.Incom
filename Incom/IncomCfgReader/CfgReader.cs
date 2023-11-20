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
//using Genesyslab.Desktop.Modules.Incom.UserEvenManagment;
//using System.Windows.Forms;
using System.Reflection;


namespace Genesyslab.Desktop.Modules.Incom.IncomConfgReader
{
    /// <summary>
    /// Public class which retrieves all custom configuration for the customization from Configuration Layer
    /// </summary>
    public class CfgReader : ICfgReader
    {
        private readonly IObjectContainer container = null;
        public readonly IConfigManager config = null;
        private readonly IConfigurationService configurationService = null;
        private readonly ILogger log = null;
        public KeyValueCollection IncomOptions = new KeyValueCollection();    // Variables for storing IncomOptions values
        private string LOG_PREFIX = "#Incom_CustomConfig#";
        //Incom  custom config
        private string CFG_MAIN_PREFIX = "incom";

        //Incom view custom config
        private string CFG_VOIPSNIFFER_HOST = "voipsniffer_host";
        private string CFG_VOIPSNIFFER_TICK = "voipsniffer_tick";  // FOR CHANGE


        private string CFG_LOWPROB = "LOWPROB";
        private string CFG_HIGHPROB = "HIGHPROB";
        private string CFG_GETINFO_PATH = "getinfo_path";
        private string CFG_VERIFY_PATH = "verify_path";
        private string CFG_CREATE_PATH = "create_path";
        private string CFG_DELETE_PATH = "delete_path";
        private string CFG_CALL_PATH = "call_path";
        private string CFG_MIN_SPEECH_LEN_AUTHENTIFICATION = "minSpeechLenAuthentification";
        private string CFG_MIN_SPEECH_LEN_CREATE = "minSpeechLenCreate";
        private string CFG_WAIT_VOICE_QUALITY_CHECK= "waitVoiceQualityCheck";
        private string CFG_WAIT_AUTHENTIFICATION_CHECK = "waitAuthentificationCheck";
        private string CFG_AUTOMATIC_AITHENTIFICATION = "startAutomaticAuthentication";

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

            IncomOptions.Add(keyName, configValue);
        }
        #endregion
        #region Public Methods for Exposing Configuration
        public string GetMainConfig(string keyName)
        {
            string fullKeyName = String.Format("{0}.{1}", CFG_MAIN_PREFIX, keyName);
            if (IncomOptions.ContainsKey(fullKeyName))
            {
                return IncomOptions.GetAsString(fullKeyName);
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
                // Generic options: incom.* options (on IW options)
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_VOIPSNIFFER_HOST), "http://10.8.52.137:5000", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_HIGHPROB), "90", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_LOWPROB), "50", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_VOIPSNIFFER_TICK), "4", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_GETINFO_PATH), "/getinfo", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_VERIFY_PATH), "/verify", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_CREATE_PATH), "/create", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_DELETE_PATH), "/delete", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_CALL_PATH),"/call", ValidateString);

                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_MIN_SPEECH_LEN_AUTHENTIFICATION), "30", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_MIN_SPEECH_LEN_CREATE), "30", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_WAIT_VOICE_QUALITY_CHECK), "4", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_WAIT_AUTHENTIFICATION_CHECK), "4", ValidateString);
                ReadIWOption(String.Format("{0}.{1}", CFG_MAIN_PREFIX, CFG_AUTOMATIC_AITHENTIFICATION), "1", ValidateString);

            }
            catch (Exception ex)
            {
                log.Error("Incom config: "+LOG_PREFIX + ex.Message+" "+ ex.InnerException);
            }

            log.Info(String.Format("{0} Finished configuration from Configuration Layer", LOG_PREFIX));
        }
    }
}
