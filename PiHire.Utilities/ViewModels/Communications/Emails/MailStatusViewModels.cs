using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.Utilities.ViewModels.Communications.Emails
{
    public class SendGrid_WebhookViewModel
    {
        public string email { get; set; }
        public int timestamp { get; set; }
        //[DisplayName("smtp-id")]
        [JsonProperty("smtp-id")]
        public string smtp_id { get; set; }
        //[DisplayName("event")]
        [JsonProperty("event")]
        public string event_ { get; set; }
        public string category { get; set; }
        public string sg_event_id { get; set; }
        public string sg_message_id { get; set; }
        public string response { get; set; }
        public string attempt { get; set; }
        public string useragent { get; set; }
        public string ip { get; set; }
        public string url { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
        public int? asm_group_id { get; set; }
        public string type { get; set; }

        public static string GetEventValue(SendGrid_WebhookViewModel rptRslt)
        {
            var details = Newtonsoft.Json.Linq.JObject.Parse(JsonConvert.SerializeObject(rptRslt));
            var prp = details["event"];
            return prp.ToString();
        }
        public static MailDeliveryStatus? ConvertToMailDeliveryStatus(MailGrid_DeliveryStatus deliveryStatus)
        {
            switch (deliveryStatus)
            {
                case MailGrid_DeliveryStatus.processed:
                    return MailDeliveryStatus.NotSent;
                    break;
                case MailGrid_DeliveryStatus.dropped:
                    return MailDeliveryStatus.Blocked;
                    break;
                case MailGrid_DeliveryStatus.delivered:
                    return MailDeliveryStatus.Sent;
                    break;
                case MailGrid_DeliveryStatus.deferred:
                    return MailDeliveryStatus.Blocked;
                    break;
                case MailGrid_DeliveryStatus.bounce:
                    return MailDeliveryStatus.Bounced;
                    break;
                case MailGrid_DeliveryStatus.blocked:
                    return MailDeliveryStatus.Blocked;
                    break;
                case MailGrid_DeliveryStatus.open:
                    return MailDeliveryStatus.Opened;
                    break;
                case MailGrid_DeliveryStatus.click:
                    return MailDeliveryStatus.Clicked;
                    break;
                case MailGrid_DeliveryStatus.spamreport:
                    break;
                case MailGrid_DeliveryStatus.unsubscribe:
                    break;
                case MailGrid_DeliveryStatus.group_unsubscribe:
                    break;
                case MailGrid_DeliveryStatus.group_resubscribe:
                    break;
                default:
                    break;
            }
            return null;
        }
    }

    public class MailChimp_WebhookViewModel
    {
        public string email { get; set; }
        public int timestamp { get; set; }
        //[DisplayName("smtp-id")]
        [JsonProperty("smtp-id")]
        public string smtp_id { get; set; }
        //[DisplayName("event")]
        [JsonProperty("event")]
        public string event_ { get; set; }
        public string category { get; set; }
        public string sg_event_id { get; set; }
        public string sg_message_id { get; set; }
        public string response { get; set; }
        public string attempt { get; set; }
        public string useragent { get; set; }
        public string ip { get; set; }
        public string url { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
        public int? asm_group_id { get; set; }
        public string type { get; set; }

        public static string GetEventValue(MailChimp_WebhookViewModel rptRslt)
        {
            var details = Newtonsoft.Json.Linq.JObject.Parse(JsonConvert.SerializeObject(rptRslt));
            var prp = details["event"];
            return prp.ToString();
        }
    }
}
