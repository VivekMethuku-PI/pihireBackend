using System.Collections.Generic;

namespace PiHire.Utilities.ViewModels.Communications.Emails
{
    public class InfoBip__EmailSupport_RequestModel
    {
        public string MailSubject { get; set; }
        public string MailBody { get; set; }
        public string ReplyTo { get; set; } = "";
    }
    public class InfoBip_EmailSupport_DistributionRequestModel : InfoBip__EmailSupport_RequestModel
    {
        public List<InfoBip_EmailContactViewModel> ToEmails { get; set; }
        public bool ConfigStatus { get; set; }
    }
    public class InfoBip_EmailContactViewModel
    {
        public string To { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }
    }
    //public class InfoBip_EmailSupport_RequestModel : InfoBip__EmailSupport_RequestModel
    //{
    //    public string ToName { get; set; } = "";
    //    public List<InfoBip_EmailContactViewModel> ToEmails { get; set; }
    //    public string CCEmails { get; set; } = "";
    //    public bool configStatus { get; set; }
    //}

    #region sendGrid
    public class SendGridRequest
    {
        public List<SendGridRequest_Personalization> personalizations { get; set; }
        public SendGridRequest_Address from { get; set; }
        public SendGridRequest_Address reply_to { get; set; }
        public List<SendGridRequest_Content> content { get; set; }
        public SendGridRequest_MailSettings mail_settings { get; set; }
    }
    public class SendGridRequest_Personalization
    {
        public List<SendGridRequest_Address> to { get; set; }
        public List<SendGridRequest_Address> cc { get; set; }
        public List<SendGridRequest_Address> bcc { get; set; }
        public Dictionary<string, string> substitutions { get; set; }
        public string subject { get; set; }
    }
    public class SendGridRequest_Address
    {
        public string email { get; set; }
        public string name { get; set; }
    }
    public class SendGridRequest_Content
    {
        public string type { get; set; } = "text/html";
        public string value { get; set; }
    }
    public class SendGridRequest_MailSettings
    {
        public SendGridRequest_SandboxMode sandbox_mode { get; set; }
    }
    public class SendGridRequest_SandboxMode
    {
        public bool enable { get; set; } = false;
    }
    #endregion
}
