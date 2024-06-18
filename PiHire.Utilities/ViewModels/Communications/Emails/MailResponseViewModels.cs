using System.Collections.Generic;

namespace PiHire.Utilities.ViewModels.Communications.Emails
{
    #region InfoBip
    public class InfobipResponse
    {
        public string bulkId { get; set; }
        public List<InfobipResponseMessage> messages { get; set; }
    }
    public class InfobipResponseMessage
    {
        public string to { get; set; }
        public int messageCount { get; set; }
        public string messageId { get; set; }
        public InfobipResponseStatus status { get; set; }
    }
    public class InfobipResponseStatus
    {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
    #endregion
    #region Mail Chimp
    public class MandrillResponseViewModel
    {
        public string email { get; set; }
        public string status { get; set; }
        public string _id { get; set; }
        public string reject_reason { get; set; }
    }
    #endregion
}
