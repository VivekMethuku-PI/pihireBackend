using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PiHire.BAL.Common;
using PiHire.BAL.Common.Extensions;
using PiHire.BAL.Common.Http;
using PiHire.BAL.Common.Meeting;
using PiHire.BAL.Common.Types;
using PiHire.BAL.ViewModels;
using PiHire.DAL;
using PiHire.DAL.Entities;
using PiHire.DAL.Models;
using PiHire.Utilities.Communications.Emails;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{

    public class BaseRepository : IRepositories.IBaseRepository
    {
        public static DateTime CurrentTime { get { return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time")); } }
        public static DateTime CurrentDate { get { return CurrentTime.Date; } }
        internal PiHIRE2Context dbContext;
        internal AppSettings appSettings;
        public UserAuthorizationViewModel Usr { get; set; }

        public BaseRepository(PiHIRE2Context dbContext)
        {
            this.dbContext = dbContext;
        }
        public BaseRepository(PiHIRE2Context dbContext, AppSettings appSettings)
        {
            this.dbContext = dbContext;
            this.appSettings = appSettings;
        }

        #region Database

        /// <summary>
        /// Used to create entity db content
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static PiHIRE2Context getDatabase(string connectionString)
        {
            return new PiHIRE2Context(connectionString);
        }

        #endregion

        #region Status Codes

        public static Dictionary<ApipResponseHttpCodes, string> HttpMessages = new Dictionary<ApipResponseHttpCodes, string>
        {
            {ApipResponseHttpCodes.OK,"Request is successful" },
            {ApipResponseHttpCodes.Accepted,"Request accepted but it is queued or processing" },
            {ApipResponseHttpCodes.BadRequest,"Request has missing required parameters or validation errors" },
            {ApipResponseHttpCodes.Unauthorized,"Bad Token or missing token" },
            {ApipResponseHttpCodes.Forbidden,"Access denied for the requested resource" },
            {ApipResponseHttpCodes.NotFound,"The requested resource does not exist" },
            {ApipResponseHttpCodes.Conflict,"Request conflicts with another, trying to create already existing resource" },
            {ApipResponseHttpCodes.TooManyRequests,"Api request limit exceeded" },
            {ApipResponseHttpCodes.InternalServerError,"There was an error processing your request" }
        };

        internal string getEmployeePhotoUrl(string userPhoto, int userId)
        {
            return string.IsNullOrEmpty(userPhoto) != true ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + userId + "/ProfilePhoto/" + userPhoto : string.Empty;
        }
        internal string getCandidatePhotoUrl(string userPhoto, int userId)
        {
            return string.IsNullOrEmpty(userPhoto) != true ? userPhoto : string.Empty;
        }

        internal string getCandidateFileUrl(string fileName, int candProfId)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            fileName = fileName.Replace("#", "%23");
            return appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + candProfId + "/" + fileName;
        }

        public static Dictionary<ApiResponseErrorCodes, string> ErrorMessages = new Dictionary<ApiResponseErrorCodes, string>
        {
            {ApiResponseErrorCodes.InvalidJsonFormat,"The request body provided was not a proper JSON string" },
            {ApiResponseErrorCodes.InvalidBodyContent,"Invalid schema in the body provided." },
            {ApiResponseErrorCodes.InvalidUrlParameter,"Invalid URL parameters" },
            {ApiResponseErrorCodes.TokenInvalid,"Incorrect API Token" },
            {ApiResponseErrorCodes.TokenExpired,"API Token expired" },
            {ApiResponseErrorCodes.UserPermissionNotGranted,"Permission has not been granted by the user to make this request." },
            {ApiResponseErrorCodes.UserPermissionNoResrcAccess,"The user does not have permission to access the resource" },
            {ApiResponseErrorCodes.UserPermissionResrcAccessDisabled,"API access has been disabled for the user by the owner." },
            {ApiResponseErrorCodes.ResourceDoesNotExist ,"The resource that you're trying to access doesn't exist" },
            {ApiResponseErrorCodes.ResourceAlreadyExist ,"The resource trying to create already exist" },
            {ApiResponseErrorCodes.Exception,"There was an error processing your request" }
        };

        public static Dictionary<ApiResponseErrorCodes, ApipResponseHttpCodes> GetHttpCode = new Dictionary<ApiResponseErrorCodes, ApipResponseHttpCodes>
        {
            {ApiResponseErrorCodes.InvalidJsonFormat,ApipResponseHttpCodes.BadRequest },
            {ApiResponseErrorCodes.InvalidBodyContent,ApipResponseHttpCodes.BadRequest },
            {ApiResponseErrorCodes.InvalidUrlParameter,ApipResponseHttpCodes.BadRequest },
            {ApiResponseErrorCodes.TokenInvalid,ApipResponseHttpCodes.Unauthorized },
            {ApiResponseErrorCodes.TokenExpired,ApipResponseHttpCodes.Unauthorized },
            {ApiResponseErrorCodes.UserPermissionNotGranted,ApipResponseHttpCodes.Forbidden },
            {ApiResponseErrorCodes.UserPermissionNoResrcAccess,ApipResponseHttpCodes.Forbidden },
            {ApiResponseErrorCodes.UserPermissionResrcAccessDisabled,ApipResponseHttpCodes.Forbidden },
            {ApiResponseErrorCodes.ResourceDoesNotExist ,ApipResponseHttpCodes.NotFound },
            {ApiResponseErrorCodes.ResourceAlreadyExist ,ApipResponseHttpCodes.Conflict },
            {ApiResponseErrorCodes.Exception,ApipResponseHttpCodes.InternalServerError },
            {ApiResponseErrorCodes.ApiLimtExhausted,ApipResponseHttpCodes.TooManyRequests },
            {ApiResponseErrorCodes.LimtExhausted,ApipResponseHttpCodes.BadRequest }
        };

        #endregion

        #region Conversions
        public static string GetDaySuffix(int id)
        {
            var aa = (id % 10 == 1 && id != 11) ? id + "st"
                      : (id % 10 == 2 && id != 12) ? id + "nd"
                      : (id % 10 == 3 && id != 13) ? id + "rd"
                      : id + "th";
            return aa;
        }
        public static string GetMonthName(int id)
        {
            string monthName = "";
            switch ((int)id)
            {
                case 1:
                    monthName = "January";
                    break;
                case 2:
                    monthName = "February";
                    break;
                case 3:
                    monthName = "March";
                    break;
                case 4:
                    monthName = "April";
                    break;
                case 5:
                    monthName = "May";
                    break;
                case 6:
                    monthName = "June";
                    break;
                case 7:
                    monthName = "July";
                    break;
                case 8:
                    monthName = "August";
                    break;
                case 9:
                    monthName = "September";
                    break;
                case 10:
                    monthName = "October";
                    break;
                case 11:
                    monthName = "November";
                    break;
                case 12:
                    monthName = "December";
                    break;
            }
            return monthName;
        }
        public static string NumbersToWords(int inputNumber)
        {
            int inputNo = inputNumber;

            if (inputNo == 0)
                return "Zero";

            int[] numbers = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (inputNo < 0)
            {
                sb.Append("Minus ");
                inputNo = -inputNo;
            }

            string[] words0 = {"" ,"One ", "Two ", "Three ", "Four ",
            "Five " ,"Six ", "Seven ", "Eight ", "Nine "};
            string[] words1 = {"Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ",
            "Fifteen ","Sixteen ","Seventeen ","Eighteen ", "Nineteen "};
            string[] words2 = {"Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ",
            "Seventy ","Eighty ", "Ninety "};
            string[] words3 = { "Thousand ", "Lakh ", "Crore " };

            numbers[0] = inputNo % 1000; // units
            numbers[1] = inputNo / 1000;
            numbers[2] = inputNo / 100000;
            numbers[1] = numbers[1] - 100 * numbers[2]; // thousands
            numbers[3] = inputNo / 10000000; // crores
            numbers[2] = numbers[2] - 100 * numbers[3]; // lakhs

            for (int i = 3; i > 0; i--)
            {
                if (numbers[i] != 0)
                {
                    first = i;
                    break;
                }
            }
            for (int i = first; i >= 0; i--)
            {
                if (numbers[i] == 0) continue;
                u = numbers[i] % 10; // ones
                t = numbers[i] / 10;
                h = numbers[i] / 100; // hundreds
                t = t - 10 * h; // tens
                if (h > 0) sb.Append(words0[h] + "Hundred ");
                if (u > 0 || t > 0)
                {
                    if (h > 0 || i == 0) sb.Append("and ");
                    if (t == 0)
                        sb.Append(words0[u]);
                    else if (t == 1)
                        sb.Append(words1[u]);
                    else
                        sb.Append(words2[t - 2] + words0[u]);
                }
                if (i != 0) sb.Append(words3[i - 1]);
            }
            return sb.ToString().TrimEnd();
        }
        public int? ConvertMonths(int? Years)
        {
            if (Years.HasValue)
            {
                int months = (int)Years * 12;
                return months;
            }
            return null;
        }
        public int ConvertMonths(string Years)
        {
            int months = 0;
            if (!string.IsNullOrEmpty(Years))
            {
                string[] exp = Years.Split('.');
                if (exp.Length > 0)
                {
                    int fst = Convert.ToInt32(exp[0]);
                    months = fst * 12;
                    if (exp.Length == 2)
                    {
                        int scd = Convert.ToInt32(exp[1]);
                        months += scd;
                    }
                }
            }
            return months;
        }
        public int ConvertYears(int? Months)
        {
            int months = (int)Months / 12;
            return months;
        }
        public int? ConvertYearsNullable(int? Months)
        {
            if (Months.HasValue)
            {
                int months = (int)Months / 12;
                return months;
            }
            return null;
        }
        public string GetTimeDiff(DateTime dateTime)
        {
            TimeSpan diff = CurrentTime - dateTime;
            string formatted;
            if (diff.Days > 0)
            {
                if (diff.Days > 1)
                {
                    formatted = string.Format(CultureInfo.CurrentCulture, "{0} days ago", diff.Days);
                }
                else
                {
                    formatted = string.Format(CultureInfo.CurrentCulture, "{0} day ago", diff.Days);
                }
            }
            else if (diff.Hours > 0)
            {
                if (diff.Hours > 1)
                {
                    formatted = string.Format(CultureInfo.CurrentCulture, "{0} hours ago", diff.Hours);
                }
                else
                {
                    formatted = string.Format(CultureInfo.CurrentCulture, "{0} hour ago", diff.Hours);
                }
            }
            else
            {
                if (diff.Minutes > 1)
                {
                    formatted = string.Format(CultureInfo.CurrentCulture, "{0} minutes ago", diff.Minutes);
                }
                else
                {
                    formatted = string.Format(CultureInfo.CurrentCulture, "{0} minute ago", diff.Minutes);
                }
            }
            return formatted;
        }

        public async Task<List<TasksViewModel>> TasksList()
        {
            _ = new List<TasksViewModel>();
            List<TasksViewModel> data = await dbContext.GetTasks(this.appSettings.AppSettingsProperties.AppId, null);
            return data;
        }
        public string EnumKeyName(byte? Id, string type)
        {
            string Name = string.Empty;
            switch (type)
            {
                case "SentBy":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(SentBy), Id);
                    }
                    break;
                case "ClreviewStatus":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(ClreviewStatus), Id);
                    }
                    break;
                case "InterviewStatusName":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(InterviewStatus), Id);
                    }
                    break;
                case "ModeofInterviewName":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(ModeOfInterview), Id);
                    }
                    break;
                case "MessageType":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(MessageType), Id);
                    }
                    break;
                case "ProfileType":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(ProfileType), Id);
                    }
                    break;
                case "ActionMode":
                    if (Id != null)
                    {
                        var ActionMode = new Dictionary<byte, string> {
            { (byte)WorkflowActionMode.Candidate, "On Candidate Status"},
            { (byte)WorkflowActionMode.Opening, "On Opening Status"},
            { (byte)WorkflowActionMode.Other, "Other"}
                    };
                        Name = ActionMode[Id.Value].ToString();
                    }
                    break;
                case "ActionType":
                    if (Id != null)
                    {
                        var ActionType = new Dictionary<byte, string> {
            { (byte)WorkflowActionTypes.ChangeStatus, "Change Status"},
            { (byte)WorkflowActionTypes.EmailNotification, "Email Notification"},
            { (byte)WorkflowActionTypes.SendAssessment, "Send Assessment"},
            { (byte)WorkflowActionTypes.SMSNotification, "SMS Notification"},
            { (byte)WorkflowActionTypes.SystemAlert, "System Alert"},
            { (byte)WorkflowActionTypes.RequestDocuments, "Request Documents"},
        };
                        Name = ActionType[Id.Value].ToString();
                    }
                    break;
                case "SendTo":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(UserType), Id);
                    }
                    break;
                case "SendMode":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(SendMode), Id);
                    }
                    break;
                case "FileGroup":
                    if (Id != null)
                    {
                        Name = Enum.GetName(typeof(FileGroup), Id);
                    }
                    break;
                case "DocStatus":
                    if (Id != null)
                    {
                        var docStatus = new Dictionary<byte, string> {
            { (byte)DocStatus.Accepted, "Accepted"},
            { (byte)DocStatus.Notreviewd, "Not Reviewd"},
            { (byte)DocStatus.Rejected, "Rejected"},
            { (byte)DocStatus.Requested, "Requested"}
        };
                        Name = docStatus[Id.Value].ToString();
                    }
                    break;
                case "ActivityType":
                    if (Id != null)
                    {

                        var ActivityType = new Dictionary<byte, string> {
            { (byte)LogActivityType.AssessementUpdates, "Assessement Updates"},
            { (byte)LogActivityType.Critical, "Critical"},
            { (byte)LogActivityType.Other, "Other"},
            { (byte)LogActivityType.RecordUpdates, "Record Updates"},
            { (byte)LogActivityType.ScheduleInterviewUpdates, "Schedule Interview"},
            { (byte)LogActivityType.StatusUpdates, "Status Updates"},
            { (byte)LogActivityType.JobEditUpdates, "Job Edit Updates"},
        };
                        Name = ActivityType[Id.Value].ToString();
                    }
                    break;
                case "ResponseStatus":
                    if (Id != null)
                    {
                        var responseStatus = new Dictionary<byte, string> {
            { (byte)ResponseStatus.AssessmentStared, "Assessment Stared"},
            { (byte)ResponseStatus.AssessmentTaken, "Assessment Taken"},
            { (byte)ResponseStatus.Nottakenyet, "Not taken yet"},
              { (byte)ResponseStatus.Interrupted, "Assessment interrupted"}
        };
                        Name = responseStatus[Id.Value].ToString();
                    }
                    break;
                default:
                    Name = string.Empty;
                    break;
            }
            return Name;
        }
        public string GetTemplateCode(int Id, byte type)
        {
            var TemplateCodes = new Dictionary<byte, string> {
            { (byte)MessageType.Email, "EM"},
            { (byte)MessageType.Notifications, "NO"},
            { (byte)MessageType.SMS, "SM"},
            { (byte)MessageType.JobTemplates, "JT"}
        };
            string code = TemplateCodes[type].ToString() + "" + Id;
            return code;
        }
        public bool ValidHttpURL(string s)
        {
            if (!Regex.IsMatch(s, @"^https?:\/\/", RegexOptions.IgnoreCase))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public string GetCandidateUnSubscribeURL(string CandProfId, string EmailId)
        {
            string URL = this.appSettings.AppSettingsProperties.CandidateAppUrl + "/Candidate/UnSubscribe/";
            SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
            var CandId = simpleEncrypt.passwordEncrypt(CandProfId);
            CandId = System.Web.HttpUtility.UrlEncode(CandId);
            URL = URL + CandId + "/" + EmailId;
            return URL;
        }

        #endregion

        #region Gateway apis 

        public async Task<List<UsersViewModel>> GetUserbyTypes(List<int> Type, int? PuId)
        {
            List<UsersViewModel> data = null;
            data = await dbContext.GetUsers();
            if (Type != null)
            {
                if (Type.Count > 0)
                {
                    data = data.Where(x => Type.Contains(x.UserType) && x.Status == (byte)RecordStatus.Active).GroupBy(x => x.UserId).Select(grp => grp.First()).ToList();
                }
                else
                {
                    data = data.Where(x => x.Status == (byte)RecordStatus.Active).GroupBy(x => x.UserId).Select(grp => grp.First()).ToList();
                }
                if (PuId > 0)
                {
                    data = data.Where(x => x.PuId == PuId).ToList();
                }
                foreach (var item in data)
                {
                    item.ProfilePhoto = item.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + item.UserId + "/ProfilePhoto/" + item.ProfilePhoto : string.Empty;
                }
            }
            return data;
        }


        public async Task<List<ClientViewModel>> ListClients(int? UserId, int UserType)
        {
            _ = new List<ClientViewModel>();
            List<ClientViewModel> data = await dbContext.GetClients(UserId, UserType);
            return data;
        }
        #endregion

        #region Happiness Apis 
        public string HappinessApiAuthenticate()
        {
            string token = string.Empty;
            var userName = appSettings.AppSettingsProperties.HappinessApiUsername;
            var password = appSettings.AppSettingsProperties.HappinessApiPassword;
            var ApplicationId = appSettings.AppSettingsProperties.HappinessApplicationId;

            var client = new HttpClient { BaseAddress = new Uri(appSettings.AppSettingsProperties.HappinessApiUrl) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{userName}:{password}")));

            var userParam = new JObject
                {
                    { "Username", userName },
                    { "Password", password},
                    { "ApplicationId", ApplicationId }
                };

            HttpContent request = new StringContent(userParam.ToString(), Encoding.UTF8, "application/json");
            var response = client.PostAsync("authenticate", request).Result;
            var responseContent = response.Content;
            string responseString = responseContent.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<HappinessApiBaseViewModel<UserTokenResult>>(responseString);
            if (result.Status)
            {
                token = result.Result.Elements.Token;
            }

            return token;
        }

        public async Task<List<AssessmentViewModel>> GetAssessmentList()
        {
            var data = new List<AssessmentViewModel>();
            var getAssessmentRequestViewmodel = new GetAssessmentRequestViewmodel
            {
                SurveyExpiryDays = this.appSettings.AppSettingsProperties.SurveyExpiryDays,
                WhenToSend = DateTime.UtcNow.AddMinutes(this.appSettings.AppSettingsProperties.WhenToSend).ToString("dd-MMM-yyyy HH:mm:ss")
            };
            var token = HappinessApiAuthenticate();
            if (!string.IsNullOrEmpty(token))
            {
                using var client = new HttpClientService();
                var response = await client.PostAsync(appSettings.AppSettingsProperties.HappinessApiUrl, "api/v1/Survey/Hire/Template", token, getAssessmentRequestViewmodel);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var respne = JsonConvert.DeserializeObject<HappinessApiBaseViewModel<AssessmentsModel>>(responseContent);
                    if (respne.Status)
                    {
                        foreach (var item in respne.Result.elements)
                        {
                            data.Add(new AssessmentViewModel
                            {
                                Id = item.surveyId,
                                PreviewUrl = item.previewUrl,
                                SurveyCode = item.surveyCode,
                                SurveyDesc = item.surveyCode,
                                SurveyRefCode = item.surveyRefCode,
                                SurveyName = item.surveyTitle,
                                SurveyType = item.surveyType
                            });
                        }
                    }
                }
            }
            return data;
        }
        #endregion    

        #region Workflow Code
        // Activity insert on candidate/job
        public void SaveActivity(List<CreateActivityViewModel> data)
        {
            if (data != null)
            {
                data = data.Distinct().ToList();
                foreach (var createActivityViewModel in data)
                {
                    var activityLog = new PhActivityLog
                    {
                        ActivityDesc = createActivityViewModel.ActivityDesc,
                        ActivityMode = createActivityViewModel.ActivityMode,
                        ActivityOn = createActivityViewModel.ActivityOn,
                        ActivityType = createActivityViewModel.ActivityType,
                        CreatedBy = createActivityViewModel.UserId,
                        Joid = createActivityViewModel.JobId,
                        UpdateStatusId = createActivityViewModel.UpdateStatusId,
                        CurrentStatusId = createActivityViewModel.CurrentStatusId,
                        CreatedDate = CurrentTime,
                        Status = (byte)RecordStatus.Active
                    };
                    dbContext.PhActivityLogs.Add(activityLog);
                    dbContext.SaveChanges();
                }
            }
        }

        // Audit insert on all actions 
        public void SaveAuditLog(List<CreateAuditViewModel> data)
        {
            if (data != null)
            {
                data = data.Distinct().ToList();
                foreach (var createAuditViewModel in data)
                {
                    var auditLog = new PhAuditLog
                    {
                        ActivityDesc = createAuditViewModel.ActivityDesc,
                        ActivitySubject = createAuditViewModel.ActivitySubject,
                        ActivityType = createAuditViewModel.ActivityType,
                        CreatedBy = createAuditViewModel.UserId,
                        CreatedDate = CurrentTime,
                        Status = (byte)RecordStatus.Active,
                        TaskId = createAuditViewModel.TaskID
                    };
                    dbContext.PhAuditLogs.Add(auditLog);
                    dbContext.SaveChanges();
                }
            }
        }

        // Workflow conditions

        public async Task<WorkFlowResponse> ExecuteWorkFlowConditions(WorkFlowRuleSearchViewModel getWorkFlowRuleSearchViewModel)
        {
            try
            {
                var WorkFlowUpdateResponse = new WorkFlowResponse
                {
                    Status = true,
                    Message = new List<string>()
                };
                WorkFlowUpdateResponse.WFNotifications = new List<WFNotifications>();

                var WorkFlow = new WorkFlowIdsViewModel();

                if (getWorkFlowRuleSearchViewModel.ActionMode != (byte)WorkflowActionMode.Other)
                {
                    if (getWorkFlowRuleSearchViewModel.TaskCode == TaskCode.CUS.ToString())
                    {
                        WorkFlow = (from wf in dbContext.PhWorkflows
                                    join wfd in dbContext.PhWorkflowsDets on wf.Id equals wfd.WorkflowId
                                    where wf.TaskCode == getWorkFlowRuleSearchViewModel.TaskCode && wf.ActionMode == getWorkFlowRuleSearchViewModel.ActionMode
                                    && wf.Status != (byte)RecordStatus.Delete && wfd.CurrentStatusId == getWorkFlowRuleSearchViewModel.CurrentStatusId
                                    && wfd.UpdateStatusId == getWorkFlowRuleSearchViewModel.UpdateStatusId
                                    && wfd.Status != (byte)RecordStatus.Delete
                                    select new WorkFlowIdsViewModel
                                    {
                                        Id = wf.Id
                                    }).FirstOrDefault();
                    }
                    else
                    {
                        WorkFlow = (from wf in dbContext.PhWorkflows
                                    join wfd in dbContext.PhWorkflowsDets on wf.Id equals wfd.WorkflowId
                                    where wf.TaskCode == getWorkFlowRuleSearchViewModel.TaskCode && wf.ActionMode == getWorkFlowRuleSearchViewModel.ActionMode
                                    && wf.Status != (byte)RecordStatus.Delete && wfd.CurrentStatusId == getWorkFlowRuleSearchViewModel.CurrentStatusId
                                    && wfd.UpdateStatusId != null
                                    && wfd.Status != (byte)RecordStatus.Delete
                                    select new WorkFlowIdsViewModel
                                    {
                                        Id = wf.Id
                                    }).FirstOrDefault();
                    }
                }
                else
                {

                    WorkFlow = (from wf in dbContext.PhWorkflows
                                join wfd in dbContext.PhWorkflowsDets on wf.Id equals wfd.WorkflowId
                                where wf.TaskCode == getWorkFlowRuleSearchViewModel.TaskCode && wf.ActionMode == getWorkFlowRuleSearchViewModel.ActionMode
                                && wf.Status != (byte)RecordStatus.Delete && wfd.Status != (byte)RecordStatus.Delete
                                select new WorkFlowIdsViewModel
                                {
                                    Id = wf.Id
                                }).FirstOrDefault();

                }

                getWorkFlowRuleSearchViewModel.UsersViewModel = new List<UsersViewModel>();
                var users = await dbContext.GetUsers();
                getWorkFlowRuleSearchViewModel.UsersViewModel = users.Where(x => x.Status == (byte)RecordStatus.Active).GroupBy(x => x.UserId).Select(grp => grp.First()).ToList();

                if (WorkFlow != null)
                {
                    var workFlowDtls = dbContext.PhWorkflowsDets.Where(x => x.WorkflowId == WorkFlow.Id && x.Status != (byte)RecordStatus.Delete).ToList();
                    if (getWorkFlowRuleSearchViewModel.ActionMode != (byte)WorkflowActionMode.Other)
                    {
                        var isExist = workFlowDtls.Where(x => x.CurrentStatusId == getWorkFlowRuleSearchViewModel.CurrentStatusId && x.UpdateStatusId != null).FirstOrDefault();
                        if (isExist != null)
                        {
                            string RequestDocuments = string.Empty;
                            var workFlowtoAction = workFlowDtls.Where(x => x.Id == isExist.Id
                            || (x.CurrentStatusId == null && x.UpdateStatusId == null)).OrderByDescending(x => x.UpdateStatusId).ToList();
                            if (workFlowtoAction.Count > 0)
                            {
                                var documentExist = workFlowDtls.Where(x => x.ActionType == (byte)WorkflowActionTypes.RequestDocuments).OrderByDescending(x => x.Id).FirstOrDefault();
                                if (documentExist != null)
                                {
                                    List<int> DocsReqstdIds = documentExist.DocsReqstdIds.Split(',').Select(int.Parse).ToList();
                                    var docTypeName = dbContext.PhRefMasters.Where(x => x.GroupId == 15 && DocsReqstdIds.Contains(x.Id)).Select(x => x.Rmvalue).ToList();
                                    foreach (var docName in docTypeName)
                                    {
                                        if (string.IsNullOrEmpty(RequestDocuments))
                                        {
                                            RequestDocuments += "" + docName + "";
                                        }
                                        else
                                        {
                                            RequestDocuments += ", " + docName + "";
                                        }
                                    }
                                }
                            }
                            foreach (var item in workFlowtoAction)
                            {
                                switch (item.ActionType)
                                {
                                    case (byte)WorkflowActionTypes.ChangeStatus:
                                        var changeStatusViewModel = new ChangeStatusViewModel
                                        {
                                            ActionMode = getWorkFlowRuleSearchViewModel.ActionMode,
                                            CanProfId = getWorkFlowRuleSearchViewModel.CanProfId,
                                            CurrentStatusId = getWorkFlowRuleSearchViewModel.CurrentStatusId,
                                            JobId = getWorkFlowRuleSearchViewModel.JobId,
                                            UpdatedStatusId = item.UpdateStatusId,
                                            UserId = getWorkFlowRuleSearchViewModel.UserId,
                                            TaskCode = getWorkFlowRuleSearchViewModel.TaskCode
                                        };
                                        var res = await ChangeStatusRules(changeStatusViewModel);
                                        if (!string.IsNullOrEmpty(res))
                                        {
                                            WorkFlowUpdateResponse.Status = false;
                                            WorkFlowUpdateResponse.Message.Add(res);
                                        }
                                        break;
                                    case (byte)WorkflowActionTypes.EmailNotification:
                                        if (item.AsmtOrTplId != null)
                                        {
                                            var notificationViewModel = new NotificationViewModel
                                            {
                                                CanProfId = getWorkFlowRuleSearchViewModel.CanProfId,
                                                JobId = getWorkFlowRuleSearchViewModel.JobId,
                                                TemplateId = item.AsmtOrTplId,
                                                ActionMode = getWorkFlowRuleSearchViewModel.ActionMode,
                                                SendTo = item.SendTo,
                                                SendType = item.SendMode,
                                                UserId = getWorkFlowRuleSearchViewModel.UserId,
                                                UserName = getWorkFlowRuleSearchViewModel.UserName,
                                                UserPassword = getWorkFlowRuleSearchViewModel.UserPassword,
                                                RequestDocuments = RequestDocuments,
                                                AssignTo = getWorkFlowRuleSearchViewModel.AssignTo,
                                                NoOfCvs = getWorkFlowRuleSearchViewModel.NoOfCvs,
                                                CVAssigntoTeamMembers = getWorkFlowRuleSearchViewModel.AssignTo_CV,
                                                LocationId = getWorkFlowRuleSearchViewModel.LocationId,
                                                IntentOfferContent = getWorkFlowRuleSearchViewModel.IntentOfferContent,
                                                IntentOfferRemarks = getWorkFlowRuleSearchViewModel.IntentOfferRemarks,
                                                DOJ = getWorkFlowRuleSearchViewModel.DOJ,
                                                TaskCode = getWorkFlowRuleSearchViewModel.TaskCode,
                                                UserIds = getWorkFlowRuleSearchViewModel.UserIds,
                                                SalaryProposalOfferBenefits = getWorkFlowRuleSearchViewModel.SalaryProposalOfferBenefits
                                            };
                                            notificationViewModel.UsersViewModel = new List<UsersViewModel>();
                                            notificationViewModel.UsersViewModel = getWorkFlowRuleSearchViewModel.UsersViewModel;
                                            var resEml = await EmailNotificationRules(notificationViewModel);
                                            if (!string.IsNullOrEmpty(resEml))
                                            {
                                                WorkFlowUpdateResponse.Status = false;
                                                WorkFlowUpdateResponse.Message.Add(resEml);
                                            }
                                        }
                                        else
                                        {
                                            string TemplateMsg = " Template is not found";
                                            if (!WorkFlowUpdateResponse.Message.Contains(TemplateMsg))
                                            {
                                                WorkFlowUpdateResponse.Status = false;
                                                WorkFlowUpdateResponse.Message.Add(TemplateMsg);
                                            }
                                        }
                                        break;
                                    case (byte)WorkflowActionTypes.RequestDocuments:
                                        if (!string.IsNullOrEmpty(item.DocsReqstdIds))
                                        {
                                            if (getWorkFlowRuleSearchViewModel.JobId != null && getWorkFlowRuleSearchViewModel.JobId != 0)
                                            {
                                                var requestDocumentViewModel = new RequestDocumentViewModel
                                                {
                                                    ActionMode = getWorkFlowRuleSearchViewModel.ActionMode,
                                                    CanProfId = getWorkFlowRuleSearchViewModel.CanProfId,
                                                    JobId = getWorkFlowRuleSearchViewModel.JobId,
                                                    RequestDocuments = item.DocsReqstdIds.Split(",").Select(int.Parse).ToArray(),
                                                    UserId = getWorkFlowRuleSearchViewModel.UserId,
                                                };
                                                var resDoc = RequestDocumentRules(requestDocumentViewModel);
                                                if (!string.IsNullOrEmpty(resDoc))
                                                {
                                                    WorkFlowUpdateResponse.Status = false;
                                                    WorkFlowUpdateResponse.Message.Add(resDoc);
                                                }
                                            }
                                            else
                                            {
                                                string JobIdMsg = " Job id is not there";
                                                if (!WorkFlowUpdateResponse.Message.Contains(JobIdMsg))
                                                {
                                                    WorkFlowUpdateResponse.Status = false;
                                                    WorkFlowUpdateResponse.Message.Add(JobIdMsg);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string ReqDocMsg = " Requested documents are empty";
                                            if (!WorkFlowUpdateResponse.Message.Contains(ReqDocMsg))
                                            {
                                                WorkFlowUpdateResponse.Status = false;
                                                WorkFlowUpdateResponse.Message.Add(ReqDocMsg);
                                            }
                                        }
                                        break;
                                    case (byte)WorkflowActionTypes.SMSNotification:

                                        break;
                                    case (byte)WorkflowActionTypes.SystemAlert:
                                        if (item.AsmtOrTplId != null)
                                        {
                                            var notificationViewModel = new NotificationViewModel
                                            {
                                                CanProfId = getWorkFlowRuleSearchViewModel.CanProfId,
                                                JobId = getWorkFlowRuleSearchViewModel.JobId,
                                                TemplateId = item.AsmtOrTplId,
                                                ActionMode = getWorkFlowRuleSearchViewModel.ActionMode,
                                                SendTo = item.SendTo,
                                                SendType = item.SendMode,
                                                UserId = getWorkFlowRuleSearchViewModel.UserId,
                                                UserName = getWorkFlowRuleSearchViewModel.UserName,
                                                UserPassword = getWorkFlowRuleSearchViewModel.UserPassword,
                                                RequestDocuments = getWorkFlowRuleSearchViewModel.RequestDocuments,
                                                AssignTo = getWorkFlowRuleSearchViewModel.AssignTo,
                                                NoOfCvs = getWorkFlowRuleSearchViewModel.NoOfCvs,
                                                CVAssigntoTeamMembers = getWorkFlowRuleSearchViewModel.AssignTo_CV,
                                                DOJ = getWorkFlowRuleSearchViewModel.DOJ
                                            };
                                            notificationViewModel.UsersViewModel = new List<UsersViewModel>();
                                            notificationViewModel.UsersViewModel = getWorkFlowRuleSearchViewModel.UsersViewModel;
                                            var resEml = await SystemAlertRules(notificationViewModel);
                                            if (!string.IsNullOrEmpty(resEml.Message) && WorkFlowUpdateResponse.WFNotifications.Count == 0)
                                            {
                                                WorkFlowUpdateResponse.Status = false;
                                                WorkFlowUpdateResponse.Message.Add(resEml.Message);
                                            }
                                            else
                                            {
                                                WorkFlowUpdateResponse.Status = true;
                                                WorkFlowUpdateResponse.isNotification = true;
                                                WorkFlowUpdateResponse.JoId = resEml.JobId;
                                                var WFNotifications = new WFNotifications
                                                {
                                                    NoteDesc = resEml.NoteDesc,
                                                    Title = resEml.Title,
                                                    UserIds = resEml.UserId
                                                };
                                                WorkFlowUpdateResponse.WFNotifications.Add(WFNotifications);
                                            }
                                        }
                                        break;
                                    default:

                                        break;
                                }
                            }
                        }
                        else
                        {
                            string WrkFwMsg = " No Workflows found";
                            if (!WorkFlowUpdateResponse.Message.Contains(WrkFwMsg))
                            {
                                WorkFlowUpdateResponse.Status = false;
                                WorkFlowUpdateResponse.Message.Add(WrkFwMsg);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in workFlowDtls)
                        {
                            switch (item.ActionType)
                            {
                                case (byte)WorkflowActionTypes.EmailNotification:
                                    if (item.AsmtOrTplId != null)
                                    {
                                        var notificationViewModel = new NotificationViewModel
                                        {
                                            CanProfId = getWorkFlowRuleSearchViewModel.CanProfId,
                                            JobId = getWorkFlowRuleSearchViewModel.JobId,
                                            TemplateId = item.AsmtOrTplId,
                                            ActionMode = getWorkFlowRuleSearchViewModel.ActionMode,
                                            SendTo = item.SendTo,
                                            SendType = item.SendMode,
                                            UserId = getWorkFlowRuleSearchViewModel.UserId,
                                            UserName = getWorkFlowRuleSearchViewModel.UserName,
                                            UserPassword = getWorkFlowRuleSearchViewModel.UserPassword,
                                            RequestDocuments = getWorkFlowRuleSearchViewModel.RequestDocuments,
                                            AssignTo = getWorkFlowRuleSearchViewModel.AssignTo,
                                            NoOfCvs = getWorkFlowRuleSearchViewModel.NoOfCvs,
                                            CVAssigntoTeamMembers = getWorkFlowRuleSearchViewModel.AssignTo_CV,
                                            LocationId = getWorkFlowRuleSearchViewModel.LocationId,
                                            IntentOfferContent = getWorkFlowRuleSearchViewModel.IntentOfferContent,
                                            IntentOfferRemarks = getWorkFlowRuleSearchViewModel.IntentOfferRemarks,
                                            DOJ = getWorkFlowRuleSearchViewModel.DOJ,
                                            TaskCode = getWorkFlowRuleSearchViewModel.TaskCode,
                                            UserIds = getWorkFlowRuleSearchViewModel.UserIds,
                                            SalaryProposalOfferBenefits = getWorkFlowRuleSearchViewModel.SalaryProposalOfferBenefits
                                        };
                                        notificationViewModel.UsersViewModel = new List<UsersViewModel>();
                                        notificationViewModel.UsersViewModel = getWorkFlowRuleSearchViewModel.UsersViewModel;
                                        var resEml = await EmailNotificationRules(notificationViewModel);
                                        if (!string.IsNullOrEmpty(resEml))
                                        {
                                            WorkFlowUpdateResponse.Status = false;
                                            WorkFlowUpdateResponse.Message.Add(resEml);
                                        }
                                    }
                                    else
                                    {
                                        string TemMsg = " Template is not found";
                                        if (!WorkFlowUpdateResponse.Message.Contains(TemMsg))
                                        {
                                            WorkFlowUpdateResponse.Status = false;
                                            WorkFlowUpdateResponse.Message.Add(TemMsg);
                                        }
                                    }
                                    break;
                                case (byte)WorkflowActionTypes.RequestDocuments:
                                    if (!string.IsNullOrEmpty(item.DocsReqstdIds))
                                    {
                                        if (getWorkFlowRuleSearchViewModel.JobId != null && getWorkFlowRuleSearchViewModel.JobId != 0)
                                        {
                                            var requestDocumentViewModel = new RequestDocumentViewModel
                                            {
                                                ActionMode = getWorkFlowRuleSearchViewModel.ActionMode,
                                                CanProfId = getWorkFlowRuleSearchViewModel.CanProfId,
                                                JobId = getWorkFlowRuleSearchViewModel.JobId,
                                                RequestDocuments = item.DocsReqstdIds.Split(",").Select(int.Parse).ToArray()
                                            };
                                            var resDoc = RequestDocumentRules(requestDocumentViewModel);
                                            if (!string.IsNullOrEmpty(resDoc))
                                            {
                                                WorkFlowUpdateResponse.Status = false;
                                                WorkFlowUpdateResponse.Message.Add(resDoc);
                                            }
                                        }
                                        else
                                        {
                                            string JobIdMsg = " Job id is not there";
                                            if (!WorkFlowUpdateResponse.Message.Contains(JobIdMsg))
                                            {
                                                WorkFlowUpdateResponse.Status = false;
                                                WorkFlowUpdateResponse.Message.Add(JobIdMsg);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string ReqDocMsg = " Requested documents are empty";
                                        if (!WorkFlowUpdateResponse.Message.Contains(ReqDocMsg))
                                        {
                                            WorkFlowUpdateResponse.Status = false;
                                            WorkFlowUpdateResponse.Message.Add(ReqDocMsg);
                                        }
                                    }
                                    break;
                                case (byte)WorkflowActionTypes.SMSNotification:

                                    break;
                                case (byte)WorkflowActionTypes.SystemAlert:
                                    if (item.AsmtOrTplId != null)
                                    {
                                        var notificationViewModel = new NotificationViewModel
                                        {
                                            CanProfId = getWorkFlowRuleSearchViewModel.CanProfId,
                                            JobId = getWorkFlowRuleSearchViewModel.JobId,
                                            TemplateId = item.AsmtOrTplId,
                                            ActionMode = getWorkFlowRuleSearchViewModel.ActionMode,
                                            SendTo = item.SendTo,
                                            SendType = item.SendMode,
                                            UserId = getWorkFlowRuleSearchViewModel.UserId,
                                            RequestDocuments = getWorkFlowRuleSearchViewModel.RequestDocuments,
                                            AssignTo = getWorkFlowRuleSearchViewModel.AssignTo,
                                            NoOfCvs = getWorkFlowRuleSearchViewModel.NoOfCvs,
                                            CVAssigntoTeamMembers = getWorkFlowRuleSearchViewModel.AssignTo_CV
                                        };
                                        notificationViewModel.UsersViewModel = new List<UsersViewModel>();
                                        notificationViewModel.UsersViewModel = getWorkFlowRuleSearchViewModel.UsersViewModel;
                                        var resEml = await SystemAlertRules(notificationViewModel);
                                        var WFNotifications = new WFNotifications();
                                        if (!string.IsNullOrEmpty(resEml.Message) && WorkFlowUpdateResponse.WFNotifications.Count == 0)
                                        {
                                            WorkFlowUpdateResponse.Status = false;
                                            if (!WorkFlowUpdateResponse.Message.Contains(resEml.Message))
                                            {
                                                WorkFlowUpdateResponse.Message.Add(resEml.Message);
                                            }
                                        }
                                        else
                                        {
                                            WorkFlowUpdateResponse.Status = true;
                                            WorkFlowUpdateResponse.isNotification = true;
                                            WorkFlowUpdateResponse.JoId = resEml.JobId;
                                            var WFNotification = new WFNotifications
                                            {
                                                NoteDesc = resEml.NoteDesc,
                                                Title = resEml.Title,
                                                UserIds = resEml.UserId
                                            };
                                            WorkFlowUpdateResponse.WFNotifications.Add(WFNotification);
                                        }
                                    }
                                    break;
                                default:

                                    break;
                            }
                        }
                    }
                }
                else
                {
                    string WrkFwMsg = " No Workflows found";
                    if (!WorkFlowUpdateResponse.Message.Contains(WrkFwMsg))
                    {
                        WorkFlowUpdateResponse.Status = false;
                        WorkFlowUpdateResponse.Message.Add(WrkFwMsg);
                    }
                }

                return WorkFlowUpdateResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Workflow rule of Change Status 
        public async Task<string> ChangeStatusRules(ChangeStatusViewModel changeStatusViewModel)
        {
            // Activity
            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
            string response = string.Empty;
            if (changeStatusViewModel.ActionMode == (byte)WorkflowActionMode.Candidate)
            {
                var jobCandidate = dbContext.PhJobCandidates.Where(x => x.Joid == changeStatusViewModel.JobId && x.CandProfId == changeStatusViewModel.CanProfId).FirstOrDefault();
                var candidateProfile = dbContext.PhCandidateProfiles.Where(x => x.Id == changeStatusViewModel.CanProfId).FirstOrDefault();

                var currentCandidateStatus = dbContext.PhCandStatusSes.Select(x => new { x.Title, x.Id }).Where(x => x.Id == changeStatusViewModel.CurrentStatusId).FirstOrDefault();
                var updateCandidateStatus = dbContext.PhCandStatusSes.Select(x => new { x.Title, x.Id, x.Cscode }).Where(x => x.Id == changeStatusViewModel.UpdatedStatusId).FirstOrDefault();

                if (updateCandidateStatus != null)
                {
                    int? CurrentStageId = null;
                    if (changeStatusViewModel.CurrentStatusId != null)
                    {
                        bool isValid = true;
                        if (changeStatusViewModel.TaskCode == TaskCode.CAJ.ToString() && jobCandidate.IsTagged == false)
                        {
                            isValid = false;
                            if (candidateProfile.SourceId == (byte)SourceType.Indeed
                                  || candidateProfile.SourceId == (byte)SourceType.LinkedIn
                                  || candidateProfile.SourceId == (byte)SourceType.MonsterIndia
                                  || candidateProfile.SourceId == (byte)SourceType.MonsterGulf
                                  || candidateProfile.SourceId == (byte)SourceType.Naukri
                                  || candidateProfile.SourceId == (byte)SourceType.NaukriGulf
                                  || candidateProfile.SourceId == (byte)SourceType.References
                                  || candidateProfile.SourceId == (byte)SourceType.Shine
                                  || candidateProfile.SourceId == (byte)SourceType.PersonalContact)
                            {
                                isValid = true;
                            }
                        }
                        if (isValid)
                        {
                            var CurrentStageDtls = dbContext.PhCandStageMaps.Where(x => x.CandStatusId == changeStatusViewModel.CurrentStatusId
                       && x.Status != (byte)RecordStatus.Delete).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                            if (CurrentStageDtls != null)
                            {
                                CurrentStageId = CurrentStageDtls.StageId;
                            }
                        }
                    }

                    var UpdatedStageDtls = dbContext.PhCandStageMaps.Where(x => x.CandStatusId == changeStatusViewModel.UpdatedStatusId
                    && x.Status != (byte)RecordStatus.Delete).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                    if (UpdatedStageDtls != null)
                    {
                        jobCandidate.StageId = UpdatedStageDtls.StageId;
                    }
                    if (!string.IsNullOrEmpty(changeStatusViewModel.Remarks) && candidateProfile != null)
                    {
                        candidateProfile.Remarks = changeStatusViewModel.Remarks;
                        dbContext.PhCandidateProfiles.Update(candidateProfile);
                    }
                    if (candidateProfile != null && !string.IsNullOrEmpty(changeStatusViewModel.Remarks))
                    {
                        candidateProfile.Remarks = changeStatusViewModel.Remarks;
                        dbContext.PhCandidateProfiles.Update(candidateProfile);
                    }
                    // Available
                    if (candidateProfile != null && updateCandidateStatus.Cscode != "BLT" && updateCandidateStatus.Cscode != "SUC")
                    {
                        candidateProfile.CandOverallStatus = (byte)CandOverallStatus.Available;
                        dbContext.PhCandidateProfiles.Update(candidateProfile);
                    }
                    // Blacklisted
                    if (candidateProfile != null && updateCandidateStatus.Cscode == "BLT")
                    {
                        candidateProfile.CandOverallStatus = (byte)CandOverallStatus.Blacklisted;
                        dbContext.PhCandidateProfiles.Update(candidateProfile);
                    }
                    // SUCCESSFUL
                    if (candidateProfile != null && updateCandidateStatus.Cscode == "SUC")
                    {
                        candidateProfile.CandOverallStatus = (byte)CandOverallStatus.Joined;
                        dbContext.PhCandidateProfiles.Update(candidateProfile);
                    }

                    jobCandidate.CandProfStatus = changeStatusViewModel.UpdatedStatusId;
                    jobCandidate.UpdatedDate = CurrentTime;
                    dbContext.PhJobCandidates.Update(jobCandidate);
                    dbContext.SaveChanges();

                    if (changeStatusViewModel.CurrentStatusId != null)
                    {
                        bool isValid = true;
                        if (changeStatusViewModel.TaskCode == TaskCode.CAJ.ToString() && jobCandidate.IsTagged == false)
                        {
                            isValid = false;
                            if (candidateProfile.SourceId == (byte)SourceType.Indeed
                                  || candidateProfile.SourceId == (byte)SourceType.LinkedIn
                                  || candidateProfile.SourceId == (byte)SourceType.MonsterIndia
                                  || candidateProfile.SourceId == (byte)SourceType.MonsterGulf
                                  || candidateProfile.SourceId == (byte)SourceType.Naukri
                                  || candidateProfile.SourceId == (byte)SourceType.NaukriGulf
                                  || candidateProfile.SourceId == (byte)SourceType.References
                                  || candidateProfile.SourceId == (byte)SourceType.Shine
                                  || candidateProfile.SourceId == (byte)SourceType.PersonalContact)
                            {
                                isValid = true;
                            }
                        }
                        if (isValid)
                        {
                            var OpngStatusCounter = dbContext.PhJobOpeningStatusCounters.Where(x => x.Joid == changeStatusViewModel.JobId &&
                       x.StageId == CurrentStageId && x.CandStatusId == changeStatusViewModel.CurrentStatusId).FirstOrDefault();
                            if (OpngStatusCounter != null)
                            {
                                if (OpngStatusCounter.Counter > 0)
                                {
                                    OpngStatusCounter.Counter -= 1;
                                }
                                OpngStatusCounter.UpdatedBy = changeStatusViewModel.UserId;
                                OpngStatusCounter.UpdatedDate = CurrentTime;

                                dbContext.SaveChanges();
                            }
                        }
                    }
                    if (changeStatusViewModel.UpdatedStatusId != null)
                    {
                        var OpngStatusCounter = dbContext.PhJobOpeningStatusCounters.Where(x => x.Joid == changeStatusViewModel.JobId &&
                       x.StageId == jobCandidate.StageId && x.CandStatusId == changeStatusViewModel.UpdatedStatusId).FirstOrDefault();
                        if (OpngStatusCounter != null)
                        {
                            OpngStatusCounter.Counter += 1;
                            OpngStatusCounter.UpdatedBy = changeStatusViewModel.UserId;
                            OpngStatusCounter.UpdatedDate = CurrentTime;

                            dbContext.SaveChanges();
                        }
                        else
                        {
                            if (changeStatusViewModel.JobId != null)
                            {
                                var phJobOpeningStatusCounter = new PhJobOpeningStatusCounter
                                {
                                    UpdatedDate = CurrentTime,
                                    UpdatedBy = changeStatusViewModel.UserId,
                                    Counter = 1,
                                    CandStatusId = changeStatusViewModel.UpdatedStatusId,
                                    Joid = changeStatusViewModel.JobId.Value,
                                    StageId = jobCandidate.StageId,
                                    Status = (byte)RecordStatus.Active
                                };
                                dbContext.PhJobOpeningStatusCounters.Add(phJobOpeningStatusCounter);
                                dbContext.SaveChanges();
                            }
                        }
                    }

                    var jobAssessments = dbContext.PhJobOpeningAssmts.Where(x => x.Joid == changeStatusViewModel.JobId && x.CandStatusId == changeStatusViewModel.UpdatedStatusId && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                    if (jobAssessments != null)
                    {
                        var JobTitle = dbContext.PhJobOpenings.Where(x => x.Id == changeStatusViewModel.JobId).Select(x => x.JobTitle).FirstOrDefault();
                        var canassssment = dbContext.PhJobCandidateAssemts.Where(x => x.Joid == changeStatusViewModel.JobId
                        && x.CandProfId == candidateProfile.Id && x.AssmtId == jobAssessments.AssessmentId && x.ResponseStatus != (byte)ContactDistributionResponseStatus.Interepted).FirstOrDefault();
                        if (canassssment == null)
                        {
                            canassssment = new PhJobCandidateAssemt
                            {
                                AssmtId = jobAssessments.AssessmentId,
                                JoassmtId = jobAssessments.Id,
                                CreatedBy = changeStatusViewModel.UserId,
                                Joid = (byte)changeStatusViewModel.JobId,
                                CandProfId = candidateProfile.Id,
                                CreatedDate = CurrentTime,
                                Status = (byte)RecordStatus.Active,
                                ResponseUrl = string.Empty,
                                ResponseStatus = (byte)ContactDistributionResponseStatus.NotStarted
                            };

                            dbContext.PhJobCandidateAssemts.Add(canassssment);
                            dbContext.SaveChanges();

                            var sendAssessmentViewModel = new SendAssessmentViewModel
                            {
                                EmailTemplateID = this.appSettings.AppSettingsProperties.AssessmentTemplateId,
                                FromEmail = string.Empty,
                                FromName = string.Empty,
                                GroupID = string.Empty,
                                ReplyToEmail = string.Empty,
                                Subject = " Assessment for the " + changeStatusViewModel.JobId + " - " + JobTitle + " position",
                                SurveyExpiryDays = this.appSettings.AppSettingsProperties.SurveyExpiryDays,
                                SurveyID = jobAssessments.AssessmentId,
                                SurveyInstanceID = string.Empty,
                                WhenToSend = DateTime.UtcNow.AddMinutes(this.appSettings.AppSettingsProperties.WhenToSend).ToString("dd-MMM-yyyy HH:mm:ss")
                            };
                            sendAssessmentViewModel.Contacts = new List<AssessmentContacts>();
                            var AssessmentContacts = new AssessmentContacts
                            {
                                Email = candidateProfile.EmailId,
                                FirstName = candidateProfile.CandName,
                                LastName = string.Empty
                            };
                            AssessmentContacts.AdditionalData = new AssessmentAdditionalData();
                            var AssessmentAdditionalData = new AssessmentAdditionalData
                            {
                                JOB_TITLE = JobTitle
                            };
                            AssessmentContacts.AdditionalData = AssessmentAdditionalData;
                            sendAssessmentViewModel.Contacts.Add(AssessmentContacts);

                            var resp = await SendAssessmentRules(sendAssessmentViewModel);
                            if (!string.IsNullOrEmpty(resp.DistributionId))
                            {
                                canassssment.DistributionId = resp.DistributionId;

                                var phJobOpeningActvCounter = dbContext.PhJobOpeningActvCounters.Where(x => x.Joid == changeStatusViewModel.JobId && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                                if (phJobOpeningActvCounter != null)
                                {
                                    phJobOpeningActvCounter.AsmtCounter += 1;
                                    dbContext.PhJobOpeningActvCounters.Update(phJobOpeningActvCounter);
                                    await dbContext.SaveChangesAsync();
                                }

                                dbContext.PhJobCandidateAssemts.Update(canassssment);
                                dbContext.SaveChanges();

                                var avtyLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Candidate,
                                    ActivityOn = changeStatusViewModel.CanProfId,
                                    JobId = changeStatusViewModel.JobId == null ? 0 : changeStatusViewModel.JobId.Value,
                                    ActivityType = (byte)LogActivityType.AssessementUpdates,
                                    ActivityDesc = " has Shared Assessment to " + candidateProfile.CandName + "",
                                    UserId = changeStatusViewModel.UserId
                                };
                                activityList.Add(avtyLog);
                                SaveActivity(activityList);
                            }
                            else
                            {
                                canassssment.ResponseStatus = (byte)ContactDistributionResponseStatus.Interepted;
                                canassssment.UpdatedDate = CurrentTime;

                                dbContext.PhJobCandidateAssemts.Update(canassssment);
                                dbContext.SaveChanges();
                            }
                        }
                    }

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Candidate Profile Status",
                        ActivityDesc = "  updated Candidate Profile Status",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = changeStatusViewModel.CanProfId,
                        UserId = changeStatusViewModel.UserId
                    };
                    audList.Add(auditLog);

                    string oldStatus = "null";
                    if (currentCandidateStatus != null)
                    {
                        oldStatus = currentCandidateStatus?.Title;
                    }

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = changeStatusViewModel.CanProfId,
                        JobId = changeStatusViewModel.JobId == null ? 0 : changeStatusViewModel.JobId.Value,
                        ActivityType = (byte)LogActivityType.StatusUpdates,
                        ActivityDesc = " has Updated Status " + changeStatusViewModel.CanProfId + " from " + oldStatus + " to " + updateCandidateStatus?.Title + "",
                        UserId = changeStatusViewModel.UserId,
                        CurrentStatusId = changeStatusViewModel.CurrentStatusId,
                        UpdateStatusId = changeStatusViewModel.UpdatedStatusId
                    };
                    activityList.Add(activityLog);
                }
                else
                {
                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Candidate Status",
                        ActivityDesc = " not able to Update beacuase of " + response + "",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = changeStatusViewModel.CanProfId,
                        UserId = changeStatusViewModel.UserId
                    };
                    audList.Add(auditLog);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = changeStatusViewModel.JobId,
                        JobId = changeStatusViewModel.JobId == null ? 0 : changeStatusViewModel.JobId.Value,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has not able to Update beacuase of " + response + "",
                        UserId = changeStatusViewModel.UserId,
                    };
                    activityList.Add(activityLog);
                }
            }
            else
            {
                var job = dbContext.PhJobOpenings.Where(x => x.Id == changeStatusViewModel.JobId).FirstOrDefault();
                var CurrentJobStatus = dbContext.PhJobStatusSes.Select(x => new { x.Title, x.Id }).Where(x => x.Id == changeStatusViewModel.CurrentStatusId).FirstOrDefault();
                var UpdateJobStatus = dbContext.PhJobStatusSes.Select(x => new { x.Title, x.Id }).Where(x => x.Id == changeStatusViewModel.UpdatedStatusId).FirstOrDefault();
                if (job != null)
                {
                    var JobAssignments = dbContext.PhJobAssignments.Where(x => x.Joid == changeStatusViewModel.JobId).FirstOrDefault();
                    if (JobAssignments != null)
                    {
                        if (UpdateJobStatus != null)
                        {
                            job.JobOpeningStatus = changeStatusViewModel.UpdatedStatusId.Value;
                            job.UpdatedDate = CurrentTime;

                            dbContext.PhJobOpenings.Update(job);
                            dbContext.SaveChanges();

                            string oldStatus = "null";
                            if (CurrentJobStatus != null)
                            {
                                oldStatus = CurrentJobStatus?.Title;
                            }

                            // audit 
                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = " Candidate Profile Status",
                                ActivityDesc = " has Updated JOB Status " + changeStatusViewModel.JobId + " from " + oldStatus + " to " + UpdateJobStatus?.Title + "",
                                ActivityType = (byte)AuditActivityType.StatusUpdates,
                                TaskID = changeStatusViewModel.CanProfId,
                                UserId = changeStatusViewModel.UserId
                            };
                            audList.Add(auditLog);

                            // activity
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = changeStatusViewModel.JobId,
                                JobId = changeStatusViewModel.JobId == null ? 0 : changeStatusViewModel.JobId.Value,
                                ActivityType = (byte)LogActivityType.StatusUpdates,
                                ActivityDesc = " has Updated JOB Status " + changeStatusViewModel.JobId + " from " + CurrentJobStatus?.Title + " to " + UpdateJobStatus?.Title + "",
                                UserId = changeStatusViewModel.UserId,
                                CurrentStatusId = changeStatusViewModel.CurrentStatusId,
                                UpdateStatusId = changeStatusViewModel.UpdatedStatusId
                            };
                            activityList.Add(activityLog);

                            response = string.Empty;
                        }
                        else
                        {
                            response = "Status is not found";
                        }
                    }
                }
                else
                {
                    response = " Job is not found";
                }
                if (response == " Job is not found" || response == " Status is not found")
                {
                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Candidate Status",
                        ActivityDesc = " has not able to Update beacuase of " + response + "",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = changeStatusViewModel.JobId,
                        UserId = changeStatusViewModel.UserId
                    };
                    audList.Add(auditLog);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = changeStatusViewModel.JobId,
                        JobId = changeStatusViewModel.JobId == null ? 0 : changeStatusViewModel.JobId.Value,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has not able to Update beacuase of " + response + "",
                        UserId = changeStatusViewModel.UserId
                    };
                    activityList.Add(activityLog);
                }
            }

            if (activityList.Count > 0)
            {
                SaveActivity(activityList);
            }
            if (audList.Count > 0)
            {
                SaveAuditLog(audList);
            }

            return response;
        }

        // Workflow rule of System alerts 

        public async Task<SystemAlertResponseViewModel> SystemAlertRules(NotificationViewModel notificationViewModel)
        {
            string response = string.Empty;
            var resp = new List<ActorViewModel>();
            var systemAlertResponseViewModel = new SystemAlertResponseViewModel();

            var templateDetls = dbContext.PhMessageTemplates.Where(x => x.Id == notificationViewModel.TemplateId
            && x.Status == (byte)RecordStatus.Active).FirstOrDefault();
            if (templateDetls != null)
            {
                systemAlertResponseViewModel.JobId = notificationViewModel.JobId;
                systemAlertResponseViewModel.Title = templateDetls.TplSubject;
                string TplBody = templateDetls.TplBody;
                if (!string.IsNullOrEmpty(TplBody))
                {
                    resp = await GetActorDtls(notificationViewModel);
                    if (resp.Count > 0)
                    {
                        TplBody = TplBody.Replace("[JOB_ID]", notificationViewModel.JobId != null ? notificationViewModel.JobId.ToString() : string.Empty);
                        TplBody = TplBody.Replace("[CAND_ID]", notificationViewModel.CanProfId != null ? notificationViewModel.CanProfId.ToString() : string.Empty);
                        TplBody = TplBody.Replace("[CLIENT_NAME]", resp[0].ClientName);
                        TplBody = TplBody.Replace("[SPOC_NAME]", resp[0].SpocName);
                        TplBody = TplBody.Replace("[JOB_TITLE]", resp[0].JobTitle);
                        TplBody = TplBody.Replace("[CAND_NAME]", resp[0].CandName);
                        TplBody = TplBody.Replace("[RECRUITER_NAME]", resp[0].RecName);
                        TplBody = TplBody.Replace("[BDM_NAME]", resp[0].BdmName);
                        TplBody = TplBody.Replace("[HIRE_MANAGER]", resp[0].HireManagerName);
                        if (resp[0].AssignNoCvs != 0)
                        {
                            TplBody = TplBody.Replace("[CV_COUNT]", resp[0].AssignNoCvs.ToString());
                        }
                        else
                        {
                            TplBody = TplBody.Replace("[CV_COUNT]", resp[0].NoCvs.ToString());
                        }

                        systemAlertResponseViewModel.NoteDesc = TplBody;
                        List<int> UserIds = new List<int>();
                        foreach (var user in resp)
                        {
                            if (!UserIds.Contains(user.UserId))
                            {
                                UserIds.Add(user.UserId);
                            }
                        }
                        systemAlertResponseViewModel.UserId = UserIds.ToArray();
                    }
                    else
                    {
                        systemAlertResponseViewModel.Message = " No data found to send notification";
                    }
                }
                else
                {
                    systemAlertResponseViewModel.Message = " Template is not found to send notifications";
                }
            }
            else
            {
                systemAlertResponseViewModel.Message = " Template is not found to send notifications";
            }
            return systemAlertResponseViewModel;
        }

        // Workflow rule of Email Notifications 

        public async Task<string> EmailNotificationRules(NotificationViewModel notificationViewModel)
        {
            string response = string.Empty;
            var resp = new List<ActorViewModel>();
            string PiSignature = string.Empty;
            try
            {
                var piUser = notificationViewModel.UsersViewModel.Where(x => x.UserId == notificationViewModel.UserId).FirstOrDefault();
                string SmtpLoginName = appSettings.smtpEmailConfig.SmtpLoginName;
                string SmtpLoginPassword = appSettings.smtpEmailConfig.SmtpLoginPassword;
                notificationViewModel.Signature = " Talent Acquisition Group, <br /> ParamInfo";

                if (piUser != null)
                {
                    var smtpConfiguration = dbContext.PhUsersConfigs.Where(x => x.UserId == piUser.UserId && x.VerifyFlag).FirstOrDefault();
                    var simpleEncrypt = new SimpleEncrypt();
                    if (smtpConfiguration != null)
                    {
                        SmtpLoginName = smtpConfiguration.UserName;
                        SmtpLoginPassword = simpleEncrypt.passwordDecrypt(smtpConfiguration.PasswordHash);
                    }

                    if (!string.IsNullOrEmpty(piUser.EmailSignature))
                    {
                        notificationViewModel.Signature = piUser.EmailSignature;
                    }
                }
                var smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                                    SmtpLoginName, SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl, SmtpLoginName,
                                    appSettings.smtpEmailConfig.SmtpFromName);

                var templateDetls = dbContext.PhMessageTemplates.Where(x => x.Id == notificationViewModel.TemplateId
                && x.Status == (byte)(RecordStatus.Active)).FirstOrDefault();

                if (templateDetls != null)
                {
                    string FullBody = File.ReadAllText("EmailTemplates/DefaultEmailTemplate.html");
                    string TplBody = FullBody.Replace("!dynamicBody", templateDetls.TplBody);
                    string TplSubject = templateDetls.TplSubject;
                    if (!string.IsNullOrEmpty(TplBody))
                    {
                        resp = await GetActorDtls(notificationViewModel);
                        if (resp.Count > 0)
                        {
                            var respItem = resp[0];
                            if (respItem != null)
                            {
                                PiSignature = " Talent Acquisition Group, <br /> ParamInfo";

                                TplBody = TplBody.Replace("[CLIENT_NAME]", respItem.ClientName);
                                TplBody = TplBody.Replace("[CLIENT_EMAIL]", respItem.ClientEmail);
                                TplBody = TplBody.Replace("[CLIENT_CONTACT_NO]", respItem.ClientContactNo);
                                TplBody = TplBody.Replace("[SPOC_NAME]", resp[0].SpocName);

                                TplBody = TplBody.Replace("[JOB_ID]", notificationViewModel.JobId.ToString());
                                TplBody = TplBody.Replace("[JOB_TITLE]", respItem.JobTitle);
                                TplBody = TplBody.Replace("[JOB_DESC]", respItem.JobDesc);

                                TplBody = TplBody.Replace("[JOB_STATUS]", respItem.JobStatus);
                                TplBody = TplBody.Replace("[JOB_LOCATION]", respItem.JobLocation + "/" + respItem.JobCountry);

                                TplBody = TplBody.Replace("[JOB_CURRENCY]", respItem.JobCurrencyName);
                                if (respItem.OfferGrossPackagePerMonth != null)
                                {
                                    string PerMonth = String.Format(CultureInfo.InvariantCulture, "{0:0,0}", respItem.OfferGrossPackagePerMonth);
                                    TplBody = TplBody.Replace("[JOB_NET_SAL_MONTH]", PerMonth + " (" + respItem.OfferPackageCurrency + ") ");
                                }
                                else
                                {
                                    TplBody = TplBody.Replace("[JOB_NET_SAL_MONTH]", string.Empty);
                                }

                                TplBody = TplBody.Replace("[CAND_ID]", notificationViewModel.CanProfId.ToString());
                                TplBody = TplBody.Replace("[CAND_NAME]", respItem.CandName);
                                TplBody = TplBody.Replace("[CAND_EMAIL]", respItem.CandEmail);
                                TplBody = TplBody.Replace("[COMPANY_NAME]", this.appSettings.AppSettingsProperties.CompanyName);


                                if (notificationViewModel.LocationId != null)
                                {
                                    var dtls = await dbContext.GetCompanyLocations(notificationViewModel.LocationId.Value);
                                    if (dtls.Count > 0)
                                    {
                                        var lctnDtls = dtls[0];
                                        var address = lctnDtls.address1 + " <br /> " + lctnDtls.address2 + " <br /> " + lctnDtls.address3 + " <br /> Land Mark : " + lctnDtls.land_mark + " ";

                                        TplBody = TplBody.Replace("[COMPANY_ADDRESS]", address);
                                    }
                                    else
                                    {
                                        TplBody = TplBody.Replace("[COMPANY_ADDRESS]", string.Empty);
                                    }
                                }
                                else
                                {
                                    TplBody = TplBody.Replace("[COMPANY_ADDRESS]", string.Empty);
                                }

                                TplBody = TplBody.Replace("[BDM_NAME]", respItem.BdmName);
                                TplBody = TplBody.Replace("[HIRE_MANAGER]", respItem.HireManagerName);
                                TplBody = TplBody.Replace("[INTERVIEW_DATE]", respItem.InterviewDate);

                                if (!string.IsNullOrEmpty(respItem.InterviewStartTime))
                                {
                                    var INTERVIEW_TIME = string.Empty;
                                    if (!string.IsNullOrEmpty(respItem.InterviewEndTime))
                                    {
                                        INTERVIEW_TIME = respItem.InterviewStartTime + " - " + respItem.InterviewEndTime;
                                    }
                                    else
                                    {
                                        INTERVIEW_TIME = respItem.InterviewStartTime;
                                    }
                                    if (!string.IsNullOrEmpty(respItem.InterviewTimeZone))
                                        INTERVIEW_TIME += $" ({respItem.InterviewTimeZone})";

                                    TplBody = TplBody.Replace("[INTERVIEW_TIME]", INTERVIEW_TIME);
                                }
                                else
                                {
                                    TplBody = TplBody.Replace("[INTERVIEW_TIME]", string.Empty);
                                }

                                TplBody = TplBody.Replace("[INTERVIEW_LOCATION]", respItem.InterviewLoc);
                                TplBody = TplBody.Replace("[INTERVIEW_MODE]", respItem.InterviewMode);


                                TplBody = TplBody.Replace("[CAND_CONTACT_NO]", respItem.CandContactNo);
                                TplBody = TplBody.Replace("[CAND_STATUS]", respItem.CandStatus);

                                if (notificationViewModel.DOJ == null)
                                {
                                    if (respItem.JoiningDate == null)
                                    {
                                        TplBody = TplBody.Replace("[JOINING_DATE]", string.Empty);
                                    }
                                    else
                                    {
                                        TplBody = TplBody.Replace("[JOINING_DATE]", respItem.JoiningDate.Value.ToString("dd/MM/yyyy"));
                                    }
                                }
                                else
                                {
                                    TplBody = TplBody.Replace("[JOINING_DATE]", notificationViewModel.DOJ.Value.ToString("dd/MM/yyyy"));
                                }

                                if (resp[0].AssignNoCvs != 0)
                                {
                                    TplBody = TplBody.Replace("[CV_COUNT]", resp[0].AssignNoCvs.ToString());
                                }
                                else
                                {
                                    TplBody = TplBody.Replace("[CV_COUNT]", resp[0].NoCvs.ToString());
                                }


                                var minYears = 0;
                                var maxYears = 0;
                                if (respItem.MinExpeInMonths != null)
                                {
                                    minYears = ConvertYears(respItem.MinExpeInMonths);
                                }
                                if (respItem.MaxExpeInMonths != null)
                                {
                                    maxYears = ConvertYears(respItem.MaxExpeInMonths);
                                }
                                if (minYears != 0 && maxYears != 0)
                                {
                                    TplBody = TplBody.Replace("[JOB_EXPE]", minYears + " - " + maxYears + " Years");
                                }
                                else
                                {
                                    if (minYears != 0)
                                    {
                                        TplBody = TplBody.Replace("[JOB_EXPE]", minYears + " Years");
                                    }
                                    if (maxYears != 0)
                                    {
                                        TplBody = TplBody.Replace("[JOB_EXPE]", maxYears + " Years");
                                    }
                                }

                                if (!string.IsNullOrEmpty(notificationViewModel.UserName))
                                {
                                    TplBody = TplBody.Replace("[USER_NAME]", notificationViewModel.UserName);
                                }
                                else
                                {
                                    TplBody = TplBody.Replace("[USER_NAME]", respItem.CandEmail);
                                }

                                TplBody = TplBody.Replace("[USER_PASSWORD]", notificationViewModel.UserPassword);
                                TplBody = TplBody.Replace("[USER_SIGNATURE_DIV]", notificationViewModel.Signature);
                                TplBody = TplBody.Replace("[PI_SIGNATURE_DIV]", PiSignature);

                                if (respItem.JobPostedOn != null)
                                {
                                    TplBody = TplBody.Replace("[JOB_POSTED_ON]", respItem.JobPostedOn.Value.ToString("dd/MM/yyyy"));
                                }
                                else
                                {
                                    TplBody = TplBody.Replace("[JOB_POSTED_ON]", string.Empty);
                                }
                                if (respItem.JobEndDate != null)
                                {
                                    TplBody = TplBody.Replace("[JOB_END_DATE]", respItem.JobEndDate.Value.ToString("dd/MM/yyyy"));
                                }
                                else
                                {
                                    TplBody = TplBody.Replace("[JOB_END_DATE]", string.Empty);
                                }

                                TplBody = TplBody.Replace("[RECRUITER_NAME]", respItem.RecName);
                                TplBody = TplBody.Replace("[RECRUITER_POSITION]", respItem.RecPosition);
                                TplBody = TplBody.Replace("[RECRUITE_EMAILID]", respItem.RecEmailId);
                                TplBody = TplBody.Replace("[RECRUITER_PHONE_NUMBER]", respItem.RecPhoneNum);

                                string skillTr = string.Empty;
                                string skillTable = "<table border='0' cellspacing='0' cellpadding='0'> !SkillTr </table>";
                                if (respItem.JobSkills != null)
                                {
                                    for (int i = 0; i < respItem.JobSkills.Count; i++)
                                    {
                                        skillTr = skillTr + " " +
                                            "<tr><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;'>" + respItem.JobSkills[i].Technology +
                                            "</td><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;text-align:right;'>" + respItem.JobSkills[i].ExpYears +
                                            " Years </td><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;text-align:right;'>" + respItem.JobSkills[i].ExpMonth +
                                            " Months </td></tr>";
                                    }
                                    skillTable = skillTable.Replace("!SkillTr", skillTr);
                                    TplBody = TplBody.Replace("[JOB_SKILLS]", skillTable);
                                }

                                TplBody = TplBody.Replace("[CAN_REQ_DOCUMENT]", notificationViewModel.RequestDocuments);
                                TplBody = TplBody.Replace("[CAND_LOGIN]", appSettings.AppSettingsProperties.CandidateAppUrl + "/login?jobid=" + notificationViewModel.JobId);
                                TplBody = TplBody.Replace("[JOB_APPLY_LINK]", appSettings.AppSettingsProperties.CandidateAppUrl + "/home");


                                TplSubject = TplSubject.Replace("[JOB_ID]", notificationViewModel.JobId.ToString());
                                TplSubject = TplSubject.Replace("[CAND_ID]", notificationViewModel.CanProfId.ToString());
                                TplSubject = TplSubject.Replace("[CLIENT_NAME]", respItem.ClientName);
                                TplSubject = TplSubject.Replace("[JOB_TITLE]", respItem.JobTitle);
                                TplSubject = TplSubject.Replace("[CAND_NAME]", respItem.CandName);
                                TplSubject = TplSubject.Replace("[CAND_EMAIL]", respItem.CandEmail);
                                TplSubject = TplSubject.Replace("[COMPANY_NAME]", this.appSettings.AppSettingsProperties.CompanyName);

                                TplBody = TplBody.Replace("[INTENT_REMARKS]", notificationViewModel.IntentOfferRemarks);
                            }

                            string recCCmail = string.Empty;
                            List<string> EmailIds = new List<string>();
                            foreach (var user in resp)
                            {
                                if (!EmailIds.Contains(user.ToEmail))
                                {
                                    EmailIds.Add(user.ToEmail);
                                }
                            }

                            if (notificationViewModel.SalaryProposalOfferBenefits != null && notificationViewModel.SalaryProposalOfferBenefits.Length > 0)
                            {
                                string OFFER_BENEFITS = " <span style='font-size: 14px;font-weight:bold;'>Other Benefits: </span> <br>";
                                foreach (var benefit in notificationViewModel.SalaryProposalOfferBenefits)
                                {
                                    OFFER_BENEFITS += benefit + "<br >";
                                }
                                TplBody = TplBody.Replace("[SALARY_PROPOSAL_OFFER_BENEFIT]", OFFER_BENEFITS);
                            }
                            else
                            {
                                TplBody = TplBody.Replace("[SALARY_PROPOSAL_OFFER_BENEFIT]", string.Empty);
                            }

                            if (notificationViewModel.UserIds != null && notificationViewModel.UserIds.Count > 0)
                            {
                                recCCmail = string.Join(",", notificationViewModel.UserIds);
                            }

                            if (EmailIds.Count > 0)
                            {
                                string ToEmailIds = string.Join(",", EmailIds);
                                if (notificationViewModel.IntentOfferContent == null)
                                {
                                    _ = smtp.SendMail(ToEmailIds, TplSubject, TplBody, string.Empty);
                                }
                                else
                                {
                                    var fileName = "Intent_Offer_" + notificationViewModel.JobId + "_" + notificationViewModel.CanProfId + "_" + respItem.CandName;
                                    _ = smtp.SendMail(ToEmailIds, TplSubject, TplBody, recCCmail, notificationViewModel.IntentOfferContent, fileName);
                                }
                            }
                        }
                        else
                        {
                            response = " No data found to send notification";
                        }
                    }
                    else
                    {
                        response = " Template is not found to send notifications";
                    }
                }
                else
                {
                    response = " Template is not found to send notifications";
                }

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Workflow rules of actor details 

        public async Task<List<ActorViewModel>> GetActorDtls(NotificationViewModel notificationViewModel)
        {
            try
            {
                var actList = new List<ActorViewModel>();
                var actDtls = new ActorViewModel();
                var actResp = await dbContext.GetJobDtlsPerWorkflowRules(notificationViewModel.JobId);
                if (actResp != null)
                {
                    actDtls.JobId = notificationViewModel.JobId == null ? 0 : notificationViewModel.JobId.Value;
                    actDtls.JobDesc = actResp.JobDesc;
                    actDtls.MaxExpeInMonths = actResp.MaxExpeInMonths;
                    actDtls.MinExpeInMonths = actResp.MinExpeInMonths;
                    actDtls.JobCountry = actResp.JobCountry;
                    actDtls.JobLocation = actResp.JobLocation;
                    actDtls.JobStatus = actResp.JobOpeningStatusName;
                    actDtls.JobCurrencyName = actResp.JobCurrencyName;
                    actDtls.JobTitle = actResp.JobTitle;
                    actDtls.JobEndDate = actResp.JobEndDate;
                    actDtls.JobPostedOn = actResp.JobPostedOn;
                    actDtls.BroughtBy = actResp.BroughtBy;
                    actDtls.BdmName = actResp.BroughtByName;
                    actDtls.ClientName = actResp.ClientName;
                    actDtls.RequestDocuments = notificationViewModel.RequestDocuments;
                    actDtls.NoCvs = notificationViewModel.NoOfCvs;

                    actDtls.JobSkills = new List<GetOpeningSkillSetViewModel>();
                    var jobSkill = await dbContext.GetJobSkillsPerWorkflowRules(notificationViewModel.JobId);
                    foreach (var item in jobSkill)
                    {
                        actDtls.JobSkills.Add(new GetOpeningSkillSetViewModel { ExpMonth = item.ExpMonth, ExpYears = item.ExpYears, Technology = item.Technology });
                    }

                    if (actResp.SPOCID != null)
                    {
                        var clientSpocDtls = await dbContext.GetClientSpocs(actResp.ClientID);
                        if (clientSpocDtls.Count > 0)
                        {
                            var clientSpoc = clientSpocDtls.Where(x => x.Id == actResp.SPOCID).FirstOrDefault();
                            if (clientSpoc != null)
                            {
                                actDtls.ClientContactNo = clientSpoc.ContactNo;
                                actDtls.ClientEmail = clientSpoc.Email;
                                actDtls.SpocName = clientSpoc.Name;
                            }
                        }
                    }
                }

                var canDtls = await dbContext.GetCandDtlsPerWorkflowRules(notificationViewModel.JobId, notificationViewModel.CanProfId);
                if (canDtls != null)
                {
                    actDtls.CandContactNo = canDtls.ContactNo;
                    actDtls.CandEmail = canDtls.EmailId;
                    actDtls.CandName = canDtls.CandName;
                    actDtls.CandStatus = canDtls.CandStatus;
                    actDtls.OfferGrossPackagePerMonth = canDtls.OfferGrossPackagePerMonth;
                    actDtls.OfferNetSalMonth = canDtls.OfferNetSalMonth;
                    actDtls.OfferPackageCurrency = canDtls.OfferPackageCurrency;
                    if (canDtls.InterviewDate != null)
                    {
                        actDtls.InterviewDate = Convert.ToDateTime(canDtls.InterviewDate.Value).ToString("dd/MM/yyyy");
                    }
                    if (canDtls.ModeOfInterview != null)
                    {
                        actDtls.InterviewMode = Enum.GetName(typeof(ModeOfInterview), canDtls.ModeOfInterview);
                    }
                    actDtls.InterviewStartTime = canDtls.InterviewStartTime;
                    actDtls.InterviewEndTime = canDtls.InterviewEndTime;
                    actDtls.InterviewTimeZone = canDtls.InterviewTimeZone;
                    actDtls.InterviewLoc = canDtls.InterviewLoc;
                    actDtls.JoiningDate = canDtls.JoiningDate;
                }

                if (actResp.PuId != null)
                {
                    var piUserManager = notificationViewModel.UsersViewModel.Where(x => x.UserType == (byte)UserType.Admin
                    && x.PuId == actResp.PuId).FirstOrDefault();
                    if (piUserManager != null)
                    {
                        actDtls.HireManagerName = piUserManager.FirstName + " " + (piUserManager.LastName ?? "");
                    }
                }
                if (!string.IsNullOrEmpty(actDtls.HireManagerName))
                {
                    var piUserManager = notificationViewModel.UsersViewModel.Where(x => x.UserType == (byte)UserType.Admin).FirstOrDefault();
                    if (piUserManager != null)
                    {
                        actDtls.HireManagerName = piUserManager.FirstName + " " + (piUserManager.LastName ?? "");
                    }
                }
                if (notificationViewModel.SendType == (byte)SendMode.All)
                {
                    if (notificationViewModel.ActionMode == (byte)WorkflowActionMode.Candidate)
                    {
                        if (notificationViewModel.SendTo == (byte)UserType.Recruiter)
                        {
                            if (canDtls != null)
                            {
                                if (canDtls.RecruiterId != null)
                                {
                                    var piUser = notificationViewModel.UsersViewModel.Where(x => x.UserId == canDtls.RecruiterId).FirstOrDefault();
                                    if (piUser != null)
                                    {
                                        var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                        actorViewModel.UserId = piUser.UserId; // piHire userId
                                        actorViewModel.Name = piUser.FirstName + " " + (piUser.LastName ?? "");
                                        actorViewModel.ToEmail = piUser.Email;
                                        actorViewModel.RecEmailId = piUser.Email;
                                        actorViewModel.RecName = piUser.FirstName + " " + (piUser.LastName ?? "");
                                        actorViewModel.RecPhoneNum = piUser.MobileNo;
                                        actorViewModel.Signature = piUser.EmailSignature;
                                        actorViewModel.RecPosition = piUser.RoleName;
                                        actList.Add(actorViewModel);
                                    }
                                }
                            }

                        }
                        else if (notificationViewModel.SendTo == (byte)UserType.BDM)
                        {
                            if (actDtls.BroughtBy != null)
                            {
                                var piBroughtByUser = notificationViewModel.UsersViewModel.Where(x => x.UserId == actDtls.BroughtBy).FirstOrDefault();
                                if (piBroughtByUser != null)
                                {
                                    var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                    actorViewModel.UserId = piBroughtByUser.UserId;  // piHire userId
                                    actorViewModel.Name = piBroughtByUser.FirstName + " " + (piBroughtByUser.LastName ?? "");
                                    actorViewModel.ToEmail = piBroughtByUser.Email;
                                    actList.Add(actorViewModel);
                                }
                            }
                        }
                        else if (notificationViewModel.SendTo == (byte)UserType.Candidate)
                        {
                            if (canDtls != null)
                            {
                                var piRec = notificationViewModel.UsersViewModel.Where(x => x.UserId == canDtls.RecruiterId).FirstOrDefault();
                                var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                actorViewModel.UserId = canDtls.UserId;
                                actorViewModel.ToEmail = canDtls.ToEmail;
                                actorViewModel.ccEmail = piRec?.Email;
                                actorViewModel.CandName = canDtls.CandName;
                                actList.Add(actorViewModel);
                            }
                        }
                    }
                    else
                    {
                        if (notificationViewModel.SendTo == (byte)UserType.Recruiter)
                        {
                            bool CvsAssigntoRec = false;
                            var JobAssignments = new List<PhJobAssignment>();
                            if (notificationViewModel.CVAssigntoTeamMembers != null)
                            {
                                if (notificationViewModel.CVAssigntoTeamMembers.Count > 0)
                                {
                                    CvsAssigntoRec = true;
                                }
                            }
                            if (CvsAssigntoRec)
                            {
                                foreach (var item in notificationViewModel.CVAssigntoTeamMembers)
                                {
                                    var dtls = notificationViewModel.UsersViewModel.Where(x => x.UserId == item.UserId).FirstOrDefault();
                                    if (dtls != null)
                                    {
                                        var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                        actorViewModel.UserId = dtls.UserId;
                                        actorViewModel.Name = dtls.FirstName + " " + (dtls.LastName ?? "");
                                        actorViewModel.ToEmail = dtls.Email;
                                        actorViewModel.RecEmailId = dtls.Email;
                                        actorViewModel.RecName = dtls.FirstName + " " + (dtls.LastName ?? "");
                                        actorViewModel.RecPhoneNum = dtls.MobileNo;
                                        actorViewModel.Signature = dtls.EmailSignature;
                                        actorViewModel.RecPosition = dtls.RoleName;
                                        actorViewModel.AssignNoCvs = item.NoOfCvs;
                                        actList.Add(actorViewModel);
                                    }
                                }
                            }
                            else
                            {
                                JobAssignments = dbContext.PhJobAssignments.Where(x => x.Joid == notificationViewModel.JobId && x.DeassignBy == null).ToList();
                                if (JobAssignments != null)
                                {
                                    if (JobAssignments.Count > 0)
                                    {
                                        foreach (var item in JobAssignments)
                                        {
                                            var dtls = notificationViewModel.UsersViewModel.Where(x => x.UserId == item.AssignedTo).FirstOrDefault();
                                            if (dtls != null)
                                            {
                                                var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                                actorViewModel.UserId = dtls.UserId;
                                                actorViewModel.Name = dtls.FirstName + " " + (dtls.LastName ?? "");
                                                actorViewModel.ToEmail = dtls.Email;
                                                actorViewModel.RecEmailId = dtls.Email;
                                                actorViewModel.RecName = dtls.FirstName + " " + (dtls.LastName ?? "");
                                                actorViewModel.RecPhoneNum = dtls.MobileNo;
                                                actorViewModel.Signature = dtls.EmailSignature;
                                                actorViewModel.RecPosition = dtls.RoleName;
                                                actList.Add(actorViewModel);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else if (notificationViewModel.SendTo == (byte)UserType.BDM)
                        {
                            if (actDtls != null)
                            {
                                if (actDtls.BroughtBy != null)
                                {
                                    var piBroughtByUser = notificationViewModel.UsersViewModel.Where(x => x.UserId == actDtls.BroughtBy).FirstOrDefault();
                                    if (piBroughtByUser != null)
                                    {
                                        var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                        actorViewModel.UserId = piBroughtByUser.UserId;
                                        actorViewModel.Name = piBroughtByUser.FirstName + " " + (piBroughtByUser.LastName ?? "");
                                        actorViewModel.ToEmail = piBroughtByUser.Email;
                                        actList.Add(actorViewModel);
                                    }
                                }
                            }
                        }
                        else if (notificationViewModel.SendTo == (byte)UserType.Candidate)
                        {
                            var Candidate = await dbContext.GetJobCandDtlsPerWorkflowRules(notificationViewModel.JobId);
                            if (Candidate.Count > 0)
                            {
                                foreach (var item in Candidate)
                                {
                                    var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                    actorViewModel.UserId = item.CandUserId;
                                    actorViewModel.Name = item.CandName;
                                    actorViewModel.ToEmail = item.EmailId;
                                    actorViewModel.ccEmail = item.RecEmail;
                                    actList.Add(actorViewModel);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (notificationViewModel.ActionMode == (byte)WorkflowActionMode.Candidate)
                    {
                        if (notificationViewModel.SendTo == (byte)UserType.Recruiter)
                        {
                            if (canDtls != null)
                            {
                                if (canDtls.RecruiterId != null)
                                {
                                    var piUser = notificationViewModel.UsersViewModel.Where(x => x.UserId == canDtls.RecruiterId).FirstOrDefault();
                                    if (piUser != null)
                                    {
                                        var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                        actorViewModel.UserId = piUser.UserId; // pi Hire user id 
                                        actorViewModel.Name = piUser.FirstName + " " + (piUser.LastName ?? "");
                                        actorViewModel.ToEmail = piUser.Email;
                                        actorViewModel.RecEmailId = piUser.Email;
                                        actorViewModel.RecName = piUser.FirstName + " " + (piUser.LastName ?? "");
                                        actorViewModel.RecPhoneNum = piUser.MobileNo;
                                        actorViewModel.Signature = piUser.EmailSignature;
                                        actorViewModel.RecPosition = piUser.RoleName;
                                        actList.Add(actorViewModel);
                                    }
                                }
                            }
                        }
                        else if (notificationViewModel.SendTo == (byte)UserType.BDM)
                        {
                            if (actDtls != null)
                            {
                                if (actDtls.BroughtBy != null)
                                {
                                    var piBroughtByUser = notificationViewModel.UsersViewModel.Where(x => x.UserId == actDtls.BroughtBy).FirstOrDefault();
                                    if (piBroughtByUser != null)
                                    {
                                        var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                        actorViewModel.UserId = piBroughtByUser.UserId;
                                        actorViewModel.Name = piBroughtByUser.FirstName + " " + (piBroughtByUser.LastName ?? "");
                                        actorViewModel.ToEmail = piBroughtByUser.Email;
                                        actList.Add(actorViewModel);
                                    }
                                }
                            }
                        }
                        else if (notificationViewModel.SendTo == (byte)UserType.Candidate)
                        {
                            if (canDtls != null)
                            {
                                var piRec = notificationViewModel.UsersViewModel.Where(x => x.UserId == canDtls.RecruiterId).FirstOrDefault();
                                var piBroughtby = notificationViewModel.UsersViewModel.Where(x => x.UserId == actResp.BroughtBy).FirstOrDefault();
                                var admins = notificationViewModel.UsersViewModel.Where(x => x.UserType == (byte)UserType.Admin || x.UserType == (byte)UserType.SuperAdmin).ToList();
                                var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                actorViewModel.UserId = canDtls.UserId;
                                actorViewModel.ToEmail = canDtls.ToEmail;
                                actorViewModel.ccEmail = piRec?.Email;
                                actorViewModel.ccEmail += "," + piBroughtby?.Email;
                                foreach (var item in admins)
                                {
                                    actorViewModel.ccEmail += "," + item?.Email;
                                }
                                actorViewModel.CandName = canDtls.CandName;
                                actList.Add(actorViewModel);
                            }
                        }
                    }
                    else
                    {
                        if (notificationViewModel.SendTo == (byte)UserType.Recruiter)
                        {
                            var JobAssignments = new PhJobAssignment();
                            if (notificationViewModel.AssignTo != 0)
                            {
                                JobAssignments = dbContext.PhJobAssignments.Where(x => x.Joid == notificationViewModel.JobId
                                && x.AssignedTo == notificationViewModel.AssignTo).FirstOrDefault();
                            }
                            else
                            {
                                if (canDtls != null)
                                {
                                    JobAssignments = dbContext.PhJobAssignments.Where(x => x.Joid == notificationViewModel.JobId
                                && x.AssignedTo == canDtls.RecruiterId).FirstOrDefault();
                                }
                            }
                            if (JobAssignments == null || notificationViewModel.AssignTo == 0)
                            {
                                JobAssignments = dbContext.PhJobAssignments.Where(x => x.Joid == notificationViewModel.JobId
                                && x.DeassignBy == null).FirstOrDefault();
                            }
                            if (JobAssignments != null)
                            {
                                var dtls = notificationViewModel.UsersViewModel.Where(x => x.UserId == JobAssignments.AssignedTo).FirstOrDefault();
                                if (dtls != null)
                                {
                                    var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                    actorViewModel.UserId = dtls.UserId;
                                    actorViewModel.Name = dtls.FirstName + " " + (dtls.LastName ?? "");
                                    actorViewModel.ToEmail = dtls.Email;
                                    actorViewModel.RecEmailId = dtls.Email;
                                    actorViewModel.RecName = dtls.FirstName + " " + (dtls.LastName ?? "");
                                    actorViewModel.RecPhoneNum = dtls.MobileNo;
                                    actorViewModel.Signature = dtls.EmailSignature;
                                    actorViewModel.RecPosition = dtls.RoleName;
                                    actList.Add(actorViewModel);
                                }
                            }
                        }
                        else if (notificationViewModel.SendTo == (byte)UserType.BDM)
                        {
                            if (actDtls != null)
                            {
                                if (actDtls.BroughtBy != null)
                                {
                                    var piBroughtByUser = notificationViewModel.UsersViewModel.Where(x => x.UserId == actDtls.BroughtBy).FirstOrDefault();
                                    if (piBroughtByUser != null)
                                    {
                                        var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                        actorViewModel.UserId = piBroughtByUser.UserId;
                                        actorViewModel.Name = piBroughtByUser.FirstName + " " + (piBroughtByUser.LastName ?? "");
                                        actorViewModel.ToEmail = piBroughtByUser.Email;
                                        actList.Add(actorViewModel);
                                    }
                                }
                            }
                        }
                        else if (notificationViewModel.SendTo == (byte)UserType.Candidate)
                        {
                            if (canDtls != null)
                            {
                                var piRec = notificationViewModel.UsersViewModel.Where(x => x.UserId == canDtls.RecruiterId).FirstOrDefault();
                                var piBroughtby = notificationViewModel.UsersViewModel.Where(x => x.UserId == actResp.BroughtBy).FirstOrDefault();
                                var admins = notificationViewModel.UsersViewModel.Where(x => x.UserType == (byte)UserType.Admin || x.UserType == (byte)UserType.SuperAdmin).ToList();
                                var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                actorViewModel.UserId = canDtls.UserId;
                                actorViewModel.ToEmail = canDtls.ToEmail;
                                actorViewModel.ccEmail = piRec?.Email;
                                actorViewModel.ccEmail += "," + piBroughtby?.Email;
                                foreach (var item in admins)
                                {
                                    actorViewModel.ccEmail += "," + item?.Email;
                                }
                                actorViewModel.CandName = canDtls.CandName;
                                actList.Add(actorViewModel);
                            }
                        }
                    }
                }
                if (notificationViewModel.SendTo == (byte)UserType.Admin)
                {
                    if (notificationViewModel.SendType == (byte)SendMode.All)
                    {
                        var piUser = notificationViewModel.UsersViewModel.Where(x => x.UserType == (byte)UserType.Admin && x.PuId == actResp.PuId).ToList();
                        if (piUser.Count > 0)
                        {
                            foreach (var item in piUser)
                            {
                                var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                actorViewModel.UserId = item.UserId;
                                actorViewModel.Name = item.FirstName + " " + (item.LastName ?? "");
                                actorViewModel.ToEmail = item.Email;
                                actList.Add(actorViewModel);
                            }
                        }
                    }
                    else
                    {
                        var piUser = notificationViewModel.UsersViewModel.Where(x => x.UserType == (byte)UserType.Admin && x.PuId == actResp.PuId).FirstOrDefault();
                        if (piUser != null)
                        {
                            var actorViewModel = GetActorViewModel(actDtls, canDtls);
                            actorViewModel.UserId = piUser.UserId;
                            actorViewModel.Name = piUser.FirstName + " " + (piUser.LastName ?? "");
                            actorViewModel.ToEmail = piUser.Email;
                            actList.Add(actorViewModel);
                        }
                        else
                        {
                            piUser = notificationViewModel.UsersViewModel.Where(x => x.UserType == (byte)UserType.Admin).FirstOrDefault();
                            if (piUser != null)
                            {
                                var actorViewModel = GetActorViewModel(actDtls, canDtls);
                                actorViewModel.UserId = piUser.UserId;
                                actorViewModel.Name = piUser.FirstName + " " + (piUser.LastName ?? "");
                                actorViewModel.ToEmail = piUser.Email;
                                actList.Add(actorViewModel);
                            }
                        }
                    }
                }

                return actList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActorViewModel GetActorViewModel(ActorViewModel actDtls, CandDtlsWorkflowModel canDtls)
        {
            var actorViewModel = new ActorViewModel
            {
                RequestDocuments = actDtls?.RequestDocuments,
                CandContactNo = canDtls?.ContactNo,
                CandEmail = canDtls?.EmailId,
                CandName = canDtls?.CandName,
                CandStatus = canDtls?.CandStatus,
                OfferGrossPackagePerMonth = canDtls?.OfferGrossPackagePerMonth,
                OfferNetSalMonth = canDtls?.OfferNetSalMonth,
                OfferPackageCurrency = canDtls?.OfferPackageCurrency,
                JoiningDate = actDtls?.JoiningDate,

                InterviewLoc = actDtls?.InterviewLoc,
                InterviewDate = actDtls?.InterviewDate,
                InterviewMode = actDtls?.InterviewMode,
                InterviewStartTime = actDtls?.InterviewStartTime,
                InterviewEndTime = actDtls?.InterviewEndTime,
                InterviewTimeZone = actDtls?.InterviewTimeZone,

                JobId = actDtls.JobId,
                JobDesc = actDtls?.JobDesc,
                MaxExpeInMonths = actDtls?.MaxExpeInMonths,
                MinExpeInMonths = actDtls?.MinExpeInMonths,
                JobCountry = actDtls?.JobCountry,
                JobLocation = actDtls?.JobLocation,
                JobStatus = actDtls?.JobStatus,
                JobTitle = actDtls?.JobTitle,
                JobEndDate = actDtls?.JobEndDate,
                JobPostedOn = actDtls?.JobPostedOn,
                BdmName = actDtls?.BdmName,
                BroughtBy = actDtls?.BroughtBy,
                JobSkills = actDtls?.JobSkills,
                JobCurrencyName = actDtls?.JobCurrencyName,
                NoCvs = actDtls.NoCvs,

                ClientName = actDtls?.ClientName,
                ClientContactNo = actDtls?.ClientContactNo,
                ClientEmail = actDtls?.ClientEmail,
                SpocName = actDtls?.SpocName,
                HireManagerName = actDtls?.HireManagerName,
            };
            return actorViewModel;
        }


        // Workflow rule of Request Documents
        public string RequestDocumentRules(RequestDocumentViewModel requestDocumentViewModel)
        {
            foreach (var docType in requestDocumentViewModel.RequestDocuments)
            {
                var refDocs = dbContext.PhRefMasters.Where(x => x.GroupId == 15 && x.Id == docType).FirstOrDefault();
                var fileGroup = (byte)FileGroup.Other;
                if (refDocs != null)
                {
                    string RmDesc = string.Empty;
                    RmDesc = refDocs.Rmdesc?.Trim();
                    if (RmDesc == FileGroup.Education.ToString())
                    {
                        fileGroup = (byte)FileGroup.Education;
                    }
                    if (RmDesc == FileGroup.Employment.ToString())
                    {
                        fileGroup = (byte)FileGroup.Employment;
                    }
                    if (RmDesc == FileGroup.Profile.ToString())
                    {
                        fileGroup = (byte)FileGroup.Profile;
                    }
                    var response = dbContext.PhCandidateDocs.Where(x => x.Joid == requestDocumentViewModel.JobId && x.CandProfId == requestDocumentViewModel.CanProfId
                                          && x.DocType == refDocs.Rmvalue && x.DocStatus == (byte)DocStatus.Requested && x.Status != (byte)RecordStatus.Delete).ToList();
                    if (response == null)
                    {
                        var candidateDoc = new PhCandidateDoc
                        {
                            Joid = requestDocumentViewModel.JobId == null ? 0 : requestDocumentViewModel.JobId.Value,
                            Status = (byte)RecordStatus.Active,
                            CandProfId = requestDocumentViewModel.CanProfId == null ? 0 : requestDocumentViewModel.CanProfId.Value,
                            CreatedBy = requestDocumentViewModel.UserId,
                            CreatedDate = CurrentTime,
                            UploadedBy = requestDocumentViewModel.UserId,
                            FileGroup = fileGroup,
                            DocType = refDocs.Rmvalue,
                            FileName = string.Empty,
                            FileType = string.Empty,
                            DocStatus = (byte)DocStatus.Requested,
                            Remerks = string.Empty
                        };

                        dbContext.PhCandidateDocs.Add(candidateDoc);
                        dbContext.SaveChanges();

                        // activity
                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = candidateDoc.CandProfId,
                            JobId = candidateDoc.Joid,
                            ActivityType = (byte)LogActivityType.Other,
                            ActivityDesc = " has request to candidate for " + candidateDoc.DocType + " document",
                            UserId = requestDocumentViewModel.UserId
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);
                    }
                }

            }
            return string.Empty;
        }

        // Workflow rule of Send Assessment
        public async Task<AssessmentResponseViewmodel> SendAssessmentRules(SendAssessmentViewModel sendAssessmentViewModel)
        {
            var assessmentResponseViewmodel = new AssessmentResponseViewmodel();
            var token = HappinessApiAuthenticate();
            if (!string.IsNullOrEmpty(token))
            {
                using var client = new HttpClientService();
                var response = await client.PostAsync(appSettings.AppSettingsProperties.HappinessApiUrl, "api/v1/Distribution/Hire/Email", token, sendAssessmentViewModel);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<HappinessApiBaseViewModel<DistributionResult>>(responseContent);
                    if (result.Status)
                    {
                        assessmentResponseViewmodel.DistributionId = result.Result.DistributionID;
                    }
                }
            }
            return assessmentResponseViewmodel;
        }

        // Workflow rule of SMS Notification
        public string SMSNotificationRules(SMSNotificationViewModel sMSNotificationViewModel)
        {
            return string.Empty;
        }

        #endregion

        #region Auto assignment 

        public async Task<bool> JobAutoAssignment(AutoAssignmentSearchViewModel autoAssignmentSearchViewModel)
        {
            bool response = false;
            var users = await dbContext.GetRecuiters();
            users = users.Where(x => x.puId == autoAssignmentSearchViewModel.Puid && x.UserStatus == (byte)RecordStatus.Active).GroupBy(x => x.UserId).Select(grp => grp.First()).ToList();

            List<RecsViewModel> allRecs = new List<RecsViewModel>();
            List<RecsViewModel> localRecs = new List<RecsViewModel>();
            List<RecsViewModel> nonLocalRecs = new List<RecsViewModel>();
            List<RecsViewModel> readyToAssign = new List<RecsViewModel>();

            var jobPeriodLeaveData = dbContext.PhEmpLeaveRequests.Where(x => autoAssignmentSearchViewModel.StartDate <= x.LeaveStartDate && autoAssignmentSearchViewModel.Enddate >= x.LeaveEndDate).ToList();
            var shiftDtls = dbContext.PhShiftDetls.Where(x => x.Status != (byte)RecordStatus.Delete).ToList();

            foreach (var item in users)
            {
                var recsViewModel = new RecsViewModel
                {
                    JobsCount = item.JobsCount,
                    Location = item.Location,
                    LocationId = item.LocationId,
                    Name = item.Name,
                    puId = item.puId,
                    ShiftId = item.ShiftId,
                    UserId = item.UserId
                };
                var leaveObj = (from stus in jobPeriodLeaveData
                                where stus.EmpId == item.UserId && autoAssignmentSearchViewModel.StartDate.Date == stus.LeaveStartDate.Date
                                && stus.LeaveEndDate.Date == autoAssignmentSearchViewModel.Enddate.Date
                                && stus.Status == (byte)LeaveStatus.Accepted
                                select new
                                {
                                    stus.LeaveType,
                                    stus.Id
                                }).FirstOrDefault();

                var Weekend = shiftDtls.Where(x => x.ShiftId == item.ShiftId && x.IsAlternateWeekend == false && x.IsWeekend == true).Select(x => x.DayName).ToList();
                var WeekModelObj = shiftDtls.Where(x => x.ShiftId == item.ShiftId && x.IsAlternateWeekend == true && x.IsWeekend == true).Select(x => new SubClassWeekend { WeekendModel = x.WeekendModel, DayName = x.DayName }).ToList();

                if (!Weekend.Contains(autoAssignmentSearchViewModel.StartDate.DayOfWeek.ToString())
                    && CheckAlternativeWeekend(autoAssignmentSearchViewModel.StartDate, WeekModelObj) && leaveObj == null)
                {
                    allRecs.Add(recsViewModel);
                }
            }
            int counter = 0;
            if (autoAssignmentSearchViewModel.OfficeId != 0)
            {
                localRecs = allRecs.Where(x => x.LocationId == autoAssignmentSearchViewModel.OfficeId).OrderBy(x => x.JobsCount).ToList();
                nonLocalRecs = allRecs.Where(x => x.LocationId != autoAssignmentSearchViewModel.OfficeId).OrderBy(x => x.JobsCount).ToList();
                foreach (var item in localRecs)
                {
                    counter += 1;
                    if (counter <= 2)
                    {
                        readyToAssign.Add(item);
                    }
                    else
                    {
                        break;
                    }
                }
                if (nonLocalRecs.Count > 0)
                {
                    readyToAssign.Add(nonLocalRecs[0]);
                }
            }
            else
            {
                allRecs = allRecs.OrderBy(x => x.JobsCount).ToList();

                foreach (var item in allRecs)
                {
                    counter += 1;
                    if (counter <= 3)
                    {
                        readyToAssign.Add(item);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (readyToAssign.Count > 0)
            {
                var phJobAssignments = new List<PhJobAssignment>();
                int noOfCvs = 0;
                while (noOfCvs < autoAssignmentSearchViewModel.NoOfCvsRequired)
                {
                    foreach (var item in readyToAssign)
                    {
                        if (autoAssignmentSearchViewModel.NoOfCvsRequired <= noOfCvs)
                        {
                            break;
                        }
                        noOfCvs += 1;
                        var asstn = new PhJobAssignment
                        {
                            Joid = autoAssignmentSearchViewModel.Joid,
                            Status = (byte)RecordStatus.Active,
                            AssignedTo = item.UserId,
                            CreatedBy = autoAssignmentSearchViewModel.CreatedBy,
                            ProfilesRejected = 0,
                            ProfilesUploaded = 0,
                            NoCvsrequired = 1,
                            CreatedDate = CurrentTime,
                            CvTargetDate = autoAssignmentSearchViewModel.Enddate,
                            AssignBy = 0,
                            CvTarget = 0
                        };
                        var data = phJobAssignments.Where(x => x.AssignedTo == item.UserId).FirstOrDefault();
                        if (data != null)
                        {
                            asstn.NoCvsrequired = (short)(data.NoCvsrequired + 1);
                            phJobAssignments.Remove(data);
                            phJobAssignments.Add(asstn);
                        }
                        else
                        {
                            phJobAssignments.Add(asstn);
                        }
                    }
                }

                if (phJobAssignments.Count > 0)
                {
                    foreach (var asstn in phJobAssignments)
                    {
                        dbContext.PhJobAssignmentsDayWises.Add(new PhJobAssignmentsDayWise
                        {
                            Status = (byte)RecordStatus.Active,
                            Joid = asstn.Joid,
                            AssignedTo = asstn.AssignedTo,
                            CreatedBy = asstn.CreatedBy,
                            NoCvsuploadded = asstn.ProfilesUploaded,
                            AssignBy = asstn.AssignBy,
                            NoCvsrequired = asstn.NoCvsrequired,
                            AssignmentDate = asstn.CreatedDate
                        });
                    }
                    dbContext.PhJobAssignments.AddRange(phJobAssignments);
                    await dbContext.SaveChangesAsync();
                    response = true;
                }
            }
            return response;
        }
        internal void PhJobAssignmentsDayWise_records(ref PhJobAssignment jbAsgmnt, DateTime assignmentDate,
             short? incrementCvsrequired = null, short? updateCvsrequired = null,
             short? incrementCvsuploaded = null, short? updateCvsuploaded = null,
             short? incrementFinalCvsFilled = null, short? updateFinalCvsFilled = null)
        {
            assignmentDate = assignmentDate.Date;

            var Joid = jbAsgmnt.Joid;
            var AssignedTo = jbAsgmnt.AssignedTo;
            var fmDt = jbAsgmnt.CreatedDate.Date;
            var dbModelObjs = dbContext.PhJobAssignmentsDayWises.Where(da => (fmDt <= da.AssignmentDate.Value.Date) && da.Joid == Joid && da.AssignedTo == AssignedTo && da.Status == (byte)RecordStatus.Active).ToList();
            var dbModelObj = dbModelObjs.FirstOrDefault(da => da.AssignmentDate == assignmentDate);
            if (dbModelObj == null)
            {
                dbModelObj = new PhJobAssignmentsDayWise
                {
                    Status = (byte)RecordStatus.Active,
                    Joid = Joid,
                    AssignedTo = AssignedTo,
                    CreatedBy = jbAsgmnt.UpdatedBy ?? jbAsgmnt.CreatedBy,
                    AssignBy = jbAsgmnt.AssignBy,
                    AssignmentDate = assignmentDate
                };
                dbContext.SaveChanges();
                dbModelObjs.Add(dbModelObj);
            }
            dbModelObj.UpdatedBy = jbAsgmnt.UpdatedBy ?? jbAsgmnt.CreatedBy;
            dbModelObj.UpdatedDate = CurrentTime;

            jbAsgmnt.CvTargetDate = dbModelObjs.Max(da => da.AssignmentDate);

            int? incrementedCvValue = 0;
            if (updateCvsrequired.HasValue || incrementCvsrequired.HasValue)
            {
                var OldVal = (dbModelObj.NoCvsrequired ?? 0);
                dbModelObj.NoCvsrequired = OldVal;
                if (updateCvsrequired.HasValue)
                    dbModelObj.NoCvsrequired = updateCvsrequired;
                else if (incrementCvsrequired.HasValue)
                    dbModelObj.NoCvsrequired += incrementCvsrequired;
                incrementedCvValue = (dbModelObj.NoCvsrequired - OldVal);
                jbAsgmnt.NoCvsrequired = (short?)dbModelObjs.Where(da => da.NoCvsrequired > 0).Sum(da => da.NoCvsrequired);
            }
            if (updateCvsuploaded.HasValue || incrementCvsuploaded.HasValue)
            {
                if (updateCvsuploaded.HasValue)
                    dbModelObj.NoCvsuploadded = updateCvsuploaded;
                else if (incrementCvsuploaded.HasValue)
                    dbModelObj.NoCvsuploadded += incrementCvsuploaded;
                jbAsgmnt.ProfilesUploaded = (short?)dbModelObjs.Where(da => da.NoCvsuploadded > 0).Sum(da => da.NoCvsuploadded);
            }
            if (updateFinalCvsFilled.HasValue || incrementFinalCvsFilled.HasValue)
            {
                if (updateFinalCvsFilled.HasValue)
                    dbModelObj.NoOfFinalCvsFilled = updateFinalCvsFilled;
                else if (incrementFinalCvsFilled.HasValue)
                    dbModelObj.NoOfFinalCvsFilled += incrementFinalCvsFilled;
                jbAsgmnt.NoOfFinalCvsFilled = (short?)dbModelObjs.Where(da => da.NoOfFinalCvsFilled > 0).Sum(da => da.NoOfFinalCvsFilled);
            }
            if (dbModelObj.Id > 0)
                dbContext.PhJobAssignmentsDayWises.Update(dbModelObj);
            else
            {
                dbContext.PhJobAssignmentsDayWises.Add(dbModelObj);
                dbContext.SaveChanges();
            }
            if (incrementedCvValue.HasValue && incrementedCvValue != 0)
                dbContext.PhJobAssignmentsDayWisesLogs.Add(new PhJobAssignmentsDayWiseLog
                {
                    CreatedBy = dbModelObj.UpdatedBy,
                    CreatedDate = CurrentTime,
                    JobAssignmentDayWiseId = dbModelObj.Id,
                    NoCvsrequired = (short)(incrementedCvValue > 0 ? incrementedCvValue : (incrementedCvValue * -1)),
                    Status = (byte)RecordStatus.Active,
                    LogType = (byte)(incrementedCvValue > 0 ? RecruiterJobAssignmentLogType.ManualCvIncrement : RecruiterJobAssignmentLogType.ManualCvDecrement)
                });
        }

        public bool CheckAlternativeWeekend(DateTime curDate, List<SubClassWeekend> subClassWeekend)
        {
            try
            {
                string[] dayWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                int dayNumber = (int)curDate.DayOfWeek;

                foreach (var item in subClassWeekend)
                {
                    int weekendDayNumber = Array.IndexOf(dayWeek, item.DayName.Trim());
                    if (weekendDayNumber == dayNumber)
                    {
                        var weekNumberInaMonth = AllDatesInMonth(curDate.Year, curDate.Month).Where(i => (int)i.DayOfWeek == dayNumber).ToArray();


                        int weekNumberInaMonthIndex = Array.IndexOf(weekNumberInaMonth, curDate) + 1;

                        if (item.WeekendModel == 2)
                        {
                            if (weekNumberInaMonthIndex == 1 || weekNumberInaMonthIndex == 3)
                            {
                                return false;

                            }
                            else
                            {
                                return true;
                            }
                        }
                        else if (item.WeekendModel == 1)
                        {
                            if (weekNumberInaMonthIndex == 2 || weekNumberInaMonthIndex == 4)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static IEnumerable<DateTime> AllDatesInMonth(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= days; day++)
            {
                yield return new DateTime(year, month, day);
            }
        }

        public int ConvertToDayNumber(string dayName)
        {
            int DayNumber = 0;
            switch (dayName)
            {
                case "Monday":
                    DayNumber = 1;
                    break;
                case "Tuesday":
                    DayNumber = 2;
                    break;
                case "Wednesday":
                    DayNumber = 3;
                    break;
                case "Thursday":
                    DayNumber = 4;
                    break;
                case "Friday":
                    DayNumber = 5;
                    break;
                case "Saturday":
                    DayNumber = 6;
                    break;
                case "Sunday":
                    DayNumber = 7;
                    break;
            }
            return DayNumber;
        }

        #endregion

        #region Date Cal

        public DateTime getWeekEnd(DateTime currentTime, bool isCheck = true)
        {
            var weekEndDate = currentTime.Date;
            while (weekEndDate.DayOfWeek != DayOfWeek.Friday && (weekEndDate < currentTime || isCheck == false))
                weekEndDate = weekEndDate.AddDays(1);
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            return weekEndDate;
        }
        public DateTime getMonthEnd(DateTime currentTime, bool isCheck = true)
        {
            var dt = new DateTime(currentTime.Year, currentTime.Month, DateTime.DaysInMonth(currentTime.Year, currentTime.Month), 23, 59, 59);
            if (dt.Date > CurrentTime && isCheck)
            {
                dt = CurrentTime.Date.AddDays(1).AddSeconds(-1);
            }
            return dt;
        }
        public (int strt, int end) findQuarter(DateTime date)
        {
            int[] qtrMnth = new int[] { 3, 6, 9, 12 };
            int strt, end, indx = 0;
            do
            {
                strt = qtrMnth[indx] - 2;
                end = qtrMnth[indx];
                indx++;
            } while (date.Month > end);

            //int strt = (int)(Math.Floor((decimal)date.Month / 3) * 3);
            //int end = (int)((Math.Ceiling((decimal)date.Month / 3) + 1) * 3);
            return (strt, end);
        }
        public (DateTime? fmDt, DateTime? toDt) calcDate(dashboardDateFilter dateFilter, bool isMaxCurrent = false)
        {
            DateTime? fmDt = null;
            DateTime? toDt = null;
            switch (dateFilter)
            {
                case dashboardDateFilter.Today:
                    fmDt = new DateTime(CurrentTime.Year, CurrentTime.Month, CurrentTime.Day, 0, 0, 0, 0);
                    toDt = new DateTime(CurrentTime.Year, CurrentTime.Month, CurrentTime.Day, 23, 59, 59);
                    break;
                case dashboardDateFilter.Tomorrow:
                    var tomorrowDate = CurrentTime.AddDays(1);
                    fmDt = new DateTime(tomorrowDate.Year, tomorrowDate.Month, tomorrowDate.Day, 0, 0, 0, 0);
                    toDt = new DateTime(tomorrowDate.Year, tomorrowDate.Month, tomorrowDate.Day, 23, 59, 59);
                    break;
                case dashboardDateFilter.Yesterday:
                    var newDate = CurrentTime.AddDays(-1);
                    fmDt = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 0, 0);
                    toDt = new DateTime(newDate.Year, newDate.Month, newDate.Day, 23, 59, 59);
                    break;
                case dashboardDateFilter.ThisMonth:
                    fmDt = new DateTime(CurrentTime.Year, CurrentTime.Month, 1, 0, 0, 0, 0);
                    toDt = getMonthEnd(fmDt.Value);
                    break;
                case dashboardDateFilter.LastMonth:
                    var CurrentDt = CurrentTime.AddMonths(-1);
                    fmDt = new DateTime(CurrentDt.Year, CurrentDt.Month, 1, 0, 0, 0, 0);
                    toDt = new DateTime(CurrentDt.Year, CurrentDt.Month, DateTime.DaysInMonth(CurrentDt.Year, CurrentDt.Month), 23, 59, 59);
                    break;
                case dashboardDateFilter.ThisQuarter:
                    var qtr = findQuarter(CurrentTime);
                    fmDt = new DateTime(CurrentTime.Year, qtr.strt, 1, 0, 0, 0, 0);
                    toDt = new DateTime(CurrentTime.Year, qtr.end, DateTime.DaysInMonth(CurrentTime.Year, qtr.end), 23, 59, 59);
                    break;
                case dashboardDateFilter.LastQuarter:
                    var lstQtr = findQuarter(CurrentTime.AddMonths(-3));
                    fmDt = new DateTime(CurrentTime.AddMonths(-3).Year, lstQtr.strt, 1, 0, 0, 0, 0);
                    toDt = new DateTime(CurrentTime.AddMonths(-3).Year, lstQtr.end, DateTime.DaysInMonth(CurrentTime.AddMonths(-3).Year, lstQtr.end), 23, 59, 59);
                    break;
                case dashboardDateFilter.ThisWeek:
                    DayOfWeek currentDay = CurrentTime.DayOfWeek;
                    int daysTillCurrentDay = currentDay - DayOfWeek.Sunday;
                    fmDt = CurrentTime.AddDays(-daysTillCurrentDay).Date;
                    toDt = CurrentTime.Date.AddDays(1).AddSeconds(-1);
                    break;
                case dashboardDateFilter.LastWeek:
                    int days = CurrentTime.DayOfWeek - DayOfWeek.Sunday;
                    fmDt = CurrentTime.AddDays(-(days + 7)).Date;
                    toDt = fmDt.Value.AddDays(7).AddSeconds(-1);
                    break;
                default:
                    break;
            }
            if (isMaxCurrent)
            {
                var mxDt = CurrentTime.AddDays(1).Date;
                if (toDt.HasValue)
                    while (toDt > mxDt)
                    {
                        toDt = toDt.Value.AddDays(-1);
                    }
            }
            return (fmDt, toDt);
        }

        public (DateTime? fmDt, DateTime? toDt) calcPrevDate(dashboardDateFilter dateFilter)
        {
            DateTime? fmDt = null;
            DateTime? toDt = null;
            switch (dateFilter)
            {
                case dashboardDateFilter.Today:
                    {
                        var CurrentDt = CurrentTime.AddDays(-1);
                        fmDt = new DateTime(CurrentDt.Year, CurrentDt.Month, 1);
                        toDt = new DateTime(CurrentDt.Year, CurrentDt.Month, CurrentTime.Day, 23, 59, 59);
                    }
                    break;
                case dashboardDateFilter.ThisMonth:
                    {
                        var CurrentDt = CurrentTime.AddMonths(-1);
                        fmDt = new DateTime(CurrentDt.Year, CurrentDt.Month, 1);
                        toDt = new DateTime(CurrentDt.Year, CurrentDt.Month, DateTime.DaysInMonth(CurrentDt.Year, CurrentDt.Month), 23, 59, 59);
                    }
                    break;
                case dashboardDateFilter.LastMonth:
                    {
                        var CurrentDt = CurrentTime.AddMonths(-2);
                        fmDt = new DateTime(CurrentDt.Year, CurrentDt.Month, 1);
                        toDt = new DateTime(CurrentDt.Year, CurrentDt.Month, DateTime.DaysInMonth(CurrentDt.Year, CurrentDt.Month), 23, 59, 59);
                    }
                    break;
                case dashboardDateFilter.ThisQuarter:
                    {
                        var qtr = findQuarter(CurrentTime.AddMonths(-3));
                        fmDt = new DateTime(CurrentTime.Year, qtr.strt, 1);
                        toDt = new DateTime(CurrentTime.Year, qtr.end, DateTime.DaysInMonth(CurrentTime.Year, qtr.end), 23, 59, 59);
                    }
                    break;
                case dashboardDateFilter.LastQuarter:
                    {
                        var lstQtr = findQuarter(CurrentTime.AddMonths(-6));
                        fmDt = new DateTime(CurrentTime.Year, lstQtr.strt, 1);
                        toDt = new DateTime(CurrentTime.Year, lstQtr.end, DateTime.DaysInMonth(CurrentTime.Year, lstQtr.end), 23, 59, 59);
                    }
                    break;
                default:
                    break;
            }
            return (fmDt, toDt);
        }

        #endregion

        #region Time Zone
        private int? timeZoneHrs = null, timeZoneMins = null;
        void setTimeZone(string adjustmentTime, bool? daylightTime)
        {
            var timezone = GetTimeZoneHrsMins(adjustmentTime, daylightTime);
            timeZoneHrs = timezone.timeZoneHrs;
            timeZoneMins = timezone.timeZoneMins;
        }
        /// <summary>
        /// Get timezone hours and mins
        /// </summary>
        /// <param name="adjustmentTime"></param>
        /// <param name="daylightTime"></param>
        /// <returns></returns>
        internal (int? timeZoneHrs, int? timeZoneMins) GetTimeZoneHrsMins(string adjustmentTime, bool? daylightTime)
        {
            var data = adjustmentTime.ToString().Split('-');
            bool? isNegative = null;
            int addHrs = 0, addMnts = 0;
            if (data.Length == 2)
            {
                isNegative = true;
            }
            else
            {
                data = adjustmentTime.ToString().Split('+');
                if (data.Length == 2)
                {
                    isNegative = false;
                }
            }
            if (isNegative.HasValue)
            {
                int.TryParse(data[1].Split(':')[0], out addHrs);
                int.TryParse(data[1].Split(':')[1], out addMnts);
                if (isNegative.Value)
                {
                    addHrs *= -1;
                    addMnts *= -1;
                }
                if ((daylightTime == true))
                {
                    addHrs += 1;
                }
            }
            return (addHrs, addMnts);
        }

        /// <summary>
        /// Converts UTC time to user time zone
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        internal DateTime? ToUserDateTime(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return ToUserDateTime(dateTime.Value);
            }
            else return dateTime;
        }


        /// <summary>
        /// Convert UTC time to input time zone adjustment
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="adjustmentTime"></param>
        /// <param name="daylightTime"></param>
        /// <returns></returns>
        internal DateTime ConvertUTCToDateByTimezone(DateTime dateTime, string adjustmentTime, bool? daylightTime)
        {
            if (dateTime == null || adjustmentTime == null) return dateTime;
            var hrsMins = GetTimeZoneHrsMins(adjustmentTime, daylightTime);
            var uDateTime = dateTime.AddHours(hrsMins.timeZoneHrs.Value).AddMinutes(hrsMins.timeZoneMins.Value);
            DateTime dt = Convert.ToDateTime(uDateTime);
            return dt;
        }
        /// <summary>
        /// Convert UTC time to input time zone adjustment
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="adjustmentTime"></param>
        /// <param name="daylightTime"></param>
        /// <returns></returns>
        internal DateTime? ConvertUTCToDateByTimezone(DateTime? dateTime, string adjustmentTime, bool? daylightTime)
        {
            if (dateTime.HasValue)
            {
                return ConvertUTCToDateByTimezone(dateTime.Value, adjustmentTime, daylightTime);
            }
            else return dateTime;
        }
        /// <summary>
        /// Convert datetime from user time zone 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        internal DateTime? FromUserDateTime(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return FromUserDateTime(dateTime.Value);
            }
            else return dateTime;
        }

        internal DateTime? ConvertDateToUTCByTimezone(DateTime? dateTime, string adjustmentTime, bool? daylightTime)
        {
            if (dateTime.HasValue)
            {
                return ConvertDateToUTCByTimezone(dateTime.Value, adjustmentTime, daylightTime);
            }
            else return dateTime;
        }


        /// <summary>
        /// Converts datetime to UTC time based on timezone parameter
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="adjustmentTime"></param>
        /// <param name="daylightTime"></param>
        /// <returns></returns>
        internal DateTime ConvertDateToUTCByTimezone(DateTime dateTime, string adjustmentTime, bool? daylightTime)
        {
            if (dateTime == null || adjustmentTime == null) return dateTime;
            var hrsMins = GetTimeZoneHrsMins(adjustmentTime, daylightTime);
            var dt = dateTime.AddHours(hrsMins.timeZoneHrs.Value * -1).AddMinutes(hrsMins.timeZoneMins.Value * -1);
            return dt;
        }
        #endregion
    }

    public enum ApiResponseErrorCodes
    {
        InvalidJsonFormat = 1001,
        InvalidBodyContent = 1002,
        InvalidUrlParameter = 1003,

        TokenInvalid = 1010,
        TokenExpired = 1011,

        UserPermissionNotGranted = 1021,
        UserPermissionNoResrcAccess = 1022,
        UserPermissionResrcAccessDisabled = 1023,

        ResourceDoesNotExist = 1041,    //The resource that you're trying to access doesn't exist
        ResourceAlreadyExist = 1051,    //The resource trying to create already exist

        //1100	Custom Error Codes starts from 1100
        Exception = 1100,
        ApiLimtExhausted = 1101,
        LimtExhausted = 1102
    }

    public enum ApipResponseHttpCodes
    {
        OK = 200,                    //Request is successful
        Accepted = 202,              //Request accepted but it is queued or processing
        BadRequest = 400,            //Request has missing required parameters or validation errors
        Unauthorized = 401,          //Bad Token or missing token
        Forbidden = 403,             //Access denied for the requested resource
        NotFound = 404,              //The requested resource does not exist
        Conflict = 409,              //Request conflicts with another, trying to create already existing resource
        TooManyRequests = 429,       //Api request limit exceeded
        InternalServerError = 500    //Something went wrong in piHire
    }

}

