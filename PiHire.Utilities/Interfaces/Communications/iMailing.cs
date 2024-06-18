using PiHire.Utilities.ViewModels.Communications.Emails;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PiHire.Utilities.Interfaces.Communications
{
    public interface iMailing : iBase
    {
        int emailSetCount { get; }
        ProviderAuthViewModel ProviderAuth { get; }
        #region send Email
        Task<SendEmailResponseViewModel> SendEmailAsync(SendEmailRequestViewModel sendEmailRequestViewModel);
        Task<List<SendEmailResponseViewModel>> SendBulkEmailsAsync(SendEmailRequestViewModel sendEmailRequestViewModel);
        #endregion
    }
}
