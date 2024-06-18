using PiHire.Utilities.Communications.Emails.InfoBip;
using PiHire.Utilities.ViewModels.Communications.Emails;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IMailSupportRepository : IBaseRepository
    {
        Task SendGrid_Webhook(List<SendGrid_WebhookViewModel> model);
        Task MailChimp_Webhook(List<MailChimp_WebhookViewModel> model);
        Task InfoBip_Webhook(InfoBipNotifyReport model);
    }
}
