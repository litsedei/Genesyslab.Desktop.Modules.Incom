using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.Json;
using Newtonsoft.Json;
using RestSharp;
using Genesyslab.Desktop.Modules.Incom.JsonDeserializing;
using Genesyslab.Desktop.Modules.Incom.JsonSerializing;


namespace Genesyslab.Desktop.Modules.Incom.IncomUI
{
    public partial class IncomView : UserControl, IIncomView
    {
        private RestResponse PostBiometryJson(string path, object body)
        {
            RestResponse response = new RestResponse();
            RestClient client = new RestClient(cfgReader.GetMainConfig("voipsniffer_host"));
           // RestClient client = new RestClient("http://127.0.0.1:8080");
            
            //todo


            try

            {
                log.Info("Incom. Biometry request. Host: " +client.Options.BaseUrl+" path: "+path+" body: " +body.ToString());
                RestRequest request = new RestRequest(path, Method.Post);
                request.Method = Method.Post;
                request.Timeout = 1000;
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                response = client.Post(request);
                log.Info("Incom: Biometry response: " + response.Content.ToString() + " Response code: " + response.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
                response.Content = ex.Message + " " + response.StatusCode.ToString();
            }
            client.Dispose();
            return response; 
                   
        }
        private GetInfoResponseJson biometry_getInfo_exec(object body)
        {
            try
            {
                GetInfoResponseJson getInfoResponse = JsonConvert.DeserializeObject<GetInfoResponseJson>(PostBiometryJson(getInfoPath, body).Content);
                //GetInfoResponseJson getInfoResponse = JsonSerializer.Deserialize<GetInfoResponseJson>(PostBiometryJson(getInfoPath, body).Content);
                return getInfoResponse;
            }
            catch (Exception ex)
            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private CallResponseJson biometry_call_exec(object body)
        {
            try
            {
               // CallResponseJson callResponse = JsonSerializer.Deserialize<CallResponseJson>(PostBiometryJson(callPath, body).Content);
                CallResponseJson callResponse = JsonConvert.DeserializeObject<CallResponseJson>(PostBiometryJson(callPath, body).Content);
                return callResponse;
            }
            catch (Exception ex)

            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private CreateResponseJson biometry_create_exec(object body)
        {
            try
            {
                  CreateResponseJson createResponse = JsonConvert.DeserializeObject<CreateResponseJson>(PostBiometryJson(createPath, body).Content);
               // CreateResponseJson createResponse = JsonSerializer.Deserialize<CreateResponseJson>(PostBiometryJson(createPath, body).Content);
                lbl_bio_status.Text = createResponse.errorInfo.description;

                return createResponse;
            }
            catch (Exception ex)
            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private VerifyResponseJson biometry_verify_exec(object body)
        {
            try
            {
                VerifyResponseJson verifyResponse = JsonConvert.DeserializeObject<VerifyResponseJson>(PostBiometryJson(verifypath, body).Content);
                //VerifyResponseJson verifyResponse = JsonSerializer.Deserialize<VerifyResponseJson>(PostBiometryJson(verifypath, body).Content);
                lbl_bio_status.Text = verifyResponse.errorInfo.description;
                return verifyResponse;
            }
            catch (Exception ex)
            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private DeleteResponseJson biometry_delete_exec(object body)
        {
            try
            {
                DeleteResponseJson deleteResponse = JsonConvert.DeserializeObject<DeleteResponseJson>(PostBiometryJson(deletePath, body).Content);
                //DeleteResponseJson deleteResponse = JsonSerializer.Deserialize<DeleteResponseJson>(PostBiometryJson(deletePath, body).Content);
                lbl_bio_status.Text = deleteResponse.errorInfo.description;
                return deleteResponse;
            }
            catch (Exception ex)
            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }

        private void btn_bioCreate_click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateReq createReqBody = new CreateReq();
                createReqBody.callToken = callToken;
                createReqBody.cuid = cuid;
                createReqBody.phoneNumber = phoneNumber;
                createReqBody.agentId = agentId;
                createReqBody.callUUID = callUUID;
                string createBody = JsonConvert.SerializeObject(createReqBody);
                //string createBody = JsonSerializer.Serialize(createReqBody);
                var createResponse = biometry_create_exec(createBody);
                lbl_bio_status.Text = createResponse.errorInfo.description.ToString();
                if (createResponse.errorInfo.code.ToString() == "0" && createResponse.errorInfo.description.ToString() == "Слепок голоса записан")
                {
                    //agentCanAuthVP = true ? btn_bio_virefy.IsEnabled = true : btn_bio_virefy.IsEnabled = false;
                    //agentCanDeleteVP = true ? btn_bio_delete.IsEnabled = true : btn_bio_delete.IsEnabled = false;


                    btn_bio_delete.IsEnabled = agentCanDeleteVP ? true : false;

                    btn_bio_create.IsEnabled = false;
                    BiometryTimerCreate.Stop();
                    if (startAutomaticAuthentication)
                    {
                        lbl_bio_status.Text = "??--";
                        AuthVB(GetTimer("auth"));
                    }
                    else
                    {
                        btn_bio_virefy.IsEnabled = agentCanAuthVP ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }
        private void btn_bioVeify_click(object sender, RoutedEventArgs e)
        {
            try
            {
                lbl_bio_status.Text = null;
                btn_bio_virefy.IsEnabled = false;
                AuthVB(GetTimer("auth"));
            }
            catch (Exception ex)
            {
                log.Error("Incom: "+System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }
        private void btn_bioDelete_click(object sender, RoutedEventArgs e)
        {
            try
            {
                lbl_bio_score.Text = string.Empty;
                elps_bio.Fill = Brushes.Transparent;
                elps_bio.Stroke = Brushes.Transparent;
                DeleteReq deleteReq = new DeleteReq();
                deleteReq.cuid = cuid;
                deleteReq.callUUID = callUUID;
                deleteReq.PhoneNumber = phoneNumber;
                deleteReq.agentId = agentId;
                string deleteReqBody = JsonConvert.SerializeObject(deleteReq);
                //string deleteReqBody = JsonSerializer.Serialize(deleteReq);
                var deleteResponse = biometry_delete_exec(deleteReqBody);
                if (deleteResponse.errorInfo.code.ToString() == "0" && deleteResponse.errorInfo.description.ToString() == "Слепок голоса удалён")
                {
                    btn_bio_delete.IsEnabled = false;
                    btn_bio_virefy.IsEnabled = false;
                    if (BiometryTimerAuth.IsEnabled)
                    {
                        BiometryTimerAuth.Stop();
                    }

                    if (agentCanCreateVP == true)
                    {
                        CreateVB(GetTimer("create"));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Incom: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }

        private void AuthVB(DispatcherTimer timer)
        {
            try
            {
                if (vbioRecordingAllowed == true)
                {
                    GetInfoReq getInfoReqBody = new GetInfoReq();
                    getInfoReqBody.cuid = cuid;
                    getInfoReqBody.phoneNumber = phoneNumber;
                 //   string getinfoReq = JsonSerializer.Serialize(getInfoReqBody);
                    string getinfoReq = JsonConvert.SerializeObject(getInfoReqBody);
                    var getInfoResponse = biometry_getInfo_exec(getinfoReq);

                    if (getInfoResponse.businessInfo.disableRecording == false && getInfoResponse.businessInfo.isVoiceId == true && getInfoResponse.businessInfo.disableRecording == false)
                    {
                        // to do Add call - record Exec
                        VerifyReq verifyReqBody = new VerifyReq();
                        verifyReqBody.callToken = callToken;
                        verifyReqBody.CUID = callUUID;
                        verifyReqBody.phoneNumber = phoneNumber;
                        verifyReqBody.agentId = agentId;
                          string verifyReq = JsonConvert.SerializeObject(verifyReqBody);
                      //  string verifyReq = JsonSerializer.Serialize(verifyReqBody);
                        timer.Interval = TimeSpan.FromSeconds(waitAuthentificationCheck);
                        timer.Tick += timer_tick_verify;
                        timer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Incom: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }
        private void CreateVB(DispatcherTimer timer)
        {
            CallReqRecord callReqR = new CallReqRecord();
            callReqR.requestType = "RECORD";
            callReqR.callUUID = callUUID;
            callReqR.channelType = channelType;
            callReqR.phoneNumber= phoneNumber;
           // string bodyCallR = JsonSerializer.Serialize(callReqR);
            string bodyCallR = JsonConvert.SerializeObject(callReqR);
            var callReponseQ = biometry_call_exec(bodyCallR);
            lbl_bio_status.Text = callReponseQ.errorInfo.description.ToString();
            timer.Interval = TimeSpan.FromSeconds(waitVoiceQualityCheck);
            timer.Tick += timer_tick_create;
            timer.Start();
        }
        private static SolidColorBrush IncomBtnBrush(byte A, byte B, byte C)
        {
            SolidColorBrush msb = new SolidColorBrush();
            msb.Color = Color.FromRgb(A, B, C);
            return msb;
        }

        private void bio_eventPluginExHandling_RestClient(Exception ex)
        {
            log.Error("Incom: "+System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.Message);
            btn_bio_create.IsEnabled = false;
            btn_bio_delete.IsEnabled = false;
            btn_bio_virefy.IsEnabled = false;
            elps_bio.Fill = Brushes.Transparent;
            BiometryTimerAuth.Stop();
            BiometryTimerCreate.Stop();
            lbl_bio_score.Text = string.Empty;
            lbl_bio_status.Text = ex.Message;

        }
    }
}
