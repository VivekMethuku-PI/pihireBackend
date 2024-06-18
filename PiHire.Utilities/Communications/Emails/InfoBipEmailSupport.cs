using PiHire.Utilities.ViewModels.Communications.Emails;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.Utilities.Communications.Emails.InfoBip
{
    internal class InfoBipEmailSupport : IDisposable
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
        ~InfoBipEmailSupport()
        {
            Dispose(false);
        }
        #endregion
        internal async Task<(bool status, InfobipResponse data)> SendEmailAsync(ProviderAuthViewModel auth, SendEmailRequestViewModel sendEmailRequestViewModel)
        {
            try
            {
                string responseString = string.Empty;
                var iUrl = "email/1/send";
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(auth.ProviderUrl)
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth.AuthKey);
                var request = new MultipartFormDataContent
            {
                { new StringContent(auth.FromName+" <"+ auth.FromEmail+">"), "from" },
                { new StringContent(sendEmailRequestViewModel.MailSubject), "subject" },
                { new StringContent(sendEmailRequestViewModel.MailBody), "html" },
                { new StringContent("true"), "intermediateReport" },
                { new StringContent(auth.NotifyUrl), "notifyUrl" },
                { new StringContent("application/json"), "notifyContentType" },
                { new StringContent("balaji callback data"), "callbackData" }
            };
                foreach (var toDtls in sendEmailRequestViewModel.ToEmails)
                {
                    var toObj = new { to = toDtls.Address, placeholders = toDtls.Placeholders };
                    string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(toObj);
                    request.Add(new StringContent(jsonStr), "to");
                }
                if (!string.IsNullOrEmpty(sendEmailRequestViewModel.ReplyTo))
                {
                    request.Add(new StringContent(sendEmailRequestViewModel.ReplyTo), "replyto");
                }
                if (sendEmailRequestViewModel.CCEmails != null)
                {
                    for (int i = 0; i < sendEmailRequestViewModel.CCEmails.Length; i++)
                    {
                        iUrl = "email/2/send";
                        request.Add(new StringContent(sendEmailRequestViewModel.CCEmails[i]), "cc");
                    }
                }
                HttpResponseMessage response = await client.PostAsync(iUrl, request);
                HttpContent responseContent = response.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(responseString);
                }
                return (response.IsSuccessStatusCode, Newtonsoft.Json.JsonConvert.DeserializeObject<InfobipResponse>(responseString));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        internal async Task<InfobipResponse> SendDistributionEmailsAsync(ProviderAuthViewModel auth, InfoBip_EmailSupport_DistributionRequestModel model)
        {
            string responseString = string.Empty;
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(auth.ProviderUrl)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth.AuthKey);
            var request = new MultipartFormDataContent
            {
                { new StringContent(auth.FromName+" <"+ auth.FromEmail+">"), "from" },
                { new StringContent(model.MailSubject), "subject" },
                { new StringContent(model.MailBody), "html" }
            };
            foreach (var toDtls in model.ToEmails)
            {
                var toObj = new { to = toDtls.To, placeholders = toDtls.Placeholders };
                string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(toObj);
                request.Add(new StringContent(jsonStr), "to");
            }
            if (!string.IsNullOrEmpty(model.ReplyTo))
            {
                request.Add(new StringContent(model.ReplyTo), "replyto");
            }
            HttpResponseMessage response = await client.PostAsync("email/1/send", request);
            HttpContent responseContent = response.Content;
            responseString = await responseContent.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseString);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<InfobipResponse>(responseString);
        }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Price
    {
        public int pricePerMessage { get; set; }
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

    public class Error
    {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool permanent { get; set; }
    }

    public class Result
    {
        public string bulkId { get; set; }
        public string messageId { get; set; }
        public string to { get; set; }
        public DateTime sentAt { get; set; }
        public DateTime doneAt { get; set; }
        public int messageCount { get; set; }
        public Price price { get; set; }
        public Status status { get; set; }
        public Error error { get; set; }
        public string channel { get; set; }
    }

    public class InfoBipNotifyReport
    {
        public List<Result> results { get; set; }
    }
}
