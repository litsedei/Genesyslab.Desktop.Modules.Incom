using Genesyslab.Desktop.Modules.Incom.IncomUI;
using Genesyslab.Desktop.Modules.Incom.JsonSerializing;
using System.Text.Json;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace Genesyslab.Desktop.Modules.Incom.IncomUI
{
    public partial class IncomView : UserControl, IIncomView
    {
        public DispatcherTimer BiometryTimerAuth = new DispatcherTimer();
        public DispatcherTimer BiometryTimerCreate = new DispatcherTimer();
        private DispatcherTimer GetTimer(string type)
        {
         return type=="create" ? BiometryTimerCreate :  BiometryTimerAuth;
        }
        private void timer_tick_verify(object sender, EventArgs e)
        {
            try
            {
                CallReq callReq = new CallReq();
                callReq.requestType = "QUALITY";
                callReq.callUUID = callUUID;
                callReq.channelType = channelType;
                string bodyCall = JsonConvert.SerializeObject(callReq);
               // string bodyCall= JsonSerializer.Serialize(callReq);
                var callResponse = biometry_call_exec(bodyCall);
                int bioScore = 0;
                //bioScore = DateTime.Now.Second;
                speechLen = Convert.ToInt32(Math.Round(Convert.ToDouble(callResponse.businessInfo.speechLen.ToString())));
                len= Convert.ToInt32(Math.Round(Convert.ToDouble(callResponse.businessInfo.len.ToString())));
                callToken = callResponse.businessInfo.callToken.ToString();
                lbl_bio_status.Text = callResponse.errorInfo.description.ToString();
                if (speechLen >= minSpeechLenAuthentification)
                {
                    VerifyReq verifyReqBody = new VerifyReq();
                    verifyReqBody.callToken = callToken;
                    verifyReqBody.CUID = cuid;
                    verifyReqBody.phoneNumber = phoneNumber;
                    verifyReqBody.callUUID = callUUID;
                    verifyReqBody.agentId = agentId;
                    //string verifyReq = JsonSerializer.Serialize(verifyReqBody);
                    string verifyReq = JsonConvert.SerializeObject(verifyReqBody);
                    var verifyResponse = biometry_verify_exec(verifyReqBody);
                    bioScore = Convert.ToInt32(Math.Round(Convert.ToDouble(verifyResponse.businessInfo.prob.ToString())));
                    lbl_bio_score.Text = bioScore.ToString();
                    //call exec
                    vb_illuminator(bioScore);
                }
            }
            catch (Exception ex)
            {
              //  MessageBox.Show(ex.Message);
                log.Error("Incom: "+ex.InnerException);
            }
        }

        private void timer_tick_create(object sender, EventArgs e)
        {
            try
            {
                CallReq callReq = new CallReq();
                callReq.requestType = "QUALITY";
                callReq.callUUID = callUUID;
                callReq.channelType = "IN";
                callReq.callToken = callToken;
                string bodyCall = JsonConvert.SerializeObject(callReq);
                //string bodyCall = JsonSerializer.Serialize(callReq);
                var callResponse = biometry_call_exec(bodyCall);
                speechLen = Convert.ToInt32(Math.Round(Convert.ToDouble(callResponse.businessInfo.speechLen.ToString())));
                len = Convert.ToInt32(Math.Round(Convert.ToDouble(callResponse.businessInfo.len.ToString())));
                
                lbl_bio_status.Text = callResponse.errorInfo.description.ToString();
                if (speechLen >= minSpeechLenCreate)
                {
                    btn_bio_create.IsEnabled = true;
                    lbl_bio_status.Text="??Слепок может быть сохранен??";
                    //BiometryTimer.Stop();
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
                log.Error("Incom: "+ex.InnerException);
            }
        }

        private void vb_illuminator(int bioScore)
        {
            switch (bioScore)
            {
                case int n when (n < lowProb):
                    SolidColorBrush msbRed = IncomBtnBrush(248, 63, 63);
                    elps_bio.Stroke = msbRed;
                    lbl_bio_score.Foreground = msbRed;
                    lbl_bio_score.Text = n.ToString() + " %";
                    lbl_bio_status.Text = "Аутентификация не пройдена";
                    btn_bio_create.IsEnabled = false;
                    break;

                case int n when (n >= lowProb && n < highProb):
                    SolidColorBrush msbYellow = IncomBtnBrush(247, 227, 59);
                    elps_bio.Stroke = msbYellow;
                    lbl_bio_score.Foreground = msbYellow;
                    lbl_bio_score.Text = n.ToString() + " %";
                    lbl_bio_status.Text = "Аутентификация не завершена";
                    btn_bio_create.IsEnabled = false;
                    break;

                case int n when (n >= highProb):
                    SolidColorBrush msbGreen = IncomBtnBrush(17, 186, 54);
                    elps_bio.Stroke = msbGreen;
                    lbl_bio_score.Foreground = msbGreen;
                    lbl_bio_score.Text = n.ToString() + " %";
                    lbl_bio_status.Text = "Успешная аутентификация";
                    btn_bio_create.IsEnabled = false;
                    break;
            }
        }
    }
}
