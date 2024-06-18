using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;

namespace PiHire.BAL.Common._3rdParty
{
    public class MicrosoftOutlook
    {
        const string scopes = "openid%20offline_access%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.Read%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.Read.Shared%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.ReadWrite%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.ReadWrite.Shared%20https%3A%2F%2Fgraph.microsoft.com%2FOnlineMeetings.Read%20https%3A%2F%2Fgraph.microsoft.com%2FOnlineMeetings.ReadWrite%20https%3A%2F%2Fgraph.microsoft.com%2Fprofile%20https%3A%2F%2Fgraph.microsoft.com%2FUser.Read%20https%3A%2F%2Fgraph.microsoft.com%2Fmail.read";
        public static string getAuthorizeUrl(string RedirectUrl, string code)
        {
            var authorizeUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=" + teamClientId +
                    "&response_type=code" +
                    "&redirect_uri=" + (RedirectUrl ?? "http%3A%2F%2Flocalhost:4200%2F") +
                    "&response_mode=query" +
                    "&scope=" + scopes +
                    "&state=" + code;

            return authorizeUrl;
        }
        public static (string token, string msg) getRefreshToken(string AuthorizationCode, string RedirectUrl, Logging.Logger logger)
        {
            logger.Log(LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft refresh token generating");
            var tokenUrl = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", teamTenantId);
            var request = (HttpWebRequest)WebRequest.Create(tokenUrl);
            var postData = "client_id=" + teamClientId +
                            "&scope=" + scopes +
                            "&code=" + AuthorizationCode +
                            "&redirect_uri=" + (RedirectUrl ?? "http%3A%2F%2Flocalhost%3A4200%2F") +
                            "&grant_type=authorization_code" +
                            "&client_secret=" + teamClientSecret;
            var data = System.Text.Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            try
            {
                var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionViewModel>(responseString);
                if (responseObj != null)
                    return (responseObj.refresh_token, "");
                else
                    logger.Log(LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft refresh token generating failed:" + responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft refresh token generating error:" + responseString, e);
            }
            return ("", responseString);
            //            var respStr = "{\"token_type\":\"Bearer\",\"scope\":\"profile openid email https://graph.microsoft.com/Calendars.Read https://graph.microsoft.com/Calendars.Read.Shared https://graph.microsoft.com/Calendars.ReadWrite https://graph.microsoft.com/Calendars.ReadWrite.Shared https://graph.microsoft.com/User.Read https://graph.microsoft.com/.default\",\"expires_in\":3599,\"ext_expires_in\":3599,\"access_token\":\"eyJ0eXAiOiJKV1QiLCJub25jZSI6IkIwRlNqd3hOalVlMEk1M3VPRERVTElYM2NJUU1aTTZlSnNITUlHWTdOUEkiLCJhbGciOiJSUzI1NiIsIng1dCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyIsImtpZCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyJ9.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8yNDY1MmRiYy01NTc2LTRjZjAtOWU5Ny1lYjQ1YjgwOTQwMWQvIiwiaWF0IjoxNjIwMDE4NDY3LCJuYmYiOjE2MjAwMTg0NjcsImV4cCI6MTYyMDAyMjM2NywiYWNjdCI6MCwiYWNyIjoiMSIsImFjcnMiOlsidXJuOnVzZXI6cmVnaXN0ZXJzZWN1cml0eWluZm8iLCJ1cm46bWljcm9zb2Z0OnJlcTEiLCJ1cm46bWljcm9zb2Z0OnJlcTIiLCJ1cm46bWljcm9zb2Z0OnJlcTMiLCJjMSIsImMyIiwiYzMiLCJjNCIsImM1IiwiYzYiLCJjNyIsImM4IiwiYzkiLCJjMTAiLCJjMTEiLCJjMTIiLCJjMTMiLCJjMTQiLCJjMTUiLCJjMTYiLCJjMTciLCJjMTgiLCJjMTkiLCJjMjAiLCJjMjEiLCJjMjIiLCJjMjMiLCJjMjQiLCJjMjUiXSwiYWlvIjoiRTJaZ1lQaDYwOVBUdnFjMTdMaWdiTWlybjRudkF4c256L283S2VseXhPSHcrbzl4MTVzQiIsImFtciI6WyJwd2QiXSwiYXBwX2Rpc3BsYXluYW1lIjoicGlIaXJlIERFVih3aXRoIGluIGNtcG55KSIsImFwcGlkIjoiYzlkZWNiNWEtZDFlZC00NGYyLWFjMmQtZmYyN2JjNTM0MjFmIiwiYXBwaWRhY3IiOiIxIiwiZmFtaWx5X25hbWUiOiIoUGFyYW1JbmZvKSIsImdpdmVuX25hbWUiOiJCYWxhamkgTmFyYWhhcmlzZXR0eSIsImlkdHlwIjoidXNlciIsImlwYWRkciI6IjI3LjYuMTc3LjExMyIsIm5hbWUiOiJCYWxhamkgTmFyYWhhcmlzZXR0eSAoUGFyYW1JbmZvKSIsIm9pZCI6Ijc1YjIwYzY5LTU3ODktNGNlOS1iM2RjLTgzYTg3MTlkODI5ZiIsInBsYXRmIjoiMyIsInB1aWQiOiIxMDAzMjAwMDM0OThDODc3IiwicmgiOiIwLkFWUUF2QzFsSkhaVjhFeWVsLXRGdUFsQUhWckwzc250MGZKRXJDM19KN3hUUWg5VUFONC4iLCJzY3AiOiJDYWxlbmRhcnMuUmVhZCBDYWxlbmRhcnMuUmVhZC5TaGFyZWQgQ2FsZW5kYXJzLlJlYWRXcml0ZSBDYWxlbmRhcnMuUmVhZFdyaXRlLlNoYXJlZCBVc2VyLlJlYWQgcHJvZmlsZSBvcGVuaWQgZW1haWwiLCJzaWduaW5fc3RhdGUiOlsia21zaSJdLCJzdWIiOiJOZE9yMV9TT3R0dld4UXdvRzdsZ2h1eV9PV25oX1FGdklSR19Kb3c2M3lBIiwidGVuYW50X3JlZ2lvbl9zY29wZSI6IkFTIiwidGlkIjoiMjQ2NTJkYmMtNTU3Ni00Y2YwLTllOTctZWI0NWI4MDk0MDFkIiwidW5pcXVlX25hbWUiOiJiYWxhamkubkBwYXJhbWluZm8uY29tIiwidXBuIjoiYmFsYWppLm5AcGFyYW1pbmZvLmNvbSIsInV0aSI6ImVjSF8ybkp4blV5NTg0Ml9FRG5FQVEiLCJ2ZXIiOiIxLjAiLCJ3aWRzIjpbImI3OWZiZjRkLTNlZjktNDY4OS04MTQzLTc2YjE5NGU4NTUwOSJdLCJ4bXNfc3QiOnsic3ViIjoicVRlVVFTa2FHUTktVmtfYmtYVHcxZ3N6Z3diNnpidUt2eGw1SGo0WFV0OCJ9LCJ4bXNfdGNkdCI6MTM3MDc3ODMxM30.ZQGJfGtNBdvKsiWzfvaZalo5yb_LBIz2i-Dn2sjLvwrtx_LJ7aWSS3gLH4NjMh-jGhfEpyUr171hM3DjWiRKARu5H0iM5KqNo_0cF6zdPxoBVpshBoyU6Ao8mapOa-Hgx7VR9woqKrsOs3QAl52JNSUkkU7j138VKH-pOkPB6cSwAxkJhyRj2l4fNI9K3QE5oMGt2tO8eOT3mExBz7f3xBA6szt2GmgnI1BdgVjUe5jaFigukAAQu9Re1HUlJmu8ROSsAGbJzsbB2Uxp72vCFC6-3Bde6kCZ4Va6pxez4cY1rV5XntWqtRxUN5r7aRByyLCsk_LhSrSacSkiWPo5aw\"}";
        }


        const string baseUrl = "";
        private static Extensions.AppSettings appSettings = new Extensions.AppSettings();

        static string teamTenantId
        {
            get
            {
                return appSettings?.AppSettingsProperties?.MicrosoftTeamsTenantId ?? "24652dbc-5576-4cf0-9e97-eb45b809401d";
            }
        }
        static string teamClientId
        {
            get
            {
                return appSettings?.AppSettingsProperties?.MicrosoftTeamsClientId ?? "e9cc6b8b-8640-4874-b17c-6b8e4da753cc";
            }
        }
        static string teamClientSecret
        {
            get
            {
                return appSettings?.AppSettingsProperties?.MicrosoftTeamClientSecret ?? ".~gh4-WESj9gNTNrOj9szdCK3uz6q_3qeM";
            }
        }

        //public string userName { get; }
        //public string password { get; }
        //public MicrosoftOutlook(string userName, string password)
        //{
        //    this.userName = userName;
        //    this.password = password;
        //}
        private string token;
        public MicrosoftOutlook(string token)
        {
            this.token = token;
        }

        public async System.Threading.Tasks.Task<(string token, string msg)> getAccessTokenAsync(Logging.Logger logger)
        {
            if (string.IsNullOrEmpty(token))
            {
                return (string.Empty, "Outlook login expired. Please relogin again");
            }
            return (token, string.Empty);
            //if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            //{
            //    return (string.Empty, "Username/password not exist");
            //}
            //try
            //{
            //    var tokenUrl = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", teamTenantId);
            //    var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(tokenUrl);
            //    var postData = "client_id=" + teamClientId +
            //                    "&scope=https%3A%2F%2Fgraph.microsoft.com%2FCalendars.Read" +
            //                    "&username=" + userName +
            //                    "&password=" + password +
            //                    "&grant_type=password" +
            //                    "&client_secret=" + teamClientSecret;

            //    var data = System.Text.Encoding.ASCII.GetBytes(postData);
            //    request.Method = "POST";
            //    request.ContentType = "application/x-www-form-urlencoded";
            //    request.ContentLength = data.Length;
            //    using (var stream = request.GetRequestStream())
            //    {
            //        stream.Write(data, 0, data.Length);
            //    }
            //    var response = (System.Net.HttpWebResponse)await request.GetResponseAsync();
            //    var responseString = await new System.IO.StreamReader(response.GetResponseStream()).ReadToEndAsync();
            //    try
            //    {
            //        var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionViewModel>(responseString);
            //        if (responseObj != null)
            //            return (responseObj.access_token, string.Empty);
            //    }
            //    catch (Exception e)
            //    {

            //    }
            //    return (string.Empty, responseString);
            //}
            //catch (System.Net.HttpListenerException e)
            //{
            //    logger.Log(LogLevel.Error, Logging.LoggingEvents.Authenticate, "getAccessTokenAsync", e);
            //    return ("", "Something went wrong");
            //}
            //catch (System.Net.ProtocolViolationException e)
            //{
            //    logger.Log(LogLevel.Error, Logging.LoggingEvents.Authenticate, "getAccessTokenAsync", e);
            //    return ("", "Something went wrong");
            //}
            //catch (System.Net.WebException ex)
            //{
            //    try
            //    {
            //        var responseString = await new System.IO.StreamReader(ex.Response.GetResponseStream()).ReadToEndAsync();
            //        logger.Log(LogLevel.Error, Logging.LoggingEvents.Authenticate, "getAccessTokenAsync:" + responseString, ex);
            //    }
            //    catch (Exception e)
            //    {
            //    }
            //    return ("", "Something went wrong");
            //}
            //catch (Exception e)
            //{
            //    logger.Log(LogLevel.Error, Logging.LoggingEvents.Authenticate, "getAccessTokenAsync", e);
            //    return (string.Empty, "Something went wrong");
            //}
        }
        //https://stackoverflow.com/questions/76460724/aad-outh-generate-token-failure-aadsts50076
        /*
         * The error usually occurs if you are trying to fetch access token using ROPC flow for MFA enabled user.

I have one user where MFA is enabled like below:

enter image description here

When I tried to generate access token using ROPC flow for this MFA enabled user, I got same error like below:

POST https://login.microsoftonline.com/<tenantID>/oauth2/token
grant_type:password
client_id:<appID>
client_secret:<secret>
resource: https://graph.microsoft.com
username: user1@xxxxxx.onmicrosoft.com
password: xxxxxxxxx
Response:

enter image description here

To resolve the error, you have to either disable MFA for that user or change your authentication flow to authorization code.

To generate access token using authorization code flow, run below authorization request in browser:

https://login.microsoftonline.com/tenantID/oauth2/v2.0/authorize? 
client_id=appID
&response_type=code  
&redirect_uri=https://jwt.ms
&response_mode=query  
&scope=https://graph.microsoft.com/.default
&state=12345

         */

        /*
         * generate access token successfully using authorization code flow for MFA enabled user via Postman with below parameters:

POST https://login.microsoftonline.com/tenantID/oauth2/token
grant_type:authorization_code
client_id:<appID>
client_secret:<secret>
resource:https://graph.microsoft.com
code:<paste_code_from_above_step>
redirect_uri:https://jwt.ms
         */

        class SessionViewModel
        {
            public string token_type { get; set; }
            public string scope { get; set; }
            public int expires_in { get; set; }
            public int ext_expires_in { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string id_token { get; set; }
        }

        public async System.Threading.Tasks.Task<(Microsoft.SearchBySubject.SearchBySubject_ViewModel, string)> SearchBySubjectAsync(Logging.Logger logger, string mailSubject)
        {
            if (string.IsNullOrEmpty(mailSubject))
            {
                return (null, "Mail Subject not exist");
            }
            var tokenObj = await getAccessTokenAsync(logger);
            if (string.IsNullOrEmpty(tokenObj.token))
            {
                return (null, tokenObj.msg);
            }
            try
            {
                var searchUrl = string.Format("https://graph.microsoft.com/v1.0/me/messages?$count=true&$select=subject,conversationId,receivedDateTime,toRecipients&$search=\"subject:{0}\"", mailSubject);
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenObj.token);

                var response = await client.GetAsync(searchUrl);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    try
                    {
                        var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Microsoft.SearchBySubject.SearchBySubject_ViewModel>(responseString);
                        if (responseObj != null)
                            return (responseObj, "");
                    }
                    catch (Exception e)
                    {

                    }
                return (null, responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.ListItems, "SearchBySubjectAsync:" + mailSubject, e);
                return (null, "Some thing went wrong");
            }
        }
        public async System.Threading.Tasks.Task<(Microsoft.SearchBySubject.SearchBySubject_ViewModel, string)> SearchByEmailAsync(Logging.Logger logger, string emailId)
        {
            if (string.IsNullOrEmpty(emailId))
            {
                return (null, "email not exist");
            }
            var tokenObj = await getAccessTokenAsync(logger);
            if (string.IsNullOrEmpty(tokenObj.token))
            {
                return (null, tokenObj.msg);
            }
            Microsoft.SearchBySubject.SearchBySubject_ViewModel resp = new Microsoft.SearchBySubject.SearchBySubject_ViewModel();
            string msg = "";
            Microsoft.SearchBySubject.SearchBySubject_ViewModel _resp;
            do
            {
                var tmp = await this._SearchByEmailAsync(tokenObj.token, logger, emailId, resp.OdataNextLink);
                _resp = tmp.Item1;
                msg = tmp.Item2;
                if (_resp != null)
                {
                    resp.OdataContext = _resp.OdataContext;
                    resp.OdataCount = _resp.OdataCount;
                    resp.OdataNextLink = _resp.OdataNextLink;
                    resp.value = (resp.value ?? new List<Microsoft.SearchBySubject.SearchBySubject_Value>());
                    resp.value.AddRange(_resp.value);
                }
                else
                {

                }
            } while (_resp != null && (string.IsNullOrEmpty(_resp.OdataNextLink) == false));
            return (resp, msg);
        }
        async System.Threading.Tasks.Task<(Microsoft.SearchBySubject.SearchBySubject_ViewModel, string)>
            _SearchByEmailAsync(string token, Logging.Logger logger, string emailId, string callURL)
        {
            try
            {
                var searchUrl = callURL ?? string.Format("https://graph.microsoft.com/v1.0/me/messages?$count=true&$select=subject,isRead,isDraft,conversationId&$search=\"participants:{0}\"", emailId);
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await client.GetAsync(searchUrl);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    try
                    {
                        var responseObj = JsonConvert.DeserializeObject<Microsoft.SearchBySubject.SearchBySubject_ViewModel>(responseString);
                        if (responseObj != null)
                            return (responseObj, "");
                    }
                    catch (Exception e)
                    {

                    }
                return (null, responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.ListItems, "SearchByEmailAsync:" + emailId, e);
                return (null, "Some thing went wrong");
            }
        }
        public async System.Threading.Tasks.Task<(Microsoft.GetMailsByConversation.GetMail_ViewModel, string)> GetMailsByConversationAsync(Logging.Logger logger, string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return (null, "ConversationId not exist");
            }
            var tokenObj = await getAccessTokenAsync(logger);
            if (string.IsNullOrEmpty(tokenObj.token))
            {
                return (null, tokenObj.msg);
            }
            try
            {
                var searchUrl = string.Format("https://graph.microsoft.com/v1.0/me/messages?$count=true&$filter=conversationId eq '{0}'", conversationId);
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenObj.token);
                var response = await client.GetAsync(searchUrl);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    try
                    {
                        var responseObj = JsonConvert.DeserializeObject<Microsoft.GetMailsByConversation.GetMail_ViewModel>(responseString);
                        if (responseObj != null)
                            return (responseObj, "");
                    }
                    catch (Exception e)
                    {

                    }
                return (null, responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.ListItems, "GetMailsByConversationAsync:" + conversationId, e);
                return (null, "Some thing went wrong");
            }
        }

        public async System.Threading.Tasks.Task<(Microsoft.GetMailAttachment.GetAttachment_ViewModel, string)> getAttachmentsAsync(Logging.Logger logger, string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return (null, "messageId not exist");
            }
            var tokenObj = await getAccessTokenAsync(logger);
            if (string.IsNullOrEmpty(tokenObj.token))
            {
                return (null, tokenObj.msg);
            }
            try
            {
                var searchUrl = string.Format("https://graph.microsoft.com/v1.0/me/messages/{0}/attachments", messageId);
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenObj.token);
                var response = await client.GetAsync(searchUrl);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    try
                    {
                        var responseObj = JsonConvert.DeserializeObject<Microsoft.GetMailAttachment.GetAttachment_ViewModel>(responseString);
                        if (responseObj != null)
                            return (responseObj, "");
                    }
                    catch (Exception e)
                    {

                    }
                return (null, responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.ListItems, "getAttachments:" + messageId, e);
                return (null, "Some thing went wrong");
            }
        }
        public async System.Threading.Tasks.Task<(bool? status, string response)> setMailStatusReadedAsync(Logging.Logger logger, string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return (null, "MessageId not exist");
            }
            var tokenObj = await getAccessTokenAsync(logger);
            if (string.IsNullOrEmpty(tokenObj.token))
            {
                return (null, tokenObj.msg);
            }
            try
            {
                var searchUrl = string.Format("https://graph.microsoft.com/v1.0/me/messages/{0}", messageId);
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenObj.token);
                var model = new { isRead = true };
                var dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                var httpContent = new System.Net.Http.StringContent(dataStr, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(searchUrl, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    try
                    {
                        logger.Log(LogLevel.Information, Logging.LoggingEvents.UpdateItem, "setMailsStatusReadedAsync:" + messageId + ", read status updation success");
                        //var responseObj = JsonConvert.DeserializeObject<Microsoft.GetMailsByConversation.GetMail_ViewModel>(responseString);
                        //if (responseObj != null)
                        return (true, "");
                    }
                    catch (Exception e)
                    {

                    }
                else
                    logger.Log(LogLevel.Information, Logging.LoggingEvents.UpdateItem, "setMailsStatusReadedAsync:" + messageId + ", read status updation failed." + responseString);
                return (false, responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.UpdateItem, "setMailsStatusReadedAsync:" + messageId, e);
                return (null, "Some thing went wrong");
            }
        }

        public async System.Threading.Tasks.Task<string> SendEmailAsync(Logging.Logger logger, Microsoft.SendMail.SendMail_ViewModel model, List<IFormFile> files)
        {
            if (model == null)
            {
                return ("Content missing");
            }
            var tokenObj = await getAccessTokenAsync(logger);
            if (string.IsNullOrEmpty(tokenObj.token))
            {
                return (tokenObj.msg);
            }

            try
            {
                model.saveToSentItems = true;
                model.message.attachments = new Microsoft.Microsoft_Attachments[] { };
                if (files != null && files.Count > 0)
                {
                    var attachments = new List<Microsoft.Microsoft_Attachments>();
                    foreach (var file in files)
                    {
                        using (var ms = new System.IO.MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            string s = Convert.ToBase64String(fileBytes);
                            attachments.Add(new Microsoft.Microsoft_Attachments
                            {
                                name = file.FileName,
                                contentBytes = s
                            });
                        }
                    }
                    model.message.attachments = attachments.ToArray();
                }

                var mainUrl = "https://graph.microsoft.com/v1.0/me/sendMail";
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenObj.token);
                var dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                var httpContent = new System.Net.Http.StringContent(dataStr, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(mainUrl, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    return ("");
                return (responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.InsertItem, "SendEmailAsync: model" + Newtonsoft.Json.JsonConvert.SerializeObject(model), e);
                return ("Some thing went wrong");
            }
        }
        public async System.Threading.Tasks.Task<string> ReplyEmailAsync(Logging.Logger logger, string messageId, Microsoft.ReplyMail.ReplyMail_ViewModel model, List<IFormFile> files)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return ("mail id missing");
            }
            var tokenObj = await getAccessTokenAsync(logger);
            if (string.IsNullOrEmpty(tokenObj.token))
            {
                return (tokenObj.msg);
            }

            try
            {
                model.message.attachments = new Microsoft.Microsoft_Attachments[] { };
                if (files != null && files.Count > 0)
                {
                    var attachments = new List<Microsoft.Microsoft_Attachments>();
                    foreach (var file in files)
                    {
                        using (var ms = new System.IO.MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            string s = Convert.ToBase64String(fileBytes);
                            attachments.Add(new Microsoft.Microsoft_Attachments
                            {
                                name = file.FileName,
                                contentBytes = s
                            });
                        }
                    }
                    model.message.attachments = attachments.ToArray();
                }
                var mailUrl = string.Format("https://graph.microsoft.com/v1.0/me/messages/{0}/reply", messageId);
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenObj.token);
                var dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                var httpContent = new System.Net.Http.StringContent(dataStr, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(mailUrl, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) return ("");
                return (responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.InsertItem, "ReplyEmailAsync:" + messageId + ", model" + Newtonsoft.Json.JsonConvert.SerializeObject(model), e);
                return ("Some thing went wrong");
            }
        }

        public async System.Threading.Tasks.Task<string> ReplyAllEmailAsync(Logging.Logger logger, string messageId, Microsoft.ReplyAllMail.ReplyAllMail_ViewModel model, List<IFormFile> files)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return ("mail id missing");
            }
            var tokenObj = await getAccessTokenAsync(logger);
            if (string.IsNullOrEmpty(tokenObj.token))
            {
                return (tokenObj.msg);
            }

            try
            {
                model.message.attachments = new Microsoft.Microsoft_Attachments[] { };
                if (files != null && files.Count > 0)
                {
                    var attachments = new List<Microsoft.Microsoft_Attachments>();
                    foreach (var file in files)
                    {
                        using (var ms = new System.IO.MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            string s = Convert.ToBase64String(fileBytes);
                            attachments.Add(new Microsoft.Microsoft_Attachments
                            {
                                name = file.FileName,
                                contentBytes = s
                            });
                        }
                    }
                    model.message.attachments = attachments.ToArray();
                }
                var mailUrl = string.Format("https://graph.microsoft.com/v1.0/me/messages/{0}/replyAll", messageId);
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenObj.token);
                var dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                var httpContent = new System.Net.Http.StringContent(dataStr, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(mailUrl, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) return ("");
                return (responseString);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logging.LoggingEvents.InsertItem, "ReplyAllEmailAsync:" + messageId + ", model" + Newtonsoft.Json.JsonConvert.SerializeObject(model), e);
                return ("Some thing went wrong");
            }
        }
    }
}
namespace PiHire.BAL.Common._3rdParty.Microsoft
{
    public class Microsoft_EmailAddress
    {
        public string address { get; set; }
        public string name { get; set; } = "";
    }
    public class Microsoft_ToRecipient
    {
        public Microsoft_EmailAddress emailAddress { get; set; }
    }
    public class Microsoft_Sender
    {
        public Microsoft_EmailAddress emailAddress { get; set; }
    }

    public class Microsoft_From
    {
        public Microsoft_EmailAddress emailAddress { get; set; }
    }

    public class Microsoft_CcRecipient
    {
        public Microsoft_EmailAddress emailAddress { get; set; }
    }
    public class Microsoft_BccRecipient
    {
        public Microsoft_EmailAddress emailAddress { get; set; }
    }
    public class Microsoft_ReplyTos
    {
        public Microsoft_EmailAddress emailAddress { get; set; }
    }
    public class Microsoft_Body
    {
        /// <summary>
        /// "Text"/"HTML"
        /// </summary>
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class Microsoft_Attachments
    {
        [Newtonsoft.Json.JsonProperty("@odata.type")]
        public string odataType { get; set; } = "#microsoft.graph.fileAttachment";
        public string name { get; set; }
        public string contentBytes { get; set; }
    }
}
namespace PiHire.BAL.Common._3rdParty.Microsoft.SearchBySubject
{
    public class SearchBySubject_Value
    {
        [Newtonsoft.Json.JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }
        public string id { get; set; }
        public string subject { get; set; }
        public bool isRead { get; set; }
        public bool isDraft { get; set; }
        public string conversationId { get; set; }
        public System.Collections.Generic.List<Microsoft_ToRecipient> toRecipients { get; set; }
    }
    public class SearchBySubject_ViewModel
    {
        [Newtonsoft.Json.JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        [Newtonsoft.Json.JsonProperty("@odata.count")]
        public int? OdataCount { get; set; }
        public System.Collections.Generic.List<SearchBySubject_Value> value { get; set; }
        [Newtonsoft.Json.JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }
    }

}
namespace PiHire.BAL.Common._3rdParty.Microsoft.GetMailsByConversation
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

    public class GetMailsByConversation_Flag
    {
        public string flagStatus { get; set; }
    }


    public class GetMailsByConversation_Value
    {
        [JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }
        public string id { get; set; }
        public DateTime? createdDateTime { get; set; }
        public DateTime? lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public List<object> categories { get; set; }
        public DateTime? receivedDateTime { get; set; }
        public DateTime? sentDateTime { get; set; }
        public bool? hasAttachments { get; set; }
        public string internetMessageId { get; set; }
        public string subject { get; set; }
        public string bodyPreview { get; set; }
        public string importance { get; set; }
        public string parentFolderId { get; set; }
        public string conversationId { get; set; }
        public string conversationIndex { get; set; }
        public bool? isDeliveryReceiptRequested { get; set; }
        public bool? isReadReceiptRequested { get; set; }
        public bool? isRead { get; set; }
        public bool? isDraft { get; set; }
        public string webLink { get; set; }
        public string inferenceClassification { get; set; }
        public Microsoft_Body body { get; set; }
        public Microsoft_Sender sender { get; set; }
        public Microsoft_From from { get; set; }
        public List<Microsoft_ToRecipient> toRecipients { get; set; }
        public List<Microsoft_CcRecipient> ccRecipients { get; set; }
        public List<Microsoft_BccRecipient> bccRecipients { get; set; }
        public List<Microsoft_ReplyTos> replyTo { get; set; }
        public GetMailsByConversation_Flag flag { get; set; }
    }

    public class GetMail_ViewModel
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.count")]
        public int OdataCount { get; set; }
        public List<GetMailsByConversation_Value> value { get; set; }
    }
}
namespace PiHire.BAL.Common._3rdParty.Microsoft.GetMailAttachment
{
    public class GetMailAttachment_Value
    {
        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }
        public string contentType { get; set; }
        public string contentLocation { get; set; }
        public string contentBytes { get; set; }
        public string contentId { get; set; }
        public string lastModifiedDateTime { get; set; }
        public string id { get; set; }
        public bool isInline { get; set; }
        public string name { get; set; }
        public int size { get; set; }
    }

    public class GetAttachment_ViewModel
    {
        public List<GetMailAttachment_Value> value { get; set; }
    }
}
namespace PiHire.BAL.Common._3rdParty.Microsoft.SendMail
{

    public class SendMail_Message
    {
        public string subject { get; set; }
        public Microsoft_Body body { get; set; }
        public Microsoft_Attachments[] attachments { get; set; } = new Microsoft_Attachments[] { };
        public List<Microsoft_ToRecipient> toRecipients { get; set; }
        public List<Microsoft_CcRecipient> ccRecipients { get; set; } = new List<Microsoft_CcRecipient>();
    }

    public class SendMail_ViewModel
    {
        public SendMail_Message message { get; set; }
        public bool saveToSentItems { get; set; } = true;
    }
}
namespace PiHire.BAL.Common._3rdParty.Microsoft.ReplyMail
{

    public class ReplyMail_Message
    {
        public Microsoft_Attachments[] attachments { get; set; } = new Microsoft_Attachments[] { };
        public List<Microsoft_ToRecipient> toRecipients { get; set; }
    }

    public class ReplyMail_ViewModel
    {
        public ReplyMail_Message message { get; set; }
        public string comment { get; set; }
    }
}

namespace PiHire.BAL.Common._3rdParty.Microsoft.ReplyAllMail
{

    public class ReplyAllMail_Message
    {
        public Microsoft_Attachments[] attachments { get; set; } = new Microsoft_Attachments[] { };
    }
    public class ReplyAllMail_ViewModel
    {
        public ReplyAllMail_Message message { get; set; }
        public string comment { get; set; }
    }
}
