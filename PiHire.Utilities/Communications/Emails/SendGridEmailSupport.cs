using PiHire.Utilities.ViewModels.Communications.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.Utilities.Communications.Emails
{
    internal class SendGridEmailSupport : IDisposable
    {
        #region dispose
        bool disposed = false;
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            disposed = true;
        }
        ~SendGridEmailSupport()
        {
            Dispose(false);
        }
        #endregion
        internal async Task<(bool status, List<string> message)> SendEmailAsync(ProviderAuthViewModel auth, SendEmailRequestViewModel model)
        {
            var url = "v3/mail/send";
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(auth.ProviderUrl)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AuthKey);
            #region content
            SendGridRequest request = new SendGridRequest()
            {
                from = new SendGridRequest_Address { email = auth.FromEmail, name = auth.FromName },
                personalizations = new List<SendGridRequest_Personalization>(),
                content = new List<SendGridRequest_Content> { new SendGridRequest_Content { value = model.MailBody } }
            };
            foreach (var to in model.ToEmails)
            {
                request.personalizations.Add(new SendGridRequest_Personalization
                {
                    to = new List<SendGridRequest_Address> { new SendGridRequest_Address { email = to.Address, name = to.Name } },
                    subject = model.MailSubject,
                    substitutions = to.Placeholders ?? new Dictionary<string, string>()
                });
            }
            if (!string.IsNullOrEmpty(model.ReplyTo))
            {
                request.reply_to = new SendGridRequest_Address { email = model.ReplyTo };
            }
            if (model.CCEmails != null)
            {
                var ccEMails = model.CCEmails;
                if (ccEMails.Length > 0) request.personalizations[0].cc = new List<SendGridRequest_Address>();
                for (int i = 0; i < ccEMails.Length; i++)
                {
                    request.personalizations[0].cc.Add(new SendGridRequest_Address { email = ccEMails[i] });
                }
            }
            #endregion
            var dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(dataStr, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, httpContent);
            HttpContent responseContent = response.Content;
            var responseString = await responseContent.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseString);
            }
            //X-Message-Id
            var X_Message_Ids = response.Headers.Where(da => da.Key == "X-Message-Id").Select(da => da.Value).ToList();
            //return response.IsSuccessStatusCode&& response.StatusCode== System.Net.HttpStatusCode.Accepted;
            var ids = new List<string>();
            foreach (var item in X_Message_Ids)
            {
                ids.AddRange(item);
            }
            return (response.IsSuccessStatusCode, ids);
        }
    }
}
