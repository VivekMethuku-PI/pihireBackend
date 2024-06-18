using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace PiHire.BAL.Common.Meeting
{
    public class GoogleMeet
    {
        //static string[] Scopes = { CalendarService.Scope.Calendar };
        //static string ApplicationName = "piHire";
        //public void CreateEvent(string Summary, string Description, DateTime StartDate, DateTime EndDate, string CredentialsPath)
        //{
        //    try
        //    {
        //        UserCredential credential;
        //        using (var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read))
        //        {
        //            // The file token.json stores the user's access and refresh tokens, and is created
        //            // automatically when the authorization flow completes for the first time.
        //            string credPath = "token.json";
        //            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //                GoogleClientSecrets.Load(stream).Secrets,
        //                Scopes,
        //                "user",
        //                CancellationToken.None,
        //                new FileDataStore(credPath, true)).Result;
        //            //Console.WriteLine("Credential file saved to: " + credPath);
        //        }

        //        // Create Google Calendar API service.
        //        var service = new CalendarService(new BaseClientService.Initializer()
        //        {
        //            HttpClientInitializer = credential,
        //            ApplicationName = ApplicationName,
        //        });

        //        Event body = new Event();
        //        EventAttendee a = new EventAttendee();
        //        a.Email = "prathap.k@paraminfo.com";
        //        List<EventAttendee> attendes = new List<EventAttendee>();
        //        attendes.Add(a);
        //        body.Attendees = attendes;
        //        EventDateTime start = new EventDateTime();
        //        start.DateTime = StartDate;
        //        EventDateTime end = new EventDateTime();
        //        end.DateTime = EndDate;
        //        body.Start = start;
        //        body.End = end;
        //        body.Location = "Avengers Mansion";
        //        body.Summary = "Discussion about new Spidey suit";
        //        Event recurringEvent = service.Events.Insert(body, "primary").Execute();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        static string[] Scopes = { CalendarService.Scope.Calendar, "openid", "email" };
        static string ApplicationName = "piHire";
        const string credentialsJsonFileName = "pihire-calendar-credentials.json";
        public static string ExchangeAuthorizationCode(string code, string redirectUrl, Logging.Logger logger)
        {
            var cred = GetSettings();
            var dict = new Dictionary<string, string>();
            dict.Add("client_id", cred.web.client_id);
            dict.Add("client_secret", cred.web.client_secret);
            dict.Add("code", code);
            dict.Add("grant_type", "authorization_code");
            dict.Add("redirect_uri", redirectUrl);
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token") { Content = new FormUrlEncodedContent(dict) };
            var res = client.SendAsync(req).Result;
            if (res.IsSuccessStatusCode)
            {
                var msg = res.Content.ReadAsStringAsync().Result;
                logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, 0, "GoogleMeet-ExchangeAuthorizationCode:" + msg);
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleCredentialTokenViewModel_WithRefresh>(msg);
                return obj?.refresh_token;
            }
            else
            {
                var msg = res.Content.ReadAsStringAsync().Result;
                logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Authenticate, "GoogleMeet-ExchangeAuthorizationCode:" + msg);
            }
            return string.Empty;
        }
        public static GoogleOAuth_WebSite_CredentialViewModel GetSettings()
        {
            var _data = File.ReadAllText(credentialsJsonFileName);
            var cred = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleOAuth_WebSite_CredentialViewModel>(_data);
            if (cred != null)
            {
                cred.web.ReqAccessCode = "https://accounts.google.com/o/oauth2/v2/auth?access_type=offline&response_type=code&client_id=" + cred.web.client_id + "&redirect_uri=" + cred.web.redirect_uris[0] + "&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fcalendar%20openid%20email&state=";
                //https://accounts.google.com/o/oauth2/v2/auth?access_type=offline&response_type=code&client_id=598036275186-8n0knlgmevf87kfb46rhn21bs270e28i.apps.googleusercontent.com&redirect_uri=http%3A%2F%2F127.0.0.1%3A50425%2Fauthorize%2F&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fcalendar
                //cred.web.ReqAccessCode = "https://accounts.google.com/o/oauth2/v2/auth?scope=" + string.Join("+", Scopes) + "&include_granted_scopes=true&response_type=token&redirect_uri=" + cred.web.redirect_uris[0] + "&client_id=" + cred.web.client_id + "&state=";
            }
            return cred;
        }

        UserCredential Cred;
        Logging.Logger logger;
        public GoogleMeet(string RefreshToken, Logging.Logger logger)
        {
            this.logger = logger;
            if (string.IsNullOrEmpty(RefreshToken))
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, Logging.LoggingEvents.Other, "Google meet refresh token is missing");
                throw new Exception("Google meet refresh token is missing");
            }
            this.Cred = GetCredential(RefreshToken);
        }
        public GoogleMeet(UserCredential credential)
        {
            this.Cred = credential;
        }
        public const string accesstoken_prfx = "accesstoken_";
        UserCredential GetCredential(string RefreshToken)
        {
            //ServiceAccountCredential credential;
            UserCredential credential;
            var credPath = Path.Combine(Path.GetTempPath(), "piHire-service_GoogleToken.json" + DateTime.Now.Ticks);
            if (!Directory.Exists(credPath))
                Directory.CreateDirectory(credPath);

            {
                var tkn =
                    RefreshToken.StartsWith(accesstoken_prfx) ?
                    Newtonsoft.Json.JsonConvert.SerializeObject(new GoogleCredentialTokenViewModel { access_token = RefreshToken.Substring(accesstoken_prfx.Length), token_type = "Bearer" }) :
                    Newtonsoft.Json.JsonConvert.SerializeObject(new GoogleCredentialTokenViewModel_WithRefresh { access_token = RefreshToken.StartsWith(accesstoken_prfx) ? RefreshToken.Substring(accesstoken_prfx.Length) : string.Empty, refresh_token = RefreshToken.StartsWith(accesstoken_prfx) ? string.Empty : RefreshToken, token_type = "Bearer" });
                var fl = Path.Combine(credPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-piHire");
                if (File.Exists(fl))
                {
                    File.Delete(fl);
                }
                File.WriteAllLines(fl, new string[] { tkn });
            }
            // If modifying these scopes, delete your previously saved credentials
            // at ~/.credentials/calendar-dotnet-quickstart.json
            using (var stream =
                new FileStream(credentialsJsonFileName, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "piHire",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Google meet GetCredential is success");
            return credential;
        }

        public async System.Threading.Tasks.Task<(bool authorize, bool status, string msg, string eventId)> CreateEvent(
            string subject, string body, string location,
            DateTime start, DateTime end, //bool newTimeAllowed,
            List<MeetingAttendeeEmailAddressViewModel> attendees, string timeZone)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Google meet creating event:"
                    + ", subject:" + subject
                    + ", body:" + body
                    + ", location:" + location
                    + ", start:" + start
                    + ", end:" + end
                    + ", attendees:" + (attendees == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(attendees))
                    + ", timeZone:" + timeZone);
            try
            {
                start = ConvertToTimezone(timeZone ?? "IN", start);
                end = ConvertToTimezone(timeZone ?? "IN", end);
                // Create Google Calendar API service.
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = Cred,
                    ApplicationName = ApplicationName,
                });



                Event newEvent = new Event()
                {
                    //not working
                    //Organizer = new Event.OrganizerData
                    //{
                    //    DisplayName = "piHire Rec",
                    //    Email = "balaji.n@paraminfo.com"
                    //},
                    //Creator = new Event.CreatorData { DisplayName = "piHire creator", Email = "info@paraminfo.com" },
                    Locked = true,

                    GuestsCanInviteOthers = false,
                    GuestsCanModify = false,
                    GuestsCanSeeOtherGuests = false,

                    AnyoneCanAddSelf = false,
                    Attendees = attendees.Select(da => new EventAttendee() { Email = da.address, DisplayName = da.name, Optional = da.type == MeetingAttendeeType.optional }).ToList(),

                    Location = location,
                    //Locked
                    Summary = subject,
                    Visibility = "private",
                    Description = body,

                    Start = new EventDateTime()
                    {
                        DateTime = start/*,
                        TimeZone = "Asia/Dubai"*/
                    },
                    End = new EventDateTime()
                    {
                        DateTime = end/*,
                        TimeZone = "Asia/Dubai"*/
                    }

                };
                newEvent.ConferenceData = new ConferenceData
                {
                    //ConferenceId = "gdg-jkds-kds",//
                    //ConferenceSolution = new ConferenceSolution { Name = "piHire Google Meet" },
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = DateTime.Now.Ticks.ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey
                        {
                            Type = "hangoutsMeet"
                        }
                    }
                    //Notes = "piHire Google Meet notes",
                    //Signature = "piHire Google Meet signature"
                };
                EventsResource.InsertRequest request
                 = service.Events.Insert(newEvent, "primary");
                request.SendNotifications = true;
                request.ConferenceDataVersion = 1;
                Event recurringEvent = await request.ExecuteAsync();
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Google meet creating event complete with:", recurringEvent.Id);

                return (false, true, "", recurringEvent.Id);
            }
            catch (TokenResponseException ex)
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Google meet creating event error:", ex);
                return (true, false, "Unauthorized/expired", "");
                //ex.Error.Error== "invalid_grant"
            }
            catch (Exception e)
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Google meet creating event error:", e);
                throw;
            }
        }
        public async System.Threading.Tasks.Task<(bool authorize, bool status, string msg, string eventId)> UpdateEvent(string eventId,
            string subject, string body, string location,
            DateTime start, DateTime end, //bool newTimeAllowed,
            List<MeetingAttendeeEmailAddressViewModel> attendees, string timeZone)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Google meet updating event:" + eventId
                    + ", subject:" + subject
                    + ", body:" + body
                    + ", location:" + location
                    + ", start:" + start
                    + ", end:" + end
                    + ", attendees:" + (attendees == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(attendees))
                    + ", timeZone:" + timeZone);
            try
            {
                start = ConvertToTimezone(timeZone ?? "IN", start);
                end = ConvertToTimezone(timeZone ?? "IN", end);
                // Update Google Calendar API service.
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = Cred,
                    ApplicationName = ApplicationName,
                });



                Event newEvent = new Event()
                {
                    //not working
                    //Organizer = new Event.OrganizerData
                    //{
                    //    DisplayName = "piHire Rec",
                    //    Email = "balaji.n@paraminfo.com"
                    //},
                    //Creator = new Event.CreatorData { DisplayName = "piHire creator", Email = "info@paraminfo.com" },
                    Locked = true,

                    GuestsCanInviteOthers = false,
                    GuestsCanModify = false,
                    GuestsCanSeeOtherGuests = false,

                    AnyoneCanAddSelf = false,
                    Attendees = attendees.Select(da => new EventAttendee() { Email = da.address, DisplayName = da.name, Optional = da.type == MeetingAttendeeType.optional }).ToList(),

                    Location = location,
                    //Locked
                    Summary = subject,
                    Visibility = "private",
                    Description = body,

                    Start = new EventDateTime()
                    {
                        DateTime = start/*,
                        TimeZone = "Asia/Dubai"*/
                    },
                    End = new EventDateTime()
                    {
                        DateTime = end/*,
                        TimeZone = "Asia/Dubai"*/
                    }

                };
                newEvent.ConferenceData = new ConferenceData
                {
                    //ConferenceId = "gdg-jkds-kds",//
                    //ConferenceSolution = new ConferenceSolution { Name = "piHire Google Meet" },
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = DateTime.Now.Ticks.ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey
                        {
                            Type = "hangoutsMeet"
                        }
                    }
                    //Notes = "piHire Google Meet notes",
                    //Signature = "piHire Google Meet signature"
                };
                EventsResource.UpdateRequest request
                 = service.Events.Update(newEvent, "primary", eventId);
                request.SendNotifications = true;
                request.ConferenceDataVersion = 1;
                Event recurringEvent = await request.ExecuteAsync();
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Google meet updating event complete with:", recurringEvent.Id);

                return (false, true, "", recurringEvent.Id);
            }
            catch (TokenResponseException ex)
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Google meet updating event error:", ex);
                return (true, false, "Unauthorized/expired", "");
                //ex.Error.Error== "invalid_grant"
            }
            catch (Exception e)
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Google meet updating event error:", e);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<(bool authorize, bool status, string msg)> DeleteEvent(string eventId)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Google meet delete event:" + "eventId:" + eventId);
            try
            {
                // Delete Google Calendar API service.
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = Cred,
                    ApplicationName = ApplicationName,
                });

                EventsResource.DeleteRequest request
                 = service.Events.Delete("primary", eventId);
                request.SendNotifications = true;
                string recurringEvent = await request.ExecuteAsync();
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, Logging.LoggingEvents.Other, "Google meet delete event complete with:", recurringEvent);

                return (false, true, "");
            }
            catch (TokenResponseException ex)
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Google meet delete event error:", ex);
                return (true, false, "Unauthorized/expired");
                //ex.Error.Error== "invalid_grant"
            }
            catch (Exception e)
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Google meet delete event error:", e);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<Google.Apis.Oauth2.v2.Data.Userinfoplus> getProfile()
        {
            try
            {
                var oauthSerivce =
                             new Google.Apis.Oauth2.v2.Oauth2Service(new BaseClientService.Initializer { HttpClientInitializer = Cred });
                Google.Apis.Oauth2.v2.Data.Userinfoplus UserInfo = await oauthSerivce.Userinfo.Get().ExecuteAsync();
                if (UserInfo != null)
                    return UserInfo;
                else
                {
                    throw new Exception("User details not provided");
                }
            }
            catch (Exception e)
            {
                this.logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, Logging.LoggingEvents.Other, "Google meet profile error:", e);
                throw e;
            }
        }
        internal static string getAuthorizeUrl(string code)
        {
            var settingObj = GetSettings();
            return settingObj.web.ReqAccessCode + code;
        }
        //https://en.wikipedia.org/wiki/List_of_tz_database_time_zones
        public static List<TeamCalendarTimeZoneViewModel_value> getTimezones()
        {
            var zns = new List<TeamCalendarTimeZoneViewModel_value>
            {
                new TeamCalendarTimeZoneViewModel_value{alias="CI", displayName="Africa/Abidjan"},
new TeamCalendarTimeZoneViewModel_value{alias="GH", displayName="Africa/Accra"},
new TeamCalendarTimeZoneViewModel_value{alias="ET", displayName="Africa/Addis_Ababa"},
new TeamCalendarTimeZoneViewModel_value{alias="DZ", displayName="Africa/Algiers"},
new TeamCalendarTimeZoneViewModel_value{alias="ER", displayName="Africa/Asmara"},
new TeamCalendarTimeZoneViewModel_value{alias="ML", displayName="Africa/Bamako"},
new TeamCalendarTimeZoneViewModel_value{alias="CF", displayName="Africa/Bangui"},
new TeamCalendarTimeZoneViewModel_value{alias="GM", displayName="Africa/Banjul"},
new TeamCalendarTimeZoneViewModel_value{alias="GW", displayName="Africa/Bissau"},
new TeamCalendarTimeZoneViewModel_value{alias="MW", displayName="Africa/Blantyre"},
new TeamCalendarTimeZoneViewModel_value{alias="CG", displayName="Africa/Brazzaville"},
new TeamCalendarTimeZoneViewModel_value{alias="BI", displayName="Africa/Bujumbura"},
new TeamCalendarTimeZoneViewModel_value{alias="EG", displayName="Africa/Cairo"},
new TeamCalendarTimeZoneViewModel_value{alias="MA", displayName="Africa/Casablanca"},
new TeamCalendarTimeZoneViewModel_value{alias="ES", displayName="Africa/Ceuta"},
new TeamCalendarTimeZoneViewModel_value{alias="GN", displayName="Africa/Conakry"},
new TeamCalendarTimeZoneViewModel_value{alias="SN", displayName="Africa/Dakar"},
new TeamCalendarTimeZoneViewModel_value{alias="TZ", displayName="Africa/Dar_es_Salaam"},
new TeamCalendarTimeZoneViewModel_value{alias="DJ", displayName="Africa/Djibouti"},
new TeamCalendarTimeZoneViewModel_value{alias="CM", displayName="Africa/Douala"},
new TeamCalendarTimeZoneViewModel_value{alias="EH", displayName="Africa/El_Aaiun"},
new TeamCalendarTimeZoneViewModel_value{alias="SL", displayName="Africa/Freetown"},
new TeamCalendarTimeZoneViewModel_value{alias="BW", displayName="Africa/Gaborone"},
new TeamCalendarTimeZoneViewModel_value{alias="ZW", displayName="Africa/Harare"},
new TeamCalendarTimeZoneViewModel_value{alias="ZA", displayName="Africa/Johannesburg"},
new TeamCalendarTimeZoneViewModel_value{alias="SS", displayName="Africa/Juba"},
new TeamCalendarTimeZoneViewModel_value{alias="UG", displayName="Africa/Kampala"},
new TeamCalendarTimeZoneViewModel_value{alias="SD", displayName="Africa/Khartoum"},
new TeamCalendarTimeZoneViewModel_value{alias="RW", displayName="Africa/Kigali"},
new TeamCalendarTimeZoneViewModel_value{alias="CD", displayName="Africa/Kinshasa"},
new TeamCalendarTimeZoneViewModel_value{alias="NG", displayName="Africa/Lagos"},
new TeamCalendarTimeZoneViewModel_value{alias="GA", displayName="Africa/Libreville"},
new TeamCalendarTimeZoneViewModel_value{alias="TG", displayName="Africa/Lome                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="AO", displayName="Africa/Luanda                               "},
new TeamCalendarTimeZoneViewModel_value{alias="CD2", displayName="Africa/Lubumbashi                           "},
new TeamCalendarTimeZoneViewModel_value{alias="ZM", displayName="Africa/Lusaka                               "},
new TeamCalendarTimeZoneViewModel_value{alias="GQ", displayName="Africa/Malabo                               "},
new TeamCalendarTimeZoneViewModel_value{alias="MZ", displayName="Africa/Maputo                               "},
new TeamCalendarTimeZoneViewModel_value{alias="LS", displayName="Africa/Maseru                               "},
new TeamCalendarTimeZoneViewModel_value{alias="SZ", displayName="Africa/Mbabane                              "},
new TeamCalendarTimeZoneViewModel_value{alias="SO", displayName="Africa/Mogadishu                            "},
new TeamCalendarTimeZoneViewModel_value{alias="LR", displayName="Africa/Monrovia                             "},
new TeamCalendarTimeZoneViewModel_value{alias="KE", displayName="Africa/Nairobi                              "},
new TeamCalendarTimeZoneViewModel_value{alias="TD", displayName="Africa/Ndjamena                             "},
new TeamCalendarTimeZoneViewModel_value{alias="NE", displayName="Africa/Niamey                               "},
new TeamCalendarTimeZoneViewModel_value{alias="MR", displayName="Africa/Nouakchott                           "},
new TeamCalendarTimeZoneViewModel_value{alias="BF", displayName="Africa/Ouagadougou                          "},
new TeamCalendarTimeZoneViewModel_value{alias="BJ", displayName="Africa/Porto-Novo                           "},
new TeamCalendarTimeZoneViewModel_value{alias="ST", displayName="Africa/Sao_Tome                             "},
new TeamCalendarTimeZoneViewModel_value{alias="LY", displayName="Africa/Tripoli                              "},
new TeamCalendarTimeZoneViewModel_value{alias="TN", displayName="Africa/Tunis                                "},
new TeamCalendarTimeZoneViewModel_value{alias="NA", displayName="Africa/Windhoek                             "},
new TeamCalendarTimeZoneViewModel_value{alias="US", displayName="America/Adak                                "},
new TeamCalendarTimeZoneViewModel_value{alias="US2", displayName="America/Anchorage                           "},
new TeamCalendarTimeZoneViewModel_value{alias="AI", displayName="America/Anguilla                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AG", displayName="America/Antigua                             "},
new TeamCalendarTimeZoneViewModel_value{alias="BR", displayName="America/Araguaina                           "},
new TeamCalendarTimeZoneViewModel_value{alias="AR", displayName="America/Argentina/Buenos_Aires              "},
new TeamCalendarTimeZoneViewModel_value{alias="AR2", displayName="America/Argentina/Catamarca                 "},
new TeamCalendarTimeZoneViewModel_value{alias="AR3", displayName="America/Argentina/Cordoba                   "},
new TeamCalendarTimeZoneViewModel_value{alias="AR4", displayName="America/Argentina/Jujuy                     "},
new TeamCalendarTimeZoneViewModel_value{alias="AR5", displayName="America/Argentina/La_Rioja                  "},
new TeamCalendarTimeZoneViewModel_value{alias="AR6", displayName="America/Argentina/Mendoza                   "},
new TeamCalendarTimeZoneViewModel_value{alias="AR7", displayName="America/Argentina/Rio_Gallegos              "},
new TeamCalendarTimeZoneViewModel_value{alias="AR8", displayName="America/Argentina/Salta                     "},
new TeamCalendarTimeZoneViewModel_value{alias="AR9", displayName="America/Argentina/San_Juan                  "},
new TeamCalendarTimeZoneViewModel_value{alias="AR10", displayName="America/Argentina/San_Luis                  "},
new TeamCalendarTimeZoneViewModel_value{alias="AR11", displayName="America/Argentina/Tucuman                   "},
new TeamCalendarTimeZoneViewModel_value{alias="AR12", displayName="America/Argentina/Ushuaia                   "},
new TeamCalendarTimeZoneViewModel_value{alias="AW", displayName="America/Aruba                               "},
new TeamCalendarTimeZoneViewModel_value{alias="PY", displayName="America/Asuncion                            "},
new TeamCalendarTimeZoneViewModel_value{alias="CA", displayName="America/Atikokan                            "},
new TeamCalendarTimeZoneViewModel_value{alias="BR2", displayName="America/Bahia                               "},
new TeamCalendarTimeZoneViewModel_value{alias="MX", displayName="America/Bahia_Banderas                      "},
new TeamCalendarTimeZoneViewModel_value{alias="BB", displayName="America/Barbados                            "},
new TeamCalendarTimeZoneViewModel_value{alias="BR3", displayName="America/Belem                               "},
new TeamCalendarTimeZoneViewModel_value{alias="BZ", displayName="America/Belize                              "},
new TeamCalendarTimeZoneViewModel_value{alias="CA2", displayName="America/Blanc-Sablon                        "},
new TeamCalendarTimeZoneViewModel_value{alias="BR4", displayName="America/Boa_Vista                           "},
new TeamCalendarTimeZoneViewModel_value{alias="CO", displayName="America/Bogota                              "},
new TeamCalendarTimeZoneViewModel_value{alias="US3", displayName="America/Boise                               "},
new TeamCalendarTimeZoneViewModel_value{alias="CA3", displayName="America/Cambridge_Bay                       "},
new TeamCalendarTimeZoneViewModel_value{alias="BR5", displayName="America/Campo_Grande                        "},
new TeamCalendarTimeZoneViewModel_value{alias="MX2", displayName="America/Cancun                              "},
new TeamCalendarTimeZoneViewModel_value{alias="VE", displayName="America/Caracas                             "},
new TeamCalendarTimeZoneViewModel_value{alias="GF", displayName="America/Cayenne                             "},
new TeamCalendarTimeZoneViewModel_value{alias="KY", displayName="America/Cayman                              "},
new TeamCalendarTimeZoneViewModel_value{alias="US4", displayName="America/Chicago                             "},
new TeamCalendarTimeZoneViewModel_value{alias="MX3", displayName="America/Chihuahua                           "},
new TeamCalendarTimeZoneViewModel_value{alias="CR", displayName="America/Costa_Rica                          "},
new TeamCalendarTimeZoneViewModel_value{alias="CA4", displayName="America/Creston                             "},
new TeamCalendarTimeZoneViewModel_value{alias="BR6", displayName="America/Cuiaba                              "},
new TeamCalendarTimeZoneViewModel_value{alias="CW", displayName="America/Curacao                             "},
new TeamCalendarTimeZoneViewModel_value{alias="GL", displayName="America/Danmarkshavn                        "},
new TeamCalendarTimeZoneViewModel_value{alias="CA5", displayName="America/Dawson                              "},
new TeamCalendarTimeZoneViewModel_value{alias="CA6", displayName="America/Dawson_Creek                        "},
new TeamCalendarTimeZoneViewModel_value{alias="US5", displayName="America/Denver                              "},
new TeamCalendarTimeZoneViewModel_value{alias="US6", displayName="America/Detroit                             "},
new TeamCalendarTimeZoneViewModel_value{alias="DM", displayName="America/Dominica                            "},
new TeamCalendarTimeZoneViewModel_value{alias="CA7", displayName="America/Edmonton                            "},
new TeamCalendarTimeZoneViewModel_value{alias="BR7", displayName="America/Eirunepe                            "},
new TeamCalendarTimeZoneViewModel_value{alias="SV", displayName="America/El_Salvador                         "},
new TeamCalendarTimeZoneViewModel_value{alias="CA8", displayName="America/Fort_Nelson                         "},
new TeamCalendarTimeZoneViewModel_value{alias="BR8", displayName="America/Fortaleza                           "},
new TeamCalendarTimeZoneViewModel_value{alias="CA9", displayName="America/Glace_Bay                           "},
new TeamCalendarTimeZoneViewModel_value{alias="CA10", displayName="America/Goose_Bay                           "},
new TeamCalendarTimeZoneViewModel_value{alias="TC", displayName="America/Grand_Turk                          "},
new TeamCalendarTimeZoneViewModel_value{alias="GD", displayName="America/Grenada                             "},
new TeamCalendarTimeZoneViewModel_value{alias="GP", displayName="America/Guadeloupe                          "},
new TeamCalendarTimeZoneViewModel_value{alias="GT", displayName="America/Guatemala                           "},
new TeamCalendarTimeZoneViewModel_value{alias="EC", displayName="America/Guayaquil                           "},
new TeamCalendarTimeZoneViewModel_value{alias="GY", displayName="America/Guyana                              "},
new TeamCalendarTimeZoneViewModel_value{alias="CA11", displayName="America/Halifax                             "},
new TeamCalendarTimeZoneViewModel_value{alias="CU", displayName="America/Havana                              "},
new TeamCalendarTimeZoneViewModel_value{alias="MX4", displayName="America/Hermosillo                          "},
new TeamCalendarTimeZoneViewModel_value{alias="US7", displayName="America/Indiana/Indianapolis                "},
new TeamCalendarTimeZoneViewModel_value{alias="US8", displayName="America/Indiana/Knox                        "},
new TeamCalendarTimeZoneViewModel_value{alias="US9", displayName="America/Indiana/Marengo                     "},
new TeamCalendarTimeZoneViewModel_value{alias="US10", displayName="America/Indiana/Petersburg                  "},
new TeamCalendarTimeZoneViewModel_value{alias="US11", displayName="America/Indiana/Tell_City                   "},
new TeamCalendarTimeZoneViewModel_value{alias="US12", displayName="America/Indiana/Vevay                       "},
new TeamCalendarTimeZoneViewModel_value{alias="US13", displayName="America/Indiana/Vincennes                   "},
new TeamCalendarTimeZoneViewModel_value{alias="US14", displayName="America/Indiana/Winamac                     "},
new TeamCalendarTimeZoneViewModel_value{alias="CA12", displayName="America/Inuvik                              "},
new TeamCalendarTimeZoneViewModel_value{alias="CA13", displayName="America/Iqaluit                             "},
new TeamCalendarTimeZoneViewModel_value{alias="JM", displayName="America/Jamaica                             "},
new TeamCalendarTimeZoneViewModel_value{alias="US15", displayName="America/Juneau                              "},
new TeamCalendarTimeZoneViewModel_value{alias="US16", displayName="America/Kentucky/Louisville                 "},
new TeamCalendarTimeZoneViewModel_value{alias="US17", displayName="America/Kentucky/Monticello                 "},
new TeamCalendarTimeZoneViewModel_value{alias="BQ", displayName="America/Kralendijk                          "},
new TeamCalendarTimeZoneViewModel_value{alias="BO", displayName="America/La_Paz                              "},
new TeamCalendarTimeZoneViewModel_value{alias="PE", displayName="America/Lima                                "},
new TeamCalendarTimeZoneViewModel_value{alias="US18", displayName="America/Los_Angeles                         "},
new TeamCalendarTimeZoneViewModel_value{alias="SX", displayName="America/Lower_Princes                       "},
new TeamCalendarTimeZoneViewModel_value{alias="BR9", displayName="America/Maceio                              "},
new TeamCalendarTimeZoneViewModel_value{alias="NI", displayName="America/Managua                             "},
new TeamCalendarTimeZoneViewModel_value{alias="BR10", displayName="America/Manaus                              "},
new TeamCalendarTimeZoneViewModel_value{alias="MF", displayName="America/Marigot                             "},
new TeamCalendarTimeZoneViewModel_value{alias="MQ", displayName="America/Martinique                          "},
new TeamCalendarTimeZoneViewModel_value{alias="MX5", displayName="America/Matamoros                           "},
new TeamCalendarTimeZoneViewModel_value{alias="MX6", displayName="America/Mazatlan                            "},
new TeamCalendarTimeZoneViewModel_value{alias="US19", displayName="America/Menominee                           "},
new TeamCalendarTimeZoneViewModel_value{alias="MX7", displayName="America/Merida                              "},
new TeamCalendarTimeZoneViewModel_value{alias="US20", displayName="America/Metlakatla                          "},
new TeamCalendarTimeZoneViewModel_value{alias="MX8", displayName="America/Mexico_City                         "},
new TeamCalendarTimeZoneViewModel_value{alias="PM", displayName="America/Miquelon                            "},
new TeamCalendarTimeZoneViewModel_value{alias="CA14", displayName="America/Moncton                             "},
new TeamCalendarTimeZoneViewModel_value{alias="MX9", displayName="America/Monterrey                           "},
new TeamCalendarTimeZoneViewModel_value{alias="UY", displayName="America/Montevideo                          "},
new TeamCalendarTimeZoneViewModel_value{alias="MS", displayName="America/Montserrat                          "},
new TeamCalendarTimeZoneViewModel_value{alias="BS", displayName="America/Nassau                              "},
new TeamCalendarTimeZoneViewModel_value{alias="US21", displayName="America/New_York                            "},
new TeamCalendarTimeZoneViewModel_value{alias="CA15", displayName="America/Nipigon                             "},
new TeamCalendarTimeZoneViewModel_value{alias="US22", displayName="America/Nome                                "},
new TeamCalendarTimeZoneViewModel_value{alias="BR11", displayName="America/Noronha                             "},
new TeamCalendarTimeZoneViewModel_value{alias="US23", displayName="America/North_Dakota/Beulah                 "},
new TeamCalendarTimeZoneViewModel_value{alias="US24", displayName="America/North_Dakota/Center                 "},
new TeamCalendarTimeZoneViewModel_value{alias="US25", displayName="America/North_Dakota/New_Salem              "},
new TeamCalendarTimeZoneViewModel_value{alias="GL2", displayName="America/Nuuk                                "},
new TeamCalendarTimeZoneViewModel_value{alias="MX10", displayName="America/Ojinaga                             "},
new TeamCalendarTimeZoneViewModel_value{alias="PA", displayName="America/Panama                              "},
new TeamCalendarTimeZoneViewModel_value{alias="CA16", displayName="America/Pangnirtung                         "},
new TeamCalendarTimeZoneViewModel_value{alias="SR", displayName="America/Paramaribo                          "},
new TeamCalendarTimeZoneViewModel_value{alias="US26", displayName="America/Phoenix                             "},
new TeamCalendarTimeZoneViewModel_value{alias="TT", displayName="America/Port_of_Spain                       "},
new TeamCalendarTimeZoneViewModel_value{alias="HT", displayName="America/Port-au-Prince                      "},
new TeamCalendarTimeZoneViewModel_value{alias="BR12", displayName="America/Porto_Velho                         "},
new TeamCalendarTimeZoneViewModel_value{alias="PR", displayName="America/Puerto_Rico                         "},
new TeamCalendarTimeZoneViewModel_value{alias="CL", displayName="America/Punta_Arenas                        "},
new TeamCalendarTimeZoneViewModel_value{alias="CA17", displayName="America/Rainy_River                         "},
new TeamCalendarTimeZoneViewModel_value{alias="CA18", displayName="America/Rankin_Inlet                        "},
new TeamCalendarTimeZoneViewModel_value{alias="BR13", displayName="America/Recife                              "},
new TeamCalendarTimeZoneViewModel_value{alias="CA19", displayName="America/Regina                              "},
new TeamCalendarTimeZoneViewModel_value{alias="CA20", displayName="America/Resolute                            "},
new TeamCalendarTimeZoneViewModel_value{alias="BR14", displayName="America/Rio_Branco                          "},
new TeamCalendarTimeZoneViewModel_value{alias="BR15", displayName="America/Santarem                            "},
new TeamCalendarTimeZoneViewModel_value{alias="CL2", displayName="America/Santiago                            "},
new TeamCalendarTimeZoneViewModel_value{alias="DO", displayName="America/Santo_Domingo                       "},
new TeamCalendarTimeZoneViewModel_value{alias="BR16", displayName="America/Sao_Paulo                           "},
new TeamCalendarTimeZoneViewModel_value{alias="GL3", displayName="America/Scoresbysund                        "},
new TeamCalendarTimeZoneViewModel_value{alias="US27", displayName="America/Sitka                               "},
new TeamCalendarTimeZoneViewModel_value{alias="BL", displayName="America/St_Barthelemy                       "},
new TeamCalendarTimeZoneViewModel_value{alias="CA21", displayName="America/St_Johns                            "},
new TeamCalendarTimeZoneViewModel_value{alias="KN", displayName="America/St_Kitts                            "},
new TeamCalendarTimeZoneViewModel_value{alias="LC", displayName="America/St_Lucia                            "},
new TeamCalendarTimeZoneViewModel_value{alias="VI", displayName="America/St_Thomas                           "},
new TeamCalendarTimeZoneViewModel_value{alias="VC", displayName="America/St_Vincent                          "},
new TeamCalendarTimeZoneViewModel_value{alias="CA22", displayName="America/Swift_Current                       "},
new TeamCalendarTimeZoneViewModel_value{alias="HN", displayName="America/Tegucigalpa                         "},
new TeamCalendarTimeZoneViewModel_value{alias="GL4", displayName="America/Thule                               "},
new TeamCalendarTimeZoneViewModel_value{alias="CA23", displayName="America/Thunder_Bay                         "},
new TeamCalendarTimeZoneViewModel_value{alias="MX11", displayName="America/Tijuana                             "},
new TeamCalendarTimeZoneViewModel_value{alias="CA24", displayName="America/Toronto                             "},
new TeamCalendarTimeZoneViewModel_value{alias="VG", displayName="America/Tortola                             "},
new TeamCalendarTimeZoneViewModel_value{alias="CA25", displayName="America/Vancouver                           "},
new TeamCalendarTimeZoneViewModel_value{alias="CA26", displayName="America/Whitehorse                          "},
new TeamCalendarTimeZoneViewModel_value{alias="CA27", displayName="America/Winnipeg                            "},
new TeamCalendarTimeZoneViewModel_value{alias="US28", displayName="America/Yakutat                             "},
new TeamCalendarTimeZoneViewModel_value{alias="CA28", displayName="America/Yellowknife                         "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ", displayName="Antarctica/Casey                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ2", displayName="Antarctica/Davis                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ3", displayName="Antarctica/DumontDUrville                   "},
new TeamCalendarTimeZoneViewModel_value{alias="AU", displayName="Antarctica/Macquarie                        "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ4", displayName="Antarctica/Mawson                           "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ5", displayName="Antarctica/McMurdo                          "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ6", displayName="Antarctica/Palmer                           "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ7", displayName="Antarctica/Rothera                          "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ8", displayName="Antarctica/Syowa                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ9", displayName="Antarctica/Troll                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AQ10", displayName="Antarctica/Vostok                           "},
new TeamCalendarTimeZoneViewModel_value{alias="SJ", displayName="Arctic/Longyearbyen                         "},
new TeamCalendarTimeZoneViewModel_value{alias="YE", displayName="Asia/Aden                                   "},
new TeamCalendarTimeZoneViewModel_value{alias="KZ", displayName="Asia/Almaty                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="JO", displayName="Asia/Amman                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="RU", displayName="Asia/Anadyr                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="KZ2", displayName="Asia/Aqtau                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="KZ3", displayName="Asia/Aqtobe                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="TM", displayName="Asia/Ashgabat                               "},
new TeamCalendarTimeZoneViewModel_value{alias="KZ4", displayName="Asia/Atyrau                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="IQ", displayName="Asia/Baghdad                                "},
new TeamCalendarTimeZoneViewModel_value{alias="BH", displayName="Asia/Bahrain                                "},
new TeamCalendarTimeZoneViewModel_value{alias="AZ", displayName="Asia/Baku                                   "},
new TeamCalendarTimeZoneViewModel_value{alias="TH", displayName="Asia/Bangkok                                "},
new TeamCalendarTimeZoneViewModel_value{alias="RU2", displayName="Asia/Barnaul                                "},
new TeamCalendarTimeZoneViewModel_value{alias="LB", displayName="Asia/Beirut                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="KG", displayName="Asia/Bishkek                                "},
new TeamCalendarTimeZoneViewModel_value{alias="BN", displayName="Asia/Brunei                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="RU3", displayName="Asia/Chita                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="MN", displayName="Asia/Choibalsan                             "},
new TeamCalendarTimeZoneViewModel_value{alias="LK", displayName="Asia/Colombo                                "},
new TeamCalendarTimeZoneViewModel_value{alias="SY", displayName="Asia/Damascus                               "},
new TeamCalendarTimeZoneViewModel_value{alias="BD", displayName="Asia/Dhaka                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="TL", displayName="Asia/Dili                                   "},
new TeamCalendarTimeZoneViewModel_value{alias="AE", displayName="Asia/Dubai                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="TJ", displayName="Asia/Dushanbe                               "},
new TeamCalendarTimeZoneViewModel_value{alias="CY", displayName="Asia/Famagusta                              "},
new TeamCalendarTimeZoneViewModel_value{alias="PS", displayName="Asia/Gaza                                   "},
new TeamCalendarTimeZoneViewModel_value{alias="PS2", displayName="Asia/Hebron                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="VN", displayName="Asia/Ho_Chi_Minh                            "},
new TeamCalendarTimeZoneViewModel_value{alias="HK", displayName="Asia/Hong_Kong                              "},
new TeamCalendarTimeZoneViewModel_value{alias="MN2", displayName="Asia/Hovd                                   "},
new TeamCalendarTimeZoneViewModel_value{alias="RU4", displayName="Asia/Irkutsk                                "},
new TeamCalendarTimeZoneViewModel_value{alias="TR", displayName="Asia/Istanbul                               "},
new TeamCalendarTimeZoneViewModel_value{alias="ID", displayName="Asia/Jakarta                                "},
new TeamCalendarTimeZoneViewModel_value{alias="ID2", displayName="Asia/Jayapura                               "},
new TeamCalendarTimeZoneViewModel_value{alias="IL", displayName="Asia/Jerusalem                              "},
new TeamCalendarTimeZoneViewModel_value{alias="AF", displayName="Asia/Kabul                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="RU5", displayName="Asia/Kamchatka                              "},
new TeamCalendarTimeZoneViewModel_value{alias="PK", displayName="Asia/Karachi                                "},
new TeamCalendarTimeZoneViewModel_value{alias="NP", displayName="Asia/Kathmandu                              "},
new TeamCalendarTimeZoneViewModel_value{alias="RU6", displayName="Asia/Khandyga                               "},
new TeamCalendarTimeZoneViewModel_value{alias="IN", displayName="Asia/Kolkata                                "},
new TeamCalendarTimeZoneViewModel_value{alias="RU7", displayName="Asia/Krasnoyarsk                            "},
new TeamCalendarTimeZoneViewModel_value{alias="MY", displayName="Asia/Kuala_Lumpur                           "},
new TeamCalendarTimeZoneViewModel_value{alias="MY2", displayName="Asia/Kuching                                "},
new TeamCalendarTimeZoneViewModel_value{alias="KW", displayName="Asia/Kuwait                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="MO", displayName="Asia/Macau                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="RU8", displayName="Asia/Magadan                                "},
new TeamCalendarTimeZoneViewModel_value{alias="ID3", displayName="Asia/Makassar                               "},
new TeamCalendarTimeZoneViewModel_value{alias="PH", displayName="Asia/Manila                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="OM", displayName="Asia/Muscat                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="CY2", displayName="Asia/Nicosia                                "},
new TeamCalendarTimeZoneViewModel_value{alias="RU9", displayName="Asia/Novokuznetsk                           "},
new TeamCalendarTimeZoneViewModel_value{alias="RU10", displayName="Asia/Novosibirsk                            "},
new TeamCalendarTimeZoneViewModel_value{alias="RU11", displayName="Asia/Omsk                                   "},
new TeamCalendarTimeZoneViewModel_value{alias="KZ5", displayName="Asia/Oral                                   "},
new TeamCalendarTimeZoneViewModel_value{alias="KH", displayName="Asia/Phnom_Penh                             "},
new TeamCalendarTimeZoneViewModel_value{alias="ID4", displayName="Asia/Pontianak                              "},
new TeamCalendarTimeZoneViewModel_value{alias="KP", displayName="Asia/Pyongyang                              "},
new TeamCalendarTimeZoneViewModel_value{alias="QA", displayName="Asia/Qatar                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="KZ6", displayName="Asia/Qostanay                               "},
new TeamCalendarTimeZoneViewModel_value{alias="KZ7", displayName="Asia/Qyzylorda                              "},
new TeamCalendarTimeZoneViewModel_value{alias="SA", displayName="Asia/Riyadh                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="RU12", displayName="Asia/Sakhalin                               "},
new TeamCalendarTimeZoneViewModel_value{alias="UZ", displayName="Asia/Samarkand                              "},
new TeamCalendarTimeZoneViewModel_value{alias="KR", displayName="Asia/Seoul                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="CN", displayName="Asia/Shanghai                               "},
new TeamCalendarTimeZoneViewModel_value{alias="SG", displayName="Asia/Singapore                              "},
new TeamCalendarTimeZoneViewModel_value{alias="RU13", displayName="Asia/Srednekolymsk                          "},
new TeamCalendarTimeZoneViewModel_value{alias="TW", displayName="Asia/Taipei                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="UZ2", displayName="Asia/Tashkent                               "},
new TeamCalendarTimeZoneViewModel_value{alias="GE", displayName="Asia/Tbilisi                                "},
new TeamCalendarTimeZoneViewModel_value{alias="IR", displayName="Asia/Tehran                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="BT", displayName="Asia/Thimphu                                "},
new TeamCalendarTimeZoneViewModel_value{alias="JP", displayName="Asia/Tokyo                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="RU14", displayName="Asia/Tomsk                                  "},
new TeamCalendarTimeZoneViewModel_value{alias="MN3", displayName="Asia/Ulaanbaatar                            "},
new TeamCalendarTimeZoneViewModel_value{alias="CN2", displayName="Asia/Urumqi                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="RU15", displayName="Asia/Ust-Nera                               "},
new TeamCalendarTimeZoneViewModel_value{alias="LA", displayName="Asia/Vientiane                              "},
new TeamCalendarTimeZoneViewModel_value{alias="RU16", displayName="Asia/Vladivostok                            "},
new TeamCalendarTimeZoneViewModel_value{alias="RU17", displayName="Asia/Yakutsk                                "},
new TeamCalendarTimeZoneViewModel_value{alias="MM", displayName="Asia/Yangon                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="RU18", displayName="Asia/Yekaterinburg                          "},
new TeamCalendarTimeZoneViewModel_value{alias="AM", displayName="Asia/Yerevan                                "},
new TeamCalendarTimeZoneViewModel_value{alias="PT", displayName="Atlantic/Azores                             "},
new TeamCalendarTimeZoneViewModel_value{alias="BM", displayName="Atlantic/Bermuda                            "},
new TeamCalendarTimeZoneViewModel_value{alias="ES2", displayName="Atlantic/Canary                             "},
new TeamCalendarTimeZoneViewModel_value{alias="CV", displayName="Atlantic/Cape_Verde                         "},
new TeamCalendarTimeZoneViewModel_value{alias="FO", displayName="Atlantic/Faroe                              "},
new TeamCalendarTimeZoneViewModel_value{alias="PT2", displayName="Atlantic/Madeira                            "},
new TeamCalendarTimeZoneViewModel_value{alias="IS", displayName="Atlantic/Reykjavik                          "},
new TeamCalendarTimeZoneViewModel_value{alias="GS", displayName="Atlantic/South_Georgia                      "},
new TeamCalendarTimeZoneViewModel_value{alias="SH", displayName="Atlantic/St_Helena                          "},
new TeamCalendarTimeZoneViewModel_value{alias="FK", displayName="Atlantic/Stanley                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AU2", displayName="Australia/Adelaide                          "},
new TeamCalendarTimeZoneViewModel_value{alias="AU3", displayName="Australia/Brisbane                          "},
new TeamCalendarTimeZoneViewModel_value{alias="AU4", displayName="Australia/Broken_Hill                       "},
new TeamCalendarTimeZoneViewModel_value{alias="AU5", displayName="Australia/Darwin                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AU6", displayName="Australia/Eucla                             "},
new TeamCalendarTimeZoneViewModel_value{alias="AU7", displayName="Australia/Hobart                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AU8", displayName="Australia/Lindeman                          "},
new TeamCalendarTimeZoneViewModel_value{alias="AU9", displayName="Australia/Lord_Howe                         "},
new TeamCalendarTimeZoneViewModel_value{alias="AU10", displayName="Australia/Melbourne                         "},
new TeamCalendarTimeZoneViewModel_value{alias="AU11", displayName="Australia/Perth                             "},
new TeamCalendarTimeZoneViewModel_value{alias="AU12", displayName="Australia/Sydney                            "},
new TeamCalendarTimeZoneViewModel_value{alias="NL", displayName="Europe/Amsterdam                            "},
new TeamCalendarTimeZoneViewModel_value{alias="AD", displayName="Europe/Andorra                              "},
new TeamCalendarTimeZoneViewModel_value{alias="RU19", displayName="Europe/Astrakhan                            "},
new TeamCalendarTimeZoneViewModel_value{alias="GR", displayName="Europe/Athens                               "},
new TeamCalendarTimeZoneViewModel_value{alias="RS", displayName="Europe/Belgrade                             "},
new TeamCalendarTimeZoneViewModel_value{alias="DE", displayName="Europe/Berlin                               "},
new TeamCalendarTimeZoneViewModel_value{alias="SK", displayName="Europe/Bratislava                           "},
new TeamCalendarTimeZoneViewModel_value{alias="BE", displayName="Europe/Brussels                             "},
new TeamCalendarTimeZoneViewModel_value{alias="RO", displayName="Europe/Bucharest                            "},
new TeamCalendarTimeZoneViewModel_value{alias="HU", displayName="Europe/Budapest                             "},
new TeamCalendarTimeZoneViewModel_value{alias="DE2", displayName="Europe/Busingen                             "},
new TeamCalendarTimeZoneViewModel_value{alias="MD", displayName="Europe/Chisinau                             "},
new TeamCalendarTimeZoneViewModel_value{alias="DK", displayName="Europe/Copenhagen                           "},
new TeamCalendarTimeZoneViewModel_value{alias="IE", displayName="Europe/Dublin                               "},
new TeamCalendarTimeZoneViewModel_value{alias="GI", displayName="Europe/Gibraltar                            "},
new TeamCalendarTimeZoneViewModel_value{alias="GG", displayName="Europe/Guernsey                             "},
new TeamCalendarTimeZoneViewModel_value{alias="FI", displayName="Europe/Helsinki                             "},
new TeamCalendarTimeZoneViewModel_value{alias="IM", displayName="Europe/Isle_of_Man                          "},
new TeamCalendarTimeZoneViewModel_value{alias="TR2", displayName="Europe/Istanbul                             "},
new TeamCalendarTimeZoneViewModel_value{alias="JE", displayName="Europe/Jersey                               "},
new TeamCalendarTimeZoneViewModel_value{alias="RU20", displayName="Europe/Kaliningrad                          "},
new TeamCalendarTimeZoneViewModel_value{alias="UA", displayName="Europe/Kiev                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="RU21", displayName="Europe/Kirov                                "},
new TeamCalendarTimeZoneViewModel_value{alias="PT3", displayName="Europe/Lisbon                               "},
new TeamCalendarTimeZoneViewModel_value{alias="SI", displayName="Europe/Ljubljana                            "},
new TeamCalendarTimeZoneViewModel_value{alias="GB", displayName="Europe/London                               "},
new TeamCalendarTimeZoneViewModel_value{alias="LU", displayName="Europe/Luxembourg                           "},
new TeamCalendarTimeZoneViewModel_value{alias="ES3", displayName="Europe/Madrid                               "},
new TeamCalendarTimeZoneViewModel_value{alias="MT", displayName="Europe/Malta                                "},
new TeamCalendarTimeZoneViewModel_value{alias="AX", displayName="Europe/Mariehamn                            "},
new TeamCalendarTimeZoneViewModel_value{alias="BY", displayName="Europe/Minsk                                "},
new TeamCalendarTimeZoneViewModel_value{alias="MC", displayName="Europe/Monaco                               "},
new TeamCalendarTimeZoneViewModel_value{alias="RU22", displayName="Europe/Moscow                               "},
new TeamCalendarTimeZoneViewModel_value{alias="CY3", displayName="Europe/Nicosia                              "},
new TeamCalendarTimeZoneViewModel_value{alias="NO", displayName="Europe/Oslo                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="FR", displayName="Europe/Paris                                "},
new TeamCalendarTimeZoneViewModel_value{alias="ME", displayName="Europe/Podgorica                            "},
new TeamCalendarTimeZoneViewModel_value{alias="CZ", displayName="Europe/Prague                               "},
new TeamCalendarTimeZoneViewModel_value{alias="LV", displayName="Europe/Riga                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="IT", displayName="Europe/Rome                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="RU23", displayName="Europe/Samara                               "},
new TeamCalendarTimeZoneViewModel_value{alias="SM", displayName="Europe/San_Marino                           "},
new TeamCalendarTimeZoneViewModel_value{alias="BA", displayName="Europe/Sarajevo                             "},
new TeamCalendarTimeZoneViewModel_value{alias="RU24", displayName="Europe/Saratov                              "},
new TeamCalendarTimeZoneViewModel_value{alias="UA2", displayName="Europe/Simferopol                           "},
new TeamCalendarTimeZoneViewModel_value{alias="MK", displayName="Europe/Skopje                               "},
new TeamCalendarTimeZoneViewModel_value{alias="BG", displayName="Europe/Sofia                                "},
new TeamCalendarTimeZoneViewModel_value{alias="SE", displayName="Europe/Stockholm                            "},
new TeamCalendarTimeZoneViewModel_value{alias="EE", displayName="Europe/Tallinn                              "},
new TeamCalendarTimeZoneViewModel_value{alias="AL", displayName="Europe/Tirane                               "},
new TeamCalendarTimeZoneViewModel_value{alias="RU25", displayName="Europe/Ulyanovsk                            "},
new TeamCalendarTimeZoneViewModel_value{alias="UA3", displayName="Europe/Uzhgorod                             "},
new TeamCalendarTimeZoneViewModel_value{alias="LI", displayName="Europe/Vaduz                                "},
new TeamCalendarTimeZoneViewModel_value{alias="VA", displayName="Europe/Vatican                              "},
new TeamCalendarTimeZoneViewModel_value{alias="AT", displayName="Europe/Vienna                               "},
new TeamCalendarTimeZoneViewModel_value{alias="LT", displayName="Europe/Vilnius                              "},
new TeamCalendarTimeZoneViewModel_value{alias="RU26", displayName="Europe/Volgograd                            "},
new TeamCalendarTimeZoneViewModel_value{alias="PL", displayName="Europe/Warsaw                               "},
new TeamCalendarTimeZoneViewModel_value{alias="HR", displayName="Europe/Zagreb                               "},
new TeamCalendarTimeZoneViewModel_value{alias="UA4", displayName="Europe/Zaporozhye                           "},
new TeamCalendarTimeZoneViewModel_value{alias="CH", displayName="Europe/Zurich                               "},
new TeamCalendarTimeZoneViewModel_value{alias="MG", displayName="Indian/Antananarivo                         "},
new TeamCalendarTimeZoneViewModel_value{alias="IO", displayName="Indian/Chagos                               "},
new TeamCalendarTimeZoneViewModel_value{alias="CX", displayName="Indian/Christmas                            "},
new TeamCalendarTimeZoneViewModel_value{alias="CC", displayName="Indian/Cocos                                "},
new TeamCalendarTimeZoneViewModel_value{alias="KM", displayName="Indian/Comoro                               "},
new TeamCalendarTimeZoneViewModel_value{alias="TF", displayName="Indian/Kerguelen                            "},
new TeamCalendarTimeZoneViewModel_value{alias="SC", displayName="Indian/Mahe                                 "},
new TeamCalendarTimeZoneViewModel_value{alias="MV", displayName="Indian/Maldives                             "},
new TeamCalendarTimeZoneViewModel_value{alias="MU", displayName="Indian/Mauritius                            "},
new TeamCalendarTimeZoneViewModel_value{alias="YT", displayName="Indian/Mayotte                              "},
new TeamCalendarTimeZoneViewModel_value{alias="RE", displayName="Indian/Reunion                              "},
new TeamCalendarTimeZoneViewModel_value{alias="WS", displayName="Pacific/Apia                                "},
new TeamCalendarTimeZoneViewModel_value{alias="NZ", displayName="Pacific/Auckland                            "},
new TeamCalendarTimeZoneViewModel_value{alias="PG", displayName="Pacific/Bougainville                        "},
new TeamCalendarTimeZoneViewModel_value{alias="NZ2", displayName="Pacific/Chatham                             "},
new TeamCalendarTimeZoneViewModel_value{alias="FM", displayName="Pacific/Chuuk                               "},
new TeamCalendarTimeZoneViewModel_value{alias="CL3", displayName="Pacific/Easter                              "},
new TeamCalendarTimeZoneViewModel_value{alias="VU", displayName="Pacific/Efate                               "},
new TeamCalendarTimeZoneViewModel_value{alias="KI", displayName="Pacific/Enderbury                           "},
new TeamCalendarTimeZoneViewModel_value{alias="TK", displayName="Pacific/Fakaofo                             "},
new TeamCalendarTimeZoneViewModel_value{alias="FJ", displayName="Pacific/Fiji                                "},
new TeamCalendarTimeZoneViewModel_value{alias="TV", displayName="Pacific/Funafuti                            "},
new TeamCalendarTimeZoneViewModel_value{alias="EC2", displayName="Pacific/Galapagos                           "},
new TeamCalendarTimeZoneViewModel_value{alias="PF", displayName="Pacific/Gambier                             "},
new TeamCalendarTimeZoneViewModel_value{alias="SB", displayName="Pacific/Guadalcanal                         "},
new TeamCalendarTimeZoneViewModel_value{alias="GU", displayName="Pacific/Guam                                "},
new TeamCalendarTimeZoneViewModel_value{alias="US29", displayName="Pacific/Honolulu                            "},
new TeamCalendarTimeZoneViewModel_value{alias="KI2", displayName="Pacific/Kiritimati                          "},
new TeamCalendarTimeZoneViewModel_value{alias="FM2", displayName="Pacific/Kosrae                              "},
new TeamCalendarTimeZoneViewModel_value{alias="MH", displayName="Pacific/Kwajalein                           "},
new TeamCalendarTimeZoneViewModel_value{alias="MH2", displayName="Pacific/Majuro                              "},
new TeamCalendarTimeZoneViewModel_value{alias="PF2", displayName="Pacific/Marquesas                           "},
new TeamCalendarTimeZoneViewModel_value{alias="UM", displayName="Pacific/Midway                              "},
new TeamCalendarTimeZoneViewModel_value{alias="NR", displayName="Pacific/Nauru                               "},
new TeamCalendarTimeZoneViewModel_value{alias="NU", displayName="Pacific/Niue                                "},
new TeamCalendarTimeZoneViewModel_value{alias="NF", displayName="Pacific/Norfolk                             "},
new TeamCalendarTimeZoneViewModel_value{alias="NC", displayName="Pacific/Noumea                              "},
new TeamCalendarTimeZoneViewModel_value{alias="AS", displayName="Pacific/Pago_Pago                           "},
new TeamCalendarTimeZoneViewModel_value{alias="PW", displayName="Pacific/Palau                               "},
new TeamCalendarTimeZoneViewModel_value{alias="PN", displayName="Pacific/Pitcairn                            "},
new TeamCalendarTimeZoneViewModel_value{alias="FM3", displayName="Pacific/Pohnpei                             "},
new TeamCalendarTimeZoneViewModel_value{alias="PG2", displayName="Pacific/Port_Moresby                        "},
new TeamCalendarTimeZoneViewModel_value{alias="CK", displayName="Pacific/Rarotonga                           "},
new TeamCalendarTimeZoneViewModel_value{alias="MP", displayName="Pacific/Saipan                              "},
new TeamCalendarTimeZoneViewModel_value{alias="PF3", displayName="Pacific/Tahiti                              "},
new TeamCalendarTimeZoneViewModel_value{alias="KI3", displayName="Pacific/Tarawa                              "},
new TeamCalendarTimeZoneViewModel_value{alias="TO", displayName="Pacific/Tongatapu                           "},
new TeamCalendarTimeZoneViewModel_value{alias="UM2", displayName="Pacific/Wake                                "},
new TeamCalendarTimeZoneViewModel_value{alias="WF", displayName="Pacific/Wallis                              "}
            };

            zns.ForEach(da =>
            {
                if (googleTimeZones.ContainsKey(da.alias))
                {
                    da.displayName += " (UTC" + googleTimeZones[da.alias] + ")";
                }
            });
            return zns;
        }
        /// <summary>
        /// Country code	UTC offset ±hh:mm
        /// </summary>
        private static Dictionary<string, string> googleTimeZones = new Dictionary<string, string>
        {
{"CI","+00:00"},
{"GH","+00:00"},
{"ET","+03:00"},
{"DZ","+01:00"},
{"ER","+03:00"},
{"ML","+00:00"},
{"CF","+01:00"},
{"GM","+00:00"},
{"GW","+00:00"},
{"MW","+02:00"},
{"CG","+01:00"},
{"BI","+02:00"},
{"EG","+02:00"},
{"MA","+01:00"},
{"ES","+01:00"},
{"GN","+00:00"},
{"SN","+00:00"},
{"TZ","+03:00"},
{"DJ","+03:00"},
{"CM","+01:00"},
{"EH","+01:00"},
{"SL","+00:00"},
{"BW","+02:00"},
{"ZW","+02:00"},
{"ZA","+02:00"},
{"SS","+02:00"},
{"UG","+03:00"},
{"SD","+02:00"},
{"RW","+02:00"},
{"CD","+01:00"},
{"NG","+01:00"},
{"GA","+01:00"},
{"TG","+00:00"},
{"AO","+01:00"},
{"CD2","+02:00"},
{"ZM","+02:00"},
{"GQ","+01:00"},
{"MZ","+02:00"},
{"LS","+02:00"},
{"SZ","+02:00"},
{"SO","+03:00"},
{"LR","+00:00"},
{"KE","+03:00"},
{"TD","+01:00"},
{"NE","+01:00"},
{"MR","+00:00"},
{"BF","+00:00"},
{"BJ","+01:00"},
{"ST","+00:00"},
{"LY","+02:00"},
{"TN","+01:00"},
{"NA","+02:00"},
{"US","−10:00"},
{"US2","−09:00"},
{"AI","−04:00"},
{"AG","−04:00"},
{"BR","−03:00"},
{"AR","−03:00"},
{"AR2","−03:00"},
{"AR3","−03:00"},
{"AR4","−03:00"},
{"AR5","−03:00"},
{"AR6","−03:00"},
{"AR7","−03:00"},
{"AR8","−03:00"},
{"AR9","−03:00"},
{"AR10","−03:00"},
{"AR11","−03:00"},
{"AR12","−03:00"},
{"AW","−04:00"},
{"PY","−04:00"},
{"CA","−05:00"},
{"BR2","−03:00"},
{"MX","−06:00"},
{"BB","−04:00"},
{"BR3","−03:00"},
{"BZ","−06:00"},
{"CA2","−04:00"},
{"BR4","−04:00"},
{"CO","−05:00"},
{"US3","−07:00"},
{"CA3","−07:00"},
{"BR5","−04:00"},
{"MX2","−05:00"},
{"VE","−04:00"},
{"GF","−03:00"},
{"KY","−05:00"},
{"US4","−06:00"},
{"MX3","−07:00"},
{"CR","−06:00"},
{"CA4","−07:00"},
{"BR6","−04:00"},
{"CW","−04:00"},
{"GL","+00:00"},
{"CA5","−07:00"},
{"CA6","−07:00"},
{"US5","−07:00"},
{"US6","−05:00"},
{"DM","−04:00"},
{"CA7","−07:00"},
{"BR7","−05:00"},
{"SV","−06:00"},
{"CA8","−07:00"},
{"BR8","−03:00"},
{"CA9","−04:00"},
{"CA10","−04:00"},
{"TC","−05:00"},
{"GD","−04:00"},
{"GP","−04:00"},
{"GT","−06:00"},
{"EC","−05:00"},
{"GY","−04:00"},
{"CA11","−04:00"},
{"CU","−05:00"},
{"MX4","−07:00"},
{"US7","−05:00"},
{"US8","−06:00"},
{"US9","−05:00"},
{"US10","−05:00"},
{"US11","−06:00"},
{"US12","−05:00"},
{"US13","−05:00"},
{"US14","−05:00"},
{"CA12","−07:00"},
{"CA13","−05:00"},
{"JM","−05:00"},
{"US15","−09:00"},
{"US16","−05:00"},
{"US17","−05:00"},
{"BQ","−04:00"},
{"BO","−04:00"},
{"PE","−05:00"},
{"US18","−08:00"},
{"SX","−04:00"},
{"BR9","−03:00"},
{"NI","−06:00"},
{"BR10","−04:00"},
{"MF","−04:00"},
{"MQ","−04:00"},
{"MX5","−06:00"},
{"MX6","−07:00"},
{"US19","−06:00"},
{"MX7","−06:00"},
{"US20","−09:00"},
{"MX8","−06:00"},
{"PM","−03:00"},
{"CA14","−04:00"},
{"MX9","−06:00"},
{"UY","−03:00"},
{"MS","−04:00"},
{"BS","−05:00"},
{"US21","−05:00"},
{"CA15","−05:00"},
{"US22","−09:00"},
{"BR11","−02:00"},
{"US23","−06:00"},
{"US24","−06:00"},
{"US25","−06:00"},
{"GL2","−03:00"},
{"MX10","−07:00"},
{"PA","−05:00"},
{"CA16","−05:00"},
{"SR","−03:00"},
{"US26","−07:00"},
{"TT","−04:00"},
{"HT","−05:00"},
{"BR12","−04:00"},
{"PR","−04:00"},
{"CL","−03:00"},
{"CA17","−06:00"},
{"CA18","−06:00"},
{"BR13","−03:00"},
{"CA19","−06:00"},
{"CA20","−06:00"},
{"BR14","−05:00"},
{"BR15","−03:00"},
{"CL2","−04:00"},
{"DO","−04:00"},
{"BR16","−03:00"},
{"GL3","−01:00"},
{"US27","−09:00"},
{"BL","−04:00"},
{"CA21","−03:30"},
{"KN","−04:00"},
{"LC","−04:00"},
{"VI","−04:00"},
{"VC","−04:00"},
{"CA22","−06:00"},
{"HN","−06:00"},
{"GL4","−04:00"},
{"CA23","−05:00"},
{"MX11","−08:00"},
{"CA24","−05:00"},
{"VG","−04:00"},
{"CA25","−08:00"},
{"CA26","−07:00"},
{"CA27","−06:00"},
{"US28","−09:00"},
{"CA28","−07:00"},
{"AQ","+11:00"},
{"AQ2","+07:00"},
{"AQ3","+10:00"},
{"AU","+10:00"},
{"AQ4","+05:00"},
{"AQ5","+12:00"},
{"AQ6","−03:00"},
{"AQ7","−03:00"},
{"AQ8","+03:00"},
{"AQ9","+00:00"},
{"AQ10","+06:00"},
{"SJ","+01:00"},
{"YE","+03:00"},
{"KZ","+06:00"},
{"JO","+02:00"},
{"RU","+12:00"},
{"KZ2","+05:00"},
{"KZ3","+05:00"},
{"TM","+05:00"},
{"KZ4","+05:00"},
{"IQ","+03:00"},
{"BH","+03:00"},
{"AZ","+04:00"},
{"TH","+07:00"},
{"RU2","+07:00"},
{"LB","+02:00"},
{"KG","+06:00"},
{"BN","+08:00"},
{"RU3","+09:00"},
{"MN","+08:00"},
{"LK","+05:30"},
{"SY","+02:00"},
{"BD","+06:00"},
{"TL","+09:00"},
{"AE","+04:00"},
{"TJ","+05:00"},
{"CY","+02:00"},
{"PS","+02:00"},
{"PS2","+02:00"},
{"VN","+07:00"},
{"HK","+08:00"},
{"MN2","+07:00"},
{"RU4","+08:00"},
{"TR","+03:00"},
{"ID","+07:00"},
{"ID2","+09:00"},
{"IL","+02:00"},
{"AF","+04:30"},
{"RU5","+12:00"},
{"PK","+05:00"},
{"NP","+05:45"},
{"RU6","+09:00"},
{"IN","+05:30"},
{"RU7","+07:00"},
{"MY","+08:00"},
{"MY2","+08:00"},
{"KW","+03:00"},
{"MO","+08:00"},
{"RU8","+11:00"},
{"ID3","+08:00"},
{"PH","+08:00"},
{"OM","+04:00"},
{"CY2","+02:00"},
{"RU9","+07:00"},
{"RU10","+07:00"},
{"RU11","+06:00"},
{"KZ5","+05:00"},
{"KH","+07:00"},
{"ID4","+07:00"},
{"KP","+09:00"},
{"QA","+03:00"},
{"KZ6","+06:00"},
{"KZ7","+05:00"},
{"SA","+03:00"},
{"RU12","+11:00"},
{"UZ","+05:00"},
{"KR","+09:00"},
{"CN","+08:00"},
{"SG","+08:00"},
{"RU13","+11:00"},
{"TW","+08:00"},
{"UZ2","+05:00"},
{"GE","+04:00"},
{"IR","+03:30"},
{"BT","+06:00"},
{"JP","+09:00"},
{"RU14","+07:00"},
{"MN3","+08:00"},
{"CN2","+06:00"},
{"RU15","+10:00"},
{"LA","+07:00"},
{"RU16","+10:00"},
{"RU17","+09:00"},
{"MM","+06:30"},
{"RU18","+05:00"},
{"AM","+04:00"},
{"PT","−01:00"},
{"BM","−04:00"},
{"ES2","+00:00"},
{"CV","−01:00"},
{"FO","+00:00"},
{"PT2","+00:00"},
{"IS","+00:00"},
{"GS","−02:00"},
{"SH","+00:00"},
{"FK","−03:00"},
{"AU2","+09:30"},
{"AU3","+10:00"},
{"AU4","+09:30"},
{"AU5","+09:30"},
{"AU6","+08:45"},
{"AU7","+10:00"},
{"AU8","+10:00"},
{"AU9","+10:30"},
{"AU10","+10:00"},
{"AU11","+08:00"},
{"AU12","+10:00"},
{"NL","+01:00"},
{"AD","+01:00"},
{"RU19","+04:00"},
{"GR","+02:00"},
{"RS","+01:00"},
{"DE","+01:00"},
{"SK","+01:00"},
{"BE","+01:00"},
{"RO","+02:00"},
{"HU","+01:00"},
{"DE2","+01:00"},
{"MD","+02:00"},
{"DK","+01:00"},
{"IE","+01:00"},
{"GI","+01:00"},
{"GG","+00:00"},
{"FI","+02:00"},
{"IM","+00:00"},
{"TR2","+03:00"},
{"JE","+00:00"},
{"RU20","+02:00"},
{"UA","+02:00"},
{"RU21","+03:00"},
{"PT3","+00:00"},
{"SI","+01:00"},
{"GB","+00:00"},
{"LU","+01:00"},
{"ES3","+01:00"},
{"MT","+01:00"},
{"AX","+02:00"},
{"BY","+03:00"},
{"MC","+01:00"},
{"RU22","+03:00"},
{"CY3","+02:00"},
{"NO","+01:00"},
{"FR","+01:00"},
{"ME","+01:00"},
{"CZ","+01:00"},
{"LV","+02:00"},
{"IT","+01:00"},
{"RU23","+04:00"},
{"SM","+01:00"},
{"BA","+01:00"},
{"RU24","+04:00"},
{"UA2","+03:00"},
{"MK","+01:00"},
{"BG","+02:00"},
{"SE","+01:00"},
{"EE","+02:00"},
{"AL","+01:00"},
{"RU25","+04:00"},
{"UA3","+02:00"},
{"LI","+01:00"},
{"VA","+01:00"},
{"AT","+01:00"},
{"LT","+02:00"},
{"RU26","+03:00"},
{"PL","+01:00"},
{"HR","+01:00"},
{"UA4","+02:00"},
{"CH","+01:00"},
{"MG","+03:00"},
{"IO","+06:00"},
{"CX","+07:00"},
{"CC","+06:30"},
{"KM","+03:00"},
{"TF","+05:00"},
{"SC","+04:00"},
{"MV","+05:00"},
{"MU","+04:00"},
{"YT","+03:00"},
{"RE","+04:00"},
{"WS","+13:00"},
{"NZ","+12:00"},
{"PG","+11:00"},
{"NZ2","+12:45"},
{"FM","+10:00"},
{"CL3","−06:00"},
{"VU","+11:00"},
{"KI","+13:00"},
{"TK","+13:00"},
{"FJ","+12:00"},
{"TV","+12:00"},
{"EC2","−06:00"},
{"PF","−09:00"},
{"SB","+11:00"},
{"GU","+10:00"},
{"US29","−10:00"},
{"KI2","+14:00"},
{"FM2","+11:00"},
{"MH","+12:00"},
{"MH2","+12:00"},
{"PF2","−09:30"},
{"UM","−11:00"},
{"NR","+12:00"},
{"NU","−11:00"},
{"NF","+11:00"},
{"NC","+11:00"},
{"AS","−11:00"},
{"PW","+09:00"},
{"PN","−08:00"},
{"FM3","+11:00"},
{"PG2","+10:00"},
{"CK","−10:00"},
{"MP","+10:00"},
{"PF3","−10:00"},
{"KI3","+12:00"},
{"TO","+13:00"},
{"UM2","+12:00"},
{"WF","+12:00"}
        };


        private DateTime ConvertToTimezone(string timeZone, DateTime dt)
        {
            if (googleTimeZones.ContainsKey(timeZone))
            {
                var _dt = dt;
                var tmZn = googleTimeZones[timeZone];
                if (tmZn.StartsWith("+"))
                {
                    var zn = tmZn.Substring(1).Split(':');
                    _dt = _dt.AddHours(Convert.ToInt32(zn[0]) * -1);
                    _dt = _dt.AddMinutes(Convert.ToInt32(zn[1]) * -1);
                }
                else
                {
                    var zn = tmZn.Substring(1).Split(':');
                    _dt = _dt.AddHours(Convert.ToInt32(zn[0]));
                    _dt = _dt.AddMinutes(Convert.ToInt32(zn[1]));
                }
                return _dt;
            }
            else return dt;
        }
    }


    public class GoogleCredentialTokenViewModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }
    public class GoogleCredentialTokenViewModel_WithRefresh : GoogleCredentialTokenViewModel
    {
        //public int expires_in { get; set; }
        public string refresh_token { get; set; }
        //public string scope { get; set; }
        //public DateTime Issued { get; set; }
        //public DateTime IssuedUtc { get; set; }
    }



    public class GoogleOAuth_WebSite_CredentialViewModel
    {
        public GoogleOAuthCredentialViewModel_Installed2 web { get; set; }

    }
    public class GoogleOAuthCredentialViewModel_Installed2
    {
        public string client_id { get; set; }
        public string project_id { get; set; }
        public string auth_uri { get; set; }
        public string token_uri { get; set; }
        public string auth_provider_x509_cert_url { get; set; }
        public string client_secret { get; set; }
        public IList<string> redirect_uris { get; set; }
        public string ReqAccessCode { get; set; }
    }
}
