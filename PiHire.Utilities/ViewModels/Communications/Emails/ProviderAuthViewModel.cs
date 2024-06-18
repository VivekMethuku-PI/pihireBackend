namespace PiHire.Utilities.ViewModels.Communications.Emails
{
    public class ProviderAuthViewModel
    {
        public ProviderAuthViewModel(string providerUrl, string notifyUrl, string authKey, string fromName, string fromEmail, EmailProviders provider, int? emailSetCount = null)
        {
            ProviderUrl = providerUrl;
            AuthKey = authKey;
            FromName = fromName;
            FromEmail = fromEmail;
            Provider = provider;
            NotifyUrl = notifyUrl;
            EmailSetCount = emailSetCount;
        }

        public string ProviderUrl { get; set; }
        public string AuthKey { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }

        public int? EmailSetCount { get; set; }

        public EmailProviders Provider { get; set; } = EmailProviders.InfoBip;
        public string NotifyUrl { get; set; } = "https://qapihireapi1o.paraminfo.online/api/v1/MailSupport/InfoBip/Webhook";

        internal bool Validate()
        {
            switch (Provider)
            {
                case EmailProviders.InfoBip:
                    return string.IsNullOrEmpty(ProviderUrl) == false &&
                        string.IsNullOrEmpty(AuthKey) == false &&
                        string.IsNullOrEmpty(ProviderUrl) == false &&
                        string.IsNullOrEmpty(FromEmail) == false &&
                        string.IsNullOrEmpty(NotifyUrl) == false;
                case EmailProviders.SendGrid:
                    return string.IsNullOrEmpty(ProviderUrl) == false &&
                        string.IsNullOrEmpty(AuthKey) == false &&
                        string.IsNullOrEmpty(ProviderUrl) == false &&
                        string.IsNullOrEmpty(FromEmail) == false;
                case EmailProviders.MailChimp:
                    return string.IsNullOrEmpty(ProviderUrl) == false &&
                        string.IsNullOrEmpty(AuthKey) == false &&
                        string.IsNullOrEmpty(ProviderUrl) == false &&
                        string.IsNullOrEmpty(FromEmail) == false;
                default:
                    break;
            }
            return false;
        }
    }
}
