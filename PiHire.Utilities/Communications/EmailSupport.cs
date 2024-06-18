using Newtonsoft.Json;
using PiHire.Utilities.ViewModels.Communications.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiHire.Utilities.Communications
{
    public class EmailSupport : Interfaces.Communications.iMailing
    {
        ProviderAuthViewModel providerAuth;
        #region destroyer
        // Flag: Has Dispose already been called?
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

        ~EmailSupport()
        {
            Dispose(false);
        }
        #endregion
        public static Interfaces.Communications.iMailing getObject(EmailProviders emailProvider, string ProviderUrl, string notifyUrl, string AuthKey, string FromName, string FromEmail, int? EmailSetCount = null)
        {
            var obj = new EmailSupport();
            obj.providerAuth = new ProviderAuthViewModel(ProviderUrl, notifyUrl, AuthKey, FromName, FromEmail, emailProvider, EmailSetCount);
            if (obj.providerAuth.Validate())
                return obj;
            else
                throw new Exception("Mandatory email provider auth details missing");
        }
        public static Interfaces.Communications.iMailing getInfobipObject(string ProviderUrl, string notifyUrl, string AuthKey, string FromName, string FromEmail, int? EmailSetCount = null)
        {
            var obj = new EmailSupport();
            obj.providerAuth = new ProviderAuthViewModel(ProviderUrl, notifyUrl, AuthKey, FromName, FromEmail, EmailProviders.InfoBip, EmailSetCount);
            if (obj.providerAuth.Validate())
                return obj;
            else
                throw new Exception("Mandatory email provider auth details missing");
        }

        public async Task<SendEmailResponseViewModel> SendEmailAsync(SendEmailRequestViewModel sendEmailRequestViewModel)
        {
            try
            {
                if (this.providerAuth?.Provider == null)
                {
                    throw new Exception("Mandatory email provider auth details missing or email provider not selected");
                }
                var emailSetCount = this.emailSetCount;
                int emailCnt = sendEmailRequestViewModel?.ToEmails?.Count() ?? 0;
                if (emailCnt > emailSetCount)
                {
                    throw new Exception("To emails are more than send email, call bulk email");
                }
                if (this.providerAuth.Provider == EmailProviders.InfoBip)
                {
                    using (Emails.InfoBip.InfoBipEmailSupport infobipEmailSupport = new Emails.InfoBip.InfoBipEmailSupport())
                    {
                        var result = await infobipEmailSupport.SendEmailAsync(providerAuth, sendEmailRequestViewModel);
                        //return new SendEmailResponseViewModel { Status = result.status, ResponseJsonString = JsonConvert.SerializeObject(result.data) };
                        var ret = new SendEmailResponseViewModel
                        {
                            Status = result.status,
                            ResponseJsonString = JsonConvert.SerializeObject(result.data),
                            BulkId = result.data.bulkId,
                            Messages = SendEmailResponseViewModel.ToViewModel(result.data.messages)
                        };
                        return ret;
                    }
                }
                else if (this.providerAuth.Provider == EmailProviders.SendGrid)
                {
                    using (Emails.SendGridEmailSupport sendGridSupport = new Emails.SendGridEmailSupport())
                    {
                        var result = await sendGridSupport.SendEmailAsync(providerAuth, sendEmailRequestViewModel);
                        //return new SendEmailResponseViewModel { Status = result.status, ResponseJsonString = JsonConvert.SerializeObject(result.message) };
                        return new SendEmailResponseViewModel
                        {
                            Status = result.status,
                            ResponseJsonString = JsonConvert.SerializeObject(result.message),
                            BulkId = (result.message != null && result.message.Count > 0) ? result.message[0] : "",
                            //If successed response status is ok, non content
                            Messages = sendEmailRequestViewModel.ToEmails.Select(da => new SendEmailResponseViewModel_messages()
                            {
                                to = da.Address,
                                messageCount = 1,
                                status_reason = "",
                                status = true
                            }).ToList()
                        };
                    }
                }
                else if (this.providerAuth.Provider == EmailProviders.MailChimp)
                {
                    //using (MandrillSupport mandrillSupport = new MandrillSupport())
                    //{
                    //    var result = await mandrillSupport.SendEmail(sendEmailRequestViewModel);
                    //    //return new SendEmailResponseViewModel { Status = result.status, ResponseJsonString = JsonConvert.SerializeObject(result.message) };
                    //    return new SendEmailResponseViewModel
                    //    {
                    //        Status = result.status,
                    //        ResponseJsonString = JsonConvert.SerializeObject(result.message),
                    //        Messages = SendEmailResponseViewModel.ToViewModel(result.message)
                    //    };
                    //}
                }
                return new SendEmailResponseViewModel { Status = false, ResponseJsonString = "EmailProvider:" + providerAuth.Provider + " is missing" };
            }
            catch (Exception ex)
            {
                return new SendEmailResponseViewModel { Status = false, ResponseJsonString = JsonConvert.SerializeObject(ex) };
            }
        }
        public int emailSetCount { get { return providerAuth?.EmailSetCount ?? 1000; } }
        public ProviderAuthViewModel ProviderAuth { get { return providerAuth; } }
        public async Task<List<SendEmailResponseViewModel>> SendBulkEmailsAsync(SendEmailRequestViewModel sendEmailRequestViewModel)
        {
            List<SendEmailResponseViewModel> models = new List<SendEmailResponseViewModel>();
            try
            {
                if (this.providerAuth?.Provider == null)
                {
                    throw new Exception("Mandatory email provider auth details missing or email provider not selected");
                }
                int emailCnt = sendEmailRequestViewModel?.ToEmails?.Count() ?? 0;
                if (emailCnt == 0)
                {
                    throw new Exception("To email is missing");
                }
                var src = JsonConvert.DeserializeObject<SendEmailRequestViewModel>(Newtonsoft.Json.JsonConvert.SerializeObject(sendEmailRequestViewModel));
                var emailSetCount = this.emailSetCount;
                for (int i = 0; i < emailCnt; i += emailSetCount)
                {
                    sendEmailRequestViewModel.ToEmails = src.ToEmails.Skip(i).Take(emailSetCount);
                    if (this.providerAuth.Provider == EmailProviders.InfoBip)
                    {
                        using (Emails.InfoBip.InfoBipEmailSupport infobipEmailSupport = new Emails.InfoBip.InfoBipEmailSupport())
                        {
                            var result = await infobipEmailSupport.SendEmailAsync(providerAuth, sendEmailRequestViewModel);
                            //return new SendEmailResponseViewModel { Status = result.status, ResponseJsonString = JsonConvert.SerializeObject(result.data) };
                            var ret = new SendEmailResponseViewModel
                            {
                                Status = result.status,
                                ResponseJsonString = JsonConvert.SerializeObject(result.data),
                                BulkId = result.data.bulkId,
                                Messages = SendEmailResponseViewModel.ToViewModel(result.data.messages)
                            };
                            models.Add(ret);
                        }
                    }
                    else if (this.providerAuth.Provider == EmailProviders.SendGrid)
                    {
                        using (Emails.SendGridEmailSupport sendGridSupport = new Emails.SendGridEmailSupport())
                        {
                            var result = await sendGridSupport.SendEmailAsync(providerAuth, sendEmailRequestViewModel);
                            //return new SendEmailResponseViewModel { Status = result.status, ResponseJsonString = JsonConvert.SerializeObject(result.message) };
                            models.Add(new SendEmailResponseViewModel
                            {
                                Status = result.status,
                                ResponseJsonString = JsonConvert.SerializeObject(result.message),
                                BulkId = (result.message != null && result.message.Count > 0) ? result.message[0] : "",
                                //If successed response status is ok, non content
                                Messages = sendEmailRequestViewModel.ToEmails.Select(da => new SendEmailResponseViewModel_messages()
                                {
                                    to = da.Address,
                                    messageCount = 1,
                                    status_reason = "",
                                    status = true
                                }).ToList()
                            });
                        }
                    }
                    else if (this.providerAuth.Provider == EmailProviders.MailChimp)
                    {
                        //using (MandrillSupport mandrillSupport = new MandrillSupport())
                        //{
                        //    var result = await mandrillSupport.SendEmail(sendEmailRequestViewModel);
                        //    //return new SendEmailResponseViewModel { Status = result.status, ResponseJsonString = JsonConvert.SerializeObject(result.message) };
                        //    models.Add( new SendEmailResponseViewModel
                        //    {
                        //        Status = result.status,
                        //        ResponseJsonString = JsonConvert.SerializeObject(result.message),
                        //        Messages = SendEmailResponseViewModel.ToViewModel(result.message)
                        //    });
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                models.Add(new SendEmailResponseViewModel { Status = false, ResponseJsonString = JsonConvert.SerializeObject(ex) });
            }
            return models;
        }
    }
}
