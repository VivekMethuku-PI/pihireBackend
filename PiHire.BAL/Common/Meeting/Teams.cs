using PiHire.BAL.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PiHire.BAL.Common.Meeting
{

    public class Teams
    {
        private static AppSettings appSettings = new AppSettings();
        const string scopes = "openid%20offline_access%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.Read%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.Read.Shared%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.ReadWrite%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.ReadWrite.Shared%20https%3A%2F%2Fgraph.microsoft.com%2FOnlineMeetings.Read%20https%3A%2F%2Fgraph.microsoft.com%2FOnlineMeetings.ReadWrite%20https%3A%2F%2Fgraph.microsoft.com%2Fprofile%20https%3A%2F%2Fgraph.microsoft.com%2FUser.Read%20https%3A%2F%2Fgraph.microsoft.com%2Fmail.read";

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

        public static string getAuthorizeUrl(string RedirectUrl, string code)
        {
            //var authorizeUrl = "https://login.microsoftonline.com/" + teamTenantId + "/oauth2/v2.0/authorize?client_id=" + teamClientId +
            //                        "&response_type=code" +
            //                        "&redirect_uri=" + (RedirectUrl ?? "http%3A%2F%2Flocalhost%3A4200%2F") +
            //                        "&response_mode=query" +
            //                        "&scope=https%3A%2F%2Fgraph.microsoft.com%2F.default";

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
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft refresh token generating");
            var tokenUrl = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", teamTenantId);
            var request = (HttpWebRequest)WebRequest.Create(tokenUrl);
            var postData = "client_id=" + teamClientId +
                            "&scope=" + scopes +
                            "&code=" + AuthorizationCode +
                            "&redirect_uri=" + (RedirectUrl ?? "http%3A%2F%2Flocalhost%3A4200%2F") +
                            "&grant_type=authorization_code" +
                            "&client_secret=" + teamClientSecret;
            var data = Encoding.ASCII.GetBytes(postData);
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
                    logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft refresh token generating failed:" + responseString);
            }
            catch (Exception e)
            {
                logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft refresh token generating error:" + responseString, e);
            }
            return ("", responseString);
            //            var respStr = "{\"token_type\":\"Bearer\",\"scope\":\"profile openid email https://graph.microsoft.com/Calendars.Read https://graph.microsoft.com/Calendars.Read.Shared https://graph.microsoft.com/Calendars.ReadWrite https://graph.microsoft.com/Calendars.ReadWrite.Shared https://graph.microsoft.com/User.Read https://graph.microsoft.com/.default\",\"expires_in\":3599,\"ext_expires_in\":3599,\"access_token\":\"eyJ0eXAiOiJKV1QiLCJub25jZSI6IkIwRlNqd3hOalVlMEk1M3VPRERVTElYM2NJUU1aTTZlSnNITUlHWTdOUEkiLCJhbGciOiJSUzI1NiIsIng1dCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyIsImtpZCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyJ9.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8yNDY1MmRiYy01NTc2LTRjZjAtOWU5Ny1lYjQ1YjgwOTQwMWQvIiwiaWF0IjoxNjIwMDE4NDY3LCJuYmYiOjE2MjAwMTg0NjcsImV4cCI6MTYyMDAyMjM2NywiYWNjdCI6MCwiYWNyIjoiMSIsImFjcnMiOlsidXJuOnVzZXI6cmVnaXN0ZXJzZWN1cml0eWluZm8iLCJ1cm46bWljcm9zb2Z0OnJlcTEiLCJ1cm46bWljcm9zb2Z0OnJlcTIiLCJ1cm46bWljcm9zb2Z0OnJlcTMiLCJjMSIsImMyIiwiYzMiLCJjNCIsImM1IiwiYzYiLCJjNyIsImM4IiwiYzkiLCJjMTAiLCJjMTEiLCJjMTIiLCJjMTMiLCJjMTQiLCJjMTUiLCJjMTYiLCJjMTciLCJjMTgiLCJjMTkiLCJjMjAiLCJjMjEiLCJjMjIiLCJjMjMiLCJjMjQiLCJjMjUiXSwiYWlvIjoiRTJaZ1lQaDYwOVBUdnFjMTdMaWdiTWlybjRudkF4c256L283S2VseXhPSHcrbzl4MTVzQiIsImFtciI6WyJwd2QiXSwiYXBwX2Rpc3BsYXluYW1lIjoicGlIaXJlIERFVih3aXRoIGluIGNtcG55KSIsImFwcGlkIjoiYzlkZWNiNWEtZDFlZC00NGYyLWFjMmQtZmYyN2JjNTM0MjFmIiwiYXBwaWRhY3IiOiIxIiwiZmFtaWx5X25hbWUiOiIoUGFyYW1JbmZvKSIsImdpdmVuX25hbWUiOiJCYWxhamkgTmFyYWhhcmlzZXR0eSIsImlkdHlwIjoidXNlciIsImlwYWRkciI6IjI3LjYuMTc3LjExMyIsIm5hbWUiOiJCYWxhamkgTmFyYWhhcmlzZXR0eSAoUGFyYW1JbmZvKSIsIm9pZCI6Ijc1YjIwYzY5LTU3ODktNGNlOS1iM2RjLTgzYTg3MTlkODI5ZiIsInBsYXRmIjoiMyIsInB1aWQiOiIxMDAzMjAwMDM0OThDODc3IiwicmgiOiIwLkFWUUF2QzFsSkhaVjhFeWVsLXRGdUFsQUhWckwzc250MGZKRXJDM19KN3hUUWg5VUFONC4iLCJzY3AiOiJDYWxlbmRhcnMuUmVhZCBDYWxlbmRhcnMuUmVhZC5TaGFyZWQgQ2FsZW5kYXJzLlJlYWRXcml0ZSBDYWxlbmRhcnMuUmVhZFdyaXRlLlNoYXJlZCBVc2VyLlJlYWQgcHJvZmlsZSBvcGVuaWQgZW1haWwiLCJzaWduaW5fc3RhdGUiOlsia21zaSJdLCJzdWIiOiJOZE9yMV9TT3R0dld4UXdvRzdsZ2h1eV9PV25oX1FGdklSR19Kb3c2M3lBIiwidGVuYW50X3JlZ2lvbl9zY29wZSI6IkFTIiwidGlkIjoiMjQ2NTJkYmMtNTU3Ni00Y2YwLTllOTctZWI0NWI4MDk0MDFkIiwidW5pcXVlX25hbWUiOiJiYWxhamkubkBwYXJhbWluZm8uY29tIiwidXBuIjoiYmFsYWppLm5AcGFyYW1pbmZvLmNvbSIsInV0aSI6ImVjSF8ybkp4blV5NTg0Ml9FRG5FQVEiLCJ2ZXIiOiIxLjAiLCJ3aWRzIjpbImI3OWZiZjRkLTNlZjktNDY4OS04MTQzLTc2YjE5NGU4NTUwOSJdLCJ4bXNfc3QiOnsic3ViIjoicVRlVVFTa2FHUTktVmtfYmtYVHcxZ3N6Z3diNnpidUt2eGw1SGo0WFV0OCJ9LCJ4bXNfdGNkdCI6MTM3MDc3ODMxM30.ZQGJfGtNBdvKsiWzfvaZalo5yb_LBIz2i-Dn2sjLvwrtx_LJ7aWSS3gLH4NjMh-jGhfEpyUr171hM3DjWiRKARu5H0iM5KqNo_0cF6zdPxoBVpshBoyU6Ao8mapOa-Hgx7VR9woqKrsOs3QAl52JNSUkkU7j138VKH-pOkPB6cSwAxkJhyRj2l4fNI9K3QE5oMGt2tO8eOT3mExBz7f3xBA6szt2GmgnI1BdgVjUe5jaFigukAAQu9Re1HUlJmu8ROSsAGbJzsbB2Uxp72vCFC6-3Bde6kCZ4Va6pxez4cY1rV5XntWqtRxUN5r7aRByyLCsk_LhSrSacSkiWPo5aw\"}";
        }
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

        string getToken(string refreshToken, string RedirectUrl)
        {
            //var tokenUrl = string.Format("https://login.microsoftonline.com/common/oauth2/v2.0/token", teamTenantId);
            //var request = (HttpWebRequest)WebRequest.Create(tokenUrl);
            //var postData = "client_id=" + teamClientId +
            //                "&scope="+scopes +
            //                "&refresh_token=" + refreshToken +
            //                "&redirect_uri=" + (RedirectUrl ?? "http%3A%2F%2Flocalhost%3A4200%2F") +
            //                "&grant_type=refresh_token" +
            //                "&client_secret=" + teamClientSecret;

            //var data = Encoding.ASCII.GetBytes(postData);
            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = data.Length;
            //using (var stream = request.GetRequestStream())
            //{
            //    stream.Write(data, 0, data.Length);
            //}
            //var response = (HttpWebResponse)request.GetResponse();
            //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft token generating");
            var tokenUrl = string.Format("https://login.microsoftonline.com/common/oauth2/v2.0/token", teamTenantId);
            var request = (HttpWebRequest)WebRequest.Create(tokenUrl);
            var postData = "client_id=" + teamClientId +
                            "&scope=" + scopes +
                            "&refresh_token=" + refreshToken +
                            "&redirect_uri=" + (RedirectUrl ?? "http%3A%2F%2Flocalhost%3A4200%2F") +
                            "&grant_type=refresh_token" +
                            "&client_secret=" + teamClientSecret;
            postData = "client_id=" + teamClientId +
                        "&scope=openid%20offline_access%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.Read%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.Read.Shared%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.ReadWrite%20https%3A%2F%2Fgraph.microsoft.com%2FCalendars.ReadWrite.Shared%20https%3A%2F%2Fgraph.microsoft.com%2FOnlineMeetings.Read%20https%3A%2F%2Fgraph.microsoft.com%2FOnlineMeetings.ReadWrite%20https%3A%2F%2Fgraph.microsoft.com%2Fprofile%20https%3A%2F%2Fgraph.microsoft.com%2FUser.Read%20https%3A%2F%2Fgraph.microsoft.com%2Fmail.read" +
                        "&refresh_token=" + refreshToken +
                        "&redirect_uri=http%3A%2F%2Flocalhost%3A4200%2F" +
                        "&grant_type=refresh_token" +
                        "&client_secret=" + teamClientSecret;

            var data = Encoding.ASCII.GetBytes(postData);
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
                    return responseObj.access_token;
                else
                {
                    this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft token generating failed:" + responseString);
                    throw new Exception(responseString);
                }
            }
            catch (Exception e)
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft token generating error:" + responseString, e);
                throw e;
            }
        }

        private Logging.Logger logger;
        internal readonly string accessToken;
        //IConfidentialClientApplication confidentialClientApplication;
        //ClientCredentialProvider authProvider;
        //GraphServiceClient graphClient;
        //string[] scopes = new string[] { "api://e9cc6b8b-8640-4874-b17c-6b8e4da753cc/.default", "api://e9cc6b8b-8640-4874-b17c-6b8e4da753cc/Calendars.ReadWrite" };
        public Teams(string RefreshToken, string RedirectUrl, Logging.Logger logger)
        {
            this.logger = logger;

            if (string.IsNullOrEmpty(RefreshToken))
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, Logging.LoggingEvents.Other, "Microsoft team refresh token is missing");
                throw new Exception("Microsoft team refresh token is missing");
            }
            else if (string.IsNullOrEmpty(RedirectUrl))
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, Logging.LoggingEvents.Other, "Microsoft redirect url is missing");
                throw new Exception("Redirect url is missing");
            }
            this.accessToken = getToken(RefreshToken, RedirectUrl);
        }
        private HttpClient getHttpClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + sessionToken);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            return client;
        }

        class CreateEventViewModel
        {
            [Newtonsoft.Json.JsonProperty("@odata.context")]
            public string OdataContext { get; set; }

            [Newtonsoft.Json.JsonProperty("@odata.etag")]
            public string OdataEtag { get; set; }
            public string id { get; set; }
            public DateTime createdDateTime { get; set; }
            public DateTime lastModifiedDateTime { get; set; }
            public string changeKey { get; set; }
            public List<object> categories { get; set; }
            public object transactionId { get; set; }
            public string originalStartTimeZone { get; set; }
            public string originalEndTimeZone { get; set; }
            public string iCalUId { get; set; }
            public int reminderMinutesBeforeStart { get; set; }
            public bool isReminderOn { get; set; }
            public bool hasAttachments { get; set; }
            public string subject { get; set; }
            public string bodyPreview { get; set; }
            public string importance { get; set; }
            public string sensitivity { get; set; }
            public bool isAllDay { get; set; }
            public bool isCancelled { get; set; }
            public bool isOrganizer { get; set; }
            public bool responseRequested { get; set; }
            public object seriesMasterId { get; set; }
            public string showAs { get; set; }
            public string type { get; set; }
            public string webLink { get; set; }
            public object onlineMeetingUrl { get; set; }
            public bool isOnlineMeeting { get; set; }
            public string onlineMeetingProvider { get; set; }
            public bool allowNewTimeProposals { get; set; }
            public object occurrenceId { get; set; }
            public bool isDraft { get; set; }
            public bool hideAttendees { get; set; }
            //public ResponseStatus responseStatus { get; set; }
            //public Body body { get; set; }
            //public Start start { get; set; }
            //public End end { get; set; }
            //public Location location { get; set; }
            //public List<Location> locations { get; set; }
            //public object recurrence { get; set; }
            //public List<Attendee> attendees { get; set; }
            //public Organizer organizer { get; set; }
            //public OnlineMeeting onlineMeeting { get; set; }
        }
        public async System.Threading.Tasks.Task<(bool authorize, bool status, string msg, string eventId)> CreateEvent(
            string subject, string body, string location,
            DateTime start, DateTime end, bool newTimeAllowed,
            List<MeetingAttendeeEmailAddressViewModel> attendees, string timeZone)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft creating event:"
                    + ", subject:" + subject
                    + ", body:" + body
                    + ", location:" + location
                    + ", start:" + start
                    + ", end:" + end
                    + ", newTimeAllowed:" + newTimeAllowed
                    + ", attendees:" + (attendees == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(attendees))
                    + ", timeZone:" + timeZone);
            try
            {
                timeZone = timeZone ?? "India Standard Time";
                var postObj = new TeamMeetingViewModel
                {
                    subject = subject,
                    body = new TeamMeetingBodyViewModel { content = body, contentType = "HTML" },
                    start = new MeetingTimeViewModel { dateTime = start, timeZone = timeZone },
                    end = new MeetingTimeViewModel { dateTime = end, timeZone = timeZone },
                    allowNewTimeProposals = newTimeAllowed,
                    location = new TeamMeetingLocationViewModel { displayName = location },
                    
                    attendees = attendees.Select(da => new TeamMeetingAttendeeViewModel { type = da.type + "", emailAddress = new TeamMeetingAttendeeEmailAddressViewModel() { address = da.address, name = da.name } }).ToList(),
                    isOnlineMeeting = true,
                    onlineMeetingProvider = "teamsForBusiness"
                };
                var respStr = Newtonsoft.Json.JsonConvert.SerializeObject(postObj);

                HttpClient client = getHttpClient();
                //client.DefaultRequestHeaders.Add("Prefer", "outlook.timezone=\"Pacific Standard Time\"");
                HttpContent requestBody = new StringContent(respStr, Encoding.UTF8, "application/json");
                var resp = await client.PostAsync("https://graph.microsoft.com/v1.0/me/events", requestBody);
                //var tmp = resp.IsSuccessStatusCode;
                var respMsg = await resp.Content.ReadAsStringAsync();
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft creating event response:" + respMsg);
                try
                {
                    var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateEventViewModel>(respMsg);
                    if (responseObj != null)
                        return (resp.StatusCode != HttpStatusCode.Unauthorized, resp.IsSuccessStatusCode, respMsg, responseObj.id);
                    else
                    {
                        this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft creating event failed:" + respMsg);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft creating event error:" + respMsg, e);
                    throw e;
                }
                return (resp.StatusCode != HttpStatusCode.Unauthorized, resp.IsSuccessStatusCode, respMsg, "");
            }
            catch (Exception ex)
            {
                logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft creating event error:"
                    + ", subject:" + subject
                    + ", body:" + body
                    + ", location:" + location
                    + ", start:" + start
                    + ", end:" + end
                    + ", newTimeAllowed:" + newTimeAllowed
                    + ", attendees:" + (attendees == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(attendees))
                    + ", timeZone:" + timeZone, ex);
                throw;
            }
        }
        public async System.Threading.Tasks.Task<(bool authorize, bool status, string msg, string eventId)> UpdateEvent(string eventId,
            string subject, string body, string location,
            DateTime start, DateTime end, bool newTimeAllowed,
            List<MeetingAttendeeEmailAddressViewModel> attendees, string timeZone)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft updating event:" + eventId
                    + ", subject:" + subject
                    + ", body:" + body
                    + ", location:" + location
                    + ", start:" + start
                    + ", end:" + end
                    + ", newTimeAllowed:" + newTimeAllowed
                    + ", attendees:" + (attendees == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(attendees))
                    + ", timeZone:" + timeZone);
            try
            {
                timeZone = timeZone ?? "India Standard Time";
                var postObj = new TeamMeetingViewModel
                {
                    subject = subject,
                    body = new TeamMeetingBodyViewModel { content = body, contentType = "HTML" },
                    start = new MeetingTimeViewModel { dateTime = start, timeZone = timeZone },
                    end = new MeetingTimeViewModel { dateTime = end, timeZone = timeZone },
                    allowNewTimeProposals = newTimeAllowed,
                    location = new TeamMeetingLocationViewModel { displayName = location },

                    attendees = attendees.Select(da => new TeamMeetingAttendeeViewModel { type = da.type + "", emailAddress = new TeamMeetingAttendeeEmailAddressViewModel() { address = da.address, name = da.name } }).ToList(),
                    isOnlineMeeting = true,
                    onlineMeetingProvider = "teamsForBusiness"
                };
                var respStr = Newtonsoft.Json.JsonConvert.SerializeObject(postObj);
                string respMsg = string.Empty;
                HttpResponseMessage resp;
                do
                {
                    HttpClient client = getHttpClient();
                    //client.DefaultRequestHeaders.Add("Prefer", "outlook.timezone=\"Pacific Standard Time\"");
                    HttpContent requestBody = new StringContent(respStr, Encoding.UTF8, "application/json");
                    resp = await client.PatchAsync("https://graph.microsoft.com/v1.0/me/events/" + eventId, requestBody);
                    //var tmp = resp.IsSuccessStatusCode;
                    respMsg = await resp.Content.ReadAsStringAsync();
                    this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft updating event response:" + respMsg);
                } while (respMsg.IndexOf("\"onlineMeeting\":null") != -1);
                try
                {
                    var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateEventViewModel>(respMsg);
                    if (responseObj != null)
                        return (resp.StatusCode != HttpStatusCode.Unauthorized, resp.IsSuccessStatusCode, respMsg, responseObj.id);
                    else
                    {
                        this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft updating event failed:" + respMsg);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft updating event error:" + respMsg, e);
                    throw e;
                }
                return (resp.StatusCode != HttpStatusCode.Unauthorized, resp.IsSuccessStatusCode, respMsg, "");
            }
            catch (Exception ex)
            {
                logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft updating event error:" + eventId
                    + ", subject:" + subject
                    + ", body:" + body
                    + ", location:" + location
                    + ", start:" + start
                    + ", end:" + end
                    + ", newTimeAllowed:" + newTimeAllowed
                    + ", attendees:" + (attendees == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(attendees))
                    + ", timeZone:" + timeZone, ex);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<(bool authorize, bool status, string msg)> DeleteEvent(string eventId)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft delete event:" + "eventId:" + eventId);
            try
            {
                HttpClient client = getHttpClient();

                var resp = await client.DeleteAsync("https://graph.microsoft.com/v1.0/me/events/" + eventId);
                //var tmp = resp.IsSuccessStatusCode;
                var respMsg = await resp.Content.ReadAsStringAsync();
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Microsoft delete event complete:" + respMsg);
                return (resp.StatusCode != HttpStatusCode.Unauthorized, resp.IsSuccessStatusCode, respMsg);
            }
            catch (Exception ex)
            {
                logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft delete event error:" + "eventId:" + eventId, ex);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<TeamCalendarProfileViewModel> getProfile()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + tkn);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var resp = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
            //var tmp = resp.IsSuccessStatusCode;
            var responseString = await resp.Content.ReadAsStringAsync();
            this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft profile response:" + responseString);
            try
            {
                var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<TeamCalendarProfileViewModel>(responseString);
                if (responseObj != null)
                    return responseObj;
                else
                {
                    throw new Exception(responseString);
                }
            }
            catch (Exception e)
            {
                logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft profile error:", e);
                throw e;
            }
        }
        public async System.Threading.Tasks.Task<TeamCalendarTimeZoneViewModel> getTimezones()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + tkn);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var resp = await client.GetAsync("https://graph.microsoft.com/v1.0/me/outlook/supportedTimeZones");
            //var tmp = resp.IsSuccessStatusCode;
            var responseString = await resp.Content.ReadAsStringAsync();
            try
            {
                var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<TeamCalendarTimeZoneViewModel>(responseString);
                if (responseObj != null)
                    return responseObj;
                else
                {
                    throw new Exception(responseString);
                }
            }
            catch (Exception e)
            {
                logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Microsoft timezone error:", e);
                throw e;
            }
        }
    }
    public class TeamCalendarTimeZoneViewModel
    {
        [Newtonsoft.Json.JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        public List<TeamCalendarTimeZoneViewModel_value> value { get; set; }
    }
    public class TeamCalendarTimeZoneViewModel_value
    {
        public string alias { get; set; }
        public string displayName { get; set; }
    }
    public class TeamCalendarProfileViewModel
    {
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string mobilePhone { get; set; }
        public string preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
        public string id { get; set; }
    }

    public class TeamMeetingViewModel
    {
        public string subject { get; set; }
        public TeamMeetingBodyViewModel body { get; set; }
        public MeetingTimeViewModel start { get; set; }
        public MeetingTimeViewModel end { get; set; }
        public TeamMeetingLocationViewModel location { get; set; }
        public List<TeamMeetingAttendeeViewModel> attendees { get; set; }
        public bool allowNewTimeProposals { get; set; }
        public bool isOnlineMeeting { get; set; }
        public string onlineMeetingProvider { get; set; }
    }
    public class TeamMeetingBodyViewModel
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }
    public class JsonDateTimeConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public JsonDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        }
    }
    public class MeetingTimeViewModel
    {
        [Newtonsoft.Json.JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }
    public class TeamMeetingLocationViewModel
    {
        public string displayName { get; set; }
    }
    public class TeamMeetingAttendeeViewModel
    {
        public TeamMeetingAttendeeEmailAddressViewModel emailAddress { get; set; }
        /// <summary>
        /// required/optional
        /// </summary>
        public string type { get; set; }
    }
    public class TeamMeetingAttendeeEmailAddressViewModel
    {
        public string address { get; set; }
        public string name { get; set; }
    }
    public enum MeetingAttendeeType
    {
        required,
        optional
    }
    public class MeetingAttendeeEmailAddressViewModel
    {
        public string address { get; set; }
        public string name { get; set; }
        public MeetingAttendeeType type { get; set; }
    }
}
