﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.Json;
using Newtonsoft.Json;
using RestSharp;
using Genesyslab.Desktop.Modules.Incom.JsonDeserializing;
using Genesyslab.Desktop.Modules.Incom.JsonSerializing;
using Genesyslab.Platform.Voice.Protocols.TServer;
using System.IO.Packaging;
using System.Windows.Media.Media3D;
using Genesyslab.Desktop.Modules.Contacts.Requests;
using Genesyslab.Platform.Commons.Protocols;

namespace Genesyslab.Desktop.Modules.Incom.IncomUI
{
    public partial class IncomView : UserControl, IIncomView
    {
        private RestResponse PostBiometryJson(string path, object body)
        {
            RestResponse response = new RestResponse();
            RestClient client = new RestClient(cfgReader.GetMainConfig("voipsniffer_host"));
            try
            {
                log.Info("Incom: Biometry request. Host: " +client.Options.BaseUrl+" path: "+path+" body: " +body.ToString());
                RestRequest request = new RestRequest(path, Method.Post);
                request.Method = Method.Post;
                request.Timeout = httpTimeout;
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                response = client.Post(request);
                log.Info("Incom: Biometry response. Host: " +client.Options.BaseUrl+" path: "+path  + response.Content.ToString() + " Response code: " + response.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                log.Error("Incom: Biometry response ex: path::"+path.ToString()+ " "+ex.InnerException);
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
                response.Content = ex.Message + " " + response.StatusCode.ToString();
                lbl_bio_status.Text=ex.Message;
            }
            client.Dispose();
            return response; 
                   
        }
        private GetInfoResponseJson biometry_getInfo_exec(string cuid, string phoneNumber)
        {
            try
            {
                GetInfoReq getInfoReqBody = new GetInfoReq();
                getInfoReqBody.cuid = cuid;
                getInfoReqBody.phoneNumber = phoneNumber;
                string getinfoReq = JsonConvert.SerializeObject(getInfoReqBody);
                GetInfoResponseJson getInfoResponse = JsonConvert.DeserializeObject<GetInfoResponseJson>(PostBiometryJson(cfgReader.GetMainConfig("getinfo_path"), getinfoReq).Content);
                //GetInfoResponseJson getInfoResponse = JsonSerializer.Deserialize<GetInfoResponseJson>(PostBiometryJson(getInfoPath, body).Content);
                
                return getInfoResponse;
            }
            catch (Exception ex)
            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private CallResponseJson biometry_call_exec(string requestType,string callUUID, string channelType, string phoneNumber, string callToken)
        {
            try
            {
                    CallReq callReq = new CallReq();
                    callReq.requestType = requestType;
                    callReq.callUUID = callUUID;
                    callReq.channelType = channelType;
                    callReq.phoneNumber = phoneNumber;
                     callReq.callToken = callToken;
                    string bodyCall = JsonConvert.SerializeObject(callReq);
                    // string bodyCall= JsonSerializer.Serialize(callReq);
                    // CallResponseJson callResponse = JsonSerializer.Deserialize<CallResponseJson>(PostBiometryJson(callPath, body).Content);
                    CallResponseJson callResponse = JsonConvert.DeserializeObject<CallResponseJson>(PostBiometryJson(cfgReader.GetMainConfig("call_path"), bodyCall).Content);
                return callResponse;                
            }
            catch (Exception ex)

            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private CallResponseJson biometry_callRecord_exec(string requestType, string callUUID, string channelType, string phoneNumber, string side)
        {
            try
            {
                System.Threading.Thread.Sleep(2000);
                CallReqRecord callReq = new CallReqRecord();
                callReq.requestType = requestType;
                callReq.callUUID = callUUID;
                callReq.channelType = channelType;
                callReq.phoneNumber = phoneNumber;
                callReq.side = side;
                string bodyCall = JsonConvert.SerializeObject(callReq);
                // string bodyCall= JsonSerializer.Serialize(callReq);
                // CallResponseJson callResponse = JsonSerializer.Deserialize<CallResponseJson>(PostBiometryJson(callPath, body).Content);
                CallResponseJson callResponse = JsonConvert.DeserializeObject<CallResponseJson>(PostBiometryJson(cfgReader.GetMainConfig("call_path"), bodyCall).Content);
               
                //log.Info("Incom. CallToken");
              //  callToken =callResponse.businessInfo.callToken.ToString();
                //  isRecordStarted=true;
               // log.Info("Incom. SetCallToken");
               // incomView.SetCallToken(callResponse.businessInfo.callToken.ToString());

                return callResponse;
            }
            catch (Exception ex)

            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private CreateResponseJson biometry_create_exec(string callToken, string cuid, string phoneNumber,string agentId,string callUUID)
        {
            try
            {
                CreateReq createReq = new CreateReq();
                createReq.cuid = cuid;
                createReq.callToken = callToken;
                createReq.callUUID = callUUID;
                createReq.agentId = agentId;
                createReq.phoneNumber= phoneNumber;
                string createBody= JsonConvert.SerializeObject(createReq);
                CreateResponseJson createResponseJson = JsonConvert.DeserializeObject<CreateResponseJson>(PostBiometryJson(cfgReader.GetMainConfig("create_path"), createBody).Content);
                lbl_bio_status.Text = createResponseJson.errorInfo.description;
                return createResponseJson;
            }
            catch (Exception ex)
            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private VerifyResponseJson biometry_verify_exec(string callToken, string cuid, string phoneNumber, string agentId, string callUUID)
        {
            try
            {
                VerifyReq verifyReq= new VerifyReq();
                verifyReq.cuid = cuid;
                verifyReq.phoneNumber = phoneNumber;
                verifyReq.agentId = agentId;
                verifyReq.callUUID = callUUID;
                verifyReq.callToken=callToken;
                string verifReqBody = JsonConvert.SerializeObject(verifyReq);
                VerifyResponseJson verifyResponseJson = JsonConvert.DeserializeObject<VerifyResponseJson>(PostBiometryJson(cfgReader.GetMainConfig("verify_path"), verifReqBody).Content);
                lbl_bio_status.Text = verifyResponseJson.errorInfo.description;
                return verifyResponseJson;
            }
            catch (Exception ex)
            {
                bio_eventPluginExHandling_RestClient(ex);
                return null;
            }
        }
        private DeleteResponseJson biometry_delete_exec(string cuid, string callUUID, string phoneNumber, string agentId)
        {
            try
            {
                DeleteReq deleteReq = new DeleteReq();
                deleteReq.cuid = cuid;
                deleteReq.callUUID = callUUID;
                deleteReq.PhoneNumber = phoneNumber;
                deleteReq.agentId = agentId;
                string deleteReqBody = JsonConvert.SerializeObject(deleteReq);
                //string deleteReqBody = JsonSerializer.Serialize(deleteReq);
                DeleteResponseJson deleteResponse = JsonConvert.DeserializeObject<DeleteResponseJson>(PostBiometryJson(cfgReader.GetMainConfig("delete_path"), deleteReqBody).Content);
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
                log.Info("Incon Biometry: button Create click");
                var createResponse = biometry_create_exec(callToken, cuid, phoneNumber, agentId, callUUID);
                lbl_bio_status.Text = createResponse.errorInfo.description.ToString();
                if (createResponse.errorInfo.code.ToString() == "0" && createResponse.errorInfo.description.ToString() == "do_what_you_please")
                {
                    //agentCanAuthVP = true ? btn_bio_virefy.IsEnabled = true : btn_bio_virefy.IsEnabled = false;
                    //agentCanDeleteVP = true ? btn_bio_delete.IsEnabled = true : btn_bio_delete.IsEnabled = false;
                    btn_bio_delete.IsEnabled = agentCanDeleteVP ? true : false;
                    btn_bio_create.IsEnabled = false;
                    lbl_bio_status.Text="Голосовой эталон сохранен";
                    log.Info("Incon Biometry: Voice print saved");
                    BiometryTimerCreate.Stop();
                }
                if (createResponse.errorInfo.code.ToString() == "1" && createResponse.errorInfo.description.ToString() == "not_enough_vectors")
                {
                    lbl_bio_status.Text = "Ошибка.Голосовой эталон не сохранен";
                    log.Error("Incon Biometry: Voice print not saved :"+ createResponse.errorInfo.description.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }
        private void btn_bioVerify_click(object sender, RoutedEventArgs e)
        {
            log.Info("Incom: button verify click");
            try
            {
                log.Info("Incom: Biometry button Verify click" );
                lbl_bio_status.Text = null;
                btn_bio_verify.IsEnabled = false;
                AuthVB(GetTimer("auth"),channelType,"click");
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
                log.Debug("Incom: Biometry button Delete click");
                lbl_bio_score.Text = string.Empty;
                elps_bio.Fill = Brushes.Transparent;
                elps_bio.Stroke = Brushes.Transparent;
                var deleteResponse = biometry_delete_exec(cuid,callUUID,phoneNumber,agentId);
                if (deleteResponse.errorInfo.code.ToString() == "0" && deleteResponse.errorInfo.description.ToString() == " voicetemplate_deleted")
                {
                    log.Info("Incom: Biometry "+ deleteResponse.errorInfo.description.ToString());
                    btn_bio_delete.IsEnabled = false;
                    btn_bio_verify.IsEnabled = false;
                    if (BiometryTimerAuth.IsEnabled)
                    {
                        BiometryTimerAuth.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Incom: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.InnerException);
            }
        }
        private void AuthVB(DispatcherTimer timer, string callType, string type)
        {
            log.Info("Incom: AuthVB");
            try
            {
                if (vbioRecordingAllowed == 1)
                {
                    if (type!="auto")
                    {

                    }
                    var getInfoResponse = biometry_getInfo_exec(cuid, phoneNumber);
                    //callToken=getInfoResponse.businessInfo.callToken.ToString();

                    if (getInfoResponse.businessInfo.disableRecording == 0 && getInfoResponse.businessInfo.isVoiceId == true)
                    {
                        biometry_call_exec("QUALITY", callUUID, channelType, phoneNumber, callToken);
                        timer.Interval = TimeSpan.FromSeconds(Convert.ToInt32(cfgReader.GetMainConfig("waitVoiceQualityCheck")));
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
        private void CreateVB(DispatcherTimer timer, string channelType)
        {
            log.Info("Incom: CreateVB");
            var callReponseQ = biometry_call_exec("QUALITY", callUUID, channelType, phoneNumber,callToken);
            lbl_bio_status.Text = callReponseQ.errorInfo.description.ToString();
            log.Info("Incom: CreateVB " + callReponseQ.errorInfo.description.ToString());
                timer.Interval = TimeSpan.FromSeconds(Convert.ToInt32(cfgReader.GetMainConfig("waitVoiceQualityCheck")));
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
            log.Error("Incom:  Biometry ex: "+System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + ex.Message);
            btn_bio_create.IsEnabled = btn_cr_state;
          //  Model.btnCreateState = false;
            btn_bio_delete.IsEnabled = false;
            btn_bio_verify.IsEnabled = false;
            elps_bio.Fill = Brushes.Transparent;
            BiometryTimerAuth.Stop();
            BiometryTimerCreate.Stop();
            lbl_bio_score.Text = string.Empty;
            lbl_bio_status.Text = ex.Message;
        }

        public void SetCallToken(string token)
        {
            try
            {
                callToken = token;
            }
            catch (Exception ex)
            {
                log.Error("Incom: SetTokken: "+token +" "+ex.Message + ex.InnerException);
            }
            
        }
    }
}
