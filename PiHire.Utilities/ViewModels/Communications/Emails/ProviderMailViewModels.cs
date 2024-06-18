using System.Collections.Generic;
using System.Linq;

namespace PiHire.Utilities.ViewModels.Communications.Emails
{
    #region Request
    public class SendEmailRequestViewModel
    {
        public IEnumerable<ToViewModel> ToEmails { get; set; }
        public string MailSubject { get; set; }
        public string MailBody { get; set; }
        public string[] CCEmails { get; set; } = null;
        public string ReplyTo { get; set; } = string.Empty;
    }
    public class ToViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }
    }
    #endregion
    #region Response
    public class SendEmailResponseViewModel
    {
        // public string bulkId { get; set; }
        //public List<EmailSupport_ResponseModel_> messages { get; set; }
        public string ResponseJsonString { get; set; }
        public bool Status { get; set; }
        public string BulkId { get; set; }
        public List<SendEmailResponseViewModel_messages> Messages { get; set; } = new List<SendEmailResponseViewModel_messages>();

        internal static List<SendEmailResponseViewModel_messages> ToViewModel(List<InfobipResponseMessage> messages)
        {
            if (messages == null)
            {
                return new List<SendEmailResponseViewModel_messages>();
            }
            return messages.Select(da => new SendEmailResponseViewModel_messages { to = da.to, messageId = da.messageId, messageCount = da.messageCount, status = (!string.IsNullOrEmpty(da.to)), status_reason = (da?.status?.name) }).ToList();
        }

        internal static List<SendEmailResponseViewModel_messages> ToViewModel(List<MandrillResponseViewModel> messages)
        {
            if (messages == null)
            {
                return new List<SendEmailResponseViewModel_messages>();
            }
            return messages.Select(da => new SendEmailResponseViewModel_messages
            {
                to = da.email,
                messageId = da._id,
                messageCount = (da.status == "sent" || da.status == "queued" || da.status == "scheduled") &&
                        (da.status != "rejected" && da.status != "invalid") ? 1 : 0,
                status = (da.status == "sent" || da.status == "queued" || da.status == "scheduled") &&
                        (da.status != "rejected" && da.status != "invalid"),
                status_reason = da.reject_reason
            }).ToList();
        }
    }
    public class SendEmailResponseViewModel_messages
    {
        public string to { get; set; }
        public string messageId { get; set; }
        public int messageCount { get; set; }
        public bool status { get; set; }
        public string status_reason { get; set; }
    }
    #endregion
}
