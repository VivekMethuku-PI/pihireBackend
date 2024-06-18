using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.Utilities.Communications.Emails
{
    public class InfoBipStatus
    {
        public static byte[] FinalStatus
        {
            get
            {
                return finalStatus.Select(da => (byte)da).ToArray();
            }
        }
        static InfoBip_DeliveryStatus[] finalStatus = new InfoBip_DeliveryStatus[] {
                    InfoBip_DeliveryStatus.DELIVERED, InfoBip_DeliveryStatus.NotAvailable, InfoBip_DeliveryStatus.UNDELIVERABLE,
                    InfoBip_DeliveryStatus.EXPIRED, InfoBip_DeliveryStatus.REJECTED, InfoBip_DeliveryStatus.EC_HARD_BOUNCE
                };
        static Dictionary<InfoBip_DeliveryStatus, MailDeliveryStatus> Convert_DistContDistStatus = new Dictionary<InfoBip_DeliveryStatus, MailDeliveryStatus>
        {
            //{ InfoBip_DeliveryStatus.DELIVERED, null },
            //{ InfoBip_DeliveryStatus.NotAvailable, null },
            { InfoBip_DeliveryStatus.UNDELIVERABLE,MailDeliveryStatus.Bounced },
            { InfoBip_DeliveryStatus.EXPIRED,MailDeliveryStatus.Bounced },
            { InfoBip_DeliveryStatus.REJECTED,MailDeliveryStatus.Blocked },
            { InfoBip_DeliveryStatus.EC_HARD_BOUNCE, MailDeliveryStatus.Bounced }
        };
        public static MailDeliveryStatus? ConvertToMailDeliveryStatus(InfoBip_DeliveryStatus status)
        {
            return Convert_DistContDistStatus.ContainsKey(status) ? Convert_DistContDistStatus[status] : (MailDeliveryStatus?)null;
        }
        private static string InfobipAuthToken, InfobipUrl;
        public InfoBipStatus(string InfobipUrl, string InfobipAuthToken)
        {
            if (string.IsNullOrEmpty(InfoBipStatus.InfobipUrl)) InfoBipStatus.InfobipUrl = InfobipUrl;
            if (string.IsNullOrEmpty(InfoBipStatus.InfobipAuthToken)) InfoBipStatus.InfobipAuthToken = InfobipAuthToken;
        }
        /// <summary>
        /// InfoBip Mail status (available only for 48 hrs)
        /// </summary>
        /// <param name="InfobipBulkId"></param>
        /// <param name="InfobipMsgId"></param>
        /// <param name="logSize"></param>
        /// <returns></returns>
        public async Task<List<(InfoBip_DeliveryStatus status, string GroupName, string name, string description, string MessageId)>> LogStatus(string InfobipBulkId, string InfobipMsgId = null, int? logSize = null)
        {
            List<(InfoBip_DeliveryStatus status, string GroupName, string name, string description, string MessageId)> resp = new List<(InfoBip_DeliveryStatus status, string GroupName, string name, string description, string MessageId)>();
            InfoBipStatusClasses.Response msg = null;
            #region Getting status
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(InfobipUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", InfobipAuthToken);

                var rsp = await client.GetAsync("/email/1/logs?" + (string.IsNullOrEmpty(InfobipBulkId) ? "messageId=" + InfobipMsgId : "bulkId=" + InfobipBulkId) + (logSize.HasValue ? "&limit=" + logSize : ""));//bbnmnemun2j666r3hi2j
                if (rsp.IsSuccessStatusCode)
                {
                    var rspStr = await rsp.Content.ReadAsStringAsync();
                    msg = Newtonsoft.Json.JsonConvert.DeserializeObject<InfoBipStatusClasses.Response>(rspStr);
                }
            }
            #endregion
            if (msg != null)
            {
                foreach (var tm in msg.results)
                {
                    InfoBip_DeliveryStatus status = InfoBip_DeliveryStatus.NotStarted;
                    string GroupName = "";
                    string name = "";
                    string description = "";
                    if (tm?.status != null)
                    {
                        GroupName = tm.status.groupName;
                        name = tm.status.name;
                        description = tm.status.description;
                        status = FindStatus(GroupName, name);
                    }
                    else
                    {
                        status = InfoBip_DeliveryStatus.NotAvailable;
                    }
                    (InfoBip_DeliveryStatus status, string GroupName, string name, string description, string MessageId) rsp = (status, GroupName, name, description, tm.messageId);
                    resp.Add(rsp);
                }
            }
            return resp;
        }
        private InfoBip_DeliveryStatus FindStatus(string groupName, string name)
        {
            InfoBip_DeliveryStatus ret;
            if (!Enum.TryParse<InfoBip_DeliveryStatus>(groupName, out ret))
                ret = InfoBip_DeliveryStatus.Others;

            if (string.Equals(name, "EC_HARD_BOUNCE", StringComparison.InvariantCultureIgnoreCase)) ret = InfoBip_DeliveryStatus.EC_HARD_BOUNCE;

            return ret;
        }
        public async Task<List<(InfoBip_DeliveryStatus status, string GroupName, string name, string description, string MessageId)>> Report()
        {
            List<(InfoBip_DeliveryStatus status, string GroupName, string name, string description, string MessageId)> resp = new List<(InfoBip_DeliveryStatus status, string GroupName, string name, string description, string MessageId)>();
            InfoBipStatusClasses.Response msg = null;
            #region Getting status
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(InfobipUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", InfobipAuthToken);

                var rsp = await client.GetAsync("/email/1/reports");//bbnmnemun2j666r3hi2j
                if (rsp.IsSuccessStatusCode)
                {
                    var rspStr = await rsp.Content.ReadAsStringAsync();
                    msg = Newtonsoft.Json.JsonConvert.DeserializeObject<InfoBipStatusClasses.Response>(rspStr);
                }
            }
            #endregion
            if (msg != null)
            {
                foreach (var tm in msg.results)
                {
                    InfoBip_DeliveryStatus status = InfoBip_DeliveryStatus.NotStarted;
                    string GroupName = "";
                    string name = "";
                    string description = "";
                    if (tm?.status != null)
                    {
                        GroupName = tm.status.groupName;
                        name = tm.status.name;
                        description = tm.status.description;
                        status = FindStatus(GroupName, name);
                    }
                    else
                    {
                        status = InfoBip_DeliveryStatus.NotAvailable;
                    }
                    (InfoBip_DeliveryStatus status, string GroupName, string name, string description, string MessageId) rsp = (status, GroupName, name, description, tm.messageId);
                    resp.Add(rsp);
                }
            }
            return resp;
        }
    }
}
namespace PiHire.Utilities.Communications.Emails.InfoBipStatusClasses
{
    public class Price
    {
        public double pricePerMessage { get; set; }
        public string currency { get; set; }
    }

    public class Status
    {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Result
    {
        public string bulkId { get; set; }
        public string messageId { get; set; }
        public string to { get; set; }
        public string from { get; set; }
        public string text { get; set; }
        public DateTime sentAt { get; set; }
        public DateTime doneAt { get; set; }
        public int messageCount { get; set; }
        public Price price { get; set; }
        public Status status { get; set; }
        public string channel { get; set; }
    }

    public class Response
    {
        public List<Result> results { get; set; }
    }
}
