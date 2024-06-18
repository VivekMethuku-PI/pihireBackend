using PiHire.Utilities.Communications.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.Utilities.Communications.Emails
{
    public enum EmailTypes
    {
        AppUser_Create,
        CandidateEmailVarification,
        ShareProfile_Client,
        SmptMailConfiguration,
        Candidate_Salary_ReOffer,
        Candidate_Salary_Confirmation_Accept_Reject,
        ForgotPassword,
        ResetPassword
    }

    public class EmailTemplates
    {
        public static string GetSubject(EmailTypes emailTypes)
        {
            switch (emailTypes)
            {
                case EmailTypes.AppUser_Create:
                    return "ParamInfo Careers: User Login";
                case EmailTypes.CandidateEmailVarification:
                    return "ParamInfo Careers: Verify your candidate account";
                case EmailTypes.ShareProfile_Client:
                    return "Candidate Profiles";
                case EmailTypes.SmptMailConfiguration:
                    return "ParamInfo Careers: Email Configuration";
                case EmailTypes.Candidate_Salary_ReOffer:
                    return "Salary Re-Offer";
                case EmailTypes.Candidate_Salary_Confirmation_Accept_Reject:
                    return "Salary Confirmation";
                case EmailTypes.ForgotPassword:
                    return "ParamInfo Careers: Reset Password";
                case EmailTypes.ResetPassword:
                    return "ParamInfo Careers: Your Password has Reset Successfully";
                default:
                    return string.Empty;
            }
        }

        public static string Candidate_Salary_ReOffer(string CandidateName, int TakeHomeMonthlySalary, string RecRole, string RecName)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/Candidate_Salary_ReOffer.html");

                vBodyMessage = vBodyMessage.Replace("!recDispName", RecName);
                vBodyMessage = vBodyMessage.Replace("!recRole", RecRole);
                vBodyMessage = vBodyMessage.Replace("!CandidateName", CandidateName);
                vBodyMessage = vBodyMessage.Replace("!TakeHomeMonthlySalary", TakeHomeMonthlySalary.ToString());

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string Candidate_Salary_Confirmation_Accept_Reject(string RecName, string Desc, string Confirmation)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/Candidate_Salary_Confirmation_Accept_Reject.html");

                vBodyMessage = vBodyMessage.Replace("!recName", RecName);
                vBodyMessage = vBodyMessage.Replace("!AcceptedRejectInfo", Desc);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string User_EmailCredentials_Template(string UserDispName, string UserName,
            string Password, string SiteLoginUrl, string appURL, string apiURL)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/UserCredentails.html");

                vBodyMessage = vBodyMessage.Replace("!userDispName", UserDispName);
                vBodyMessage = vBodyMessage.Replace("!userName", UserName);
                vBodyMessage = vBodyMessage.Replace("!password", Password);
                vBodyMessage = vBodyMessage.Replace("!loginUrl", SiteLoginUrl);
                vBodyMessage = vBodyMessage.Replace("!appURL", appURL);
                vBodyMessage = vBodyMessage.Replace("!apiURL", apiURL);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string User_ForgotPassword_Template(string UserDispName, string SiteLoginUrl, string appURL)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/ForgotPassword.html");

                vBodyMessage = vBodyMessage.Replace("!userDispName", UserDispName);
                vBodyMessage = vBodyMessage.Replace("!loginUrl", SiteLoginUrl);
                vBodyMessage = vBodyMessage.Replace("!appURL", appURL);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string User_RestPassword_Template(string UserDispName, string SiteLoginUrl, string appURL)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/ResetPassword.html");

                vBodyMessage = vBodyMessage.Replace("!userDispName", UserDispName);
                vBodyMessage = vBodyMessage.Replace("!loginUrl", SiteLoginUrl);
                vBodyMessage = vBodyMessage.Replace("!appURL", appURL);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string User_RestPassword2_Template(string SiteLoginUrl, string appURL, string Username, string password)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/ResetPassword2.html");

                vBodyMessage = vBodyMessage.Replace("!loginUrl", SiteLoginUrl);
                vBodyMessage = vBodyMessage.Replace("!appURL", appURL);
                vBodyMessage = vBodyMessage.Replace("!Username", Username);
                vBodyMessage = vBodyMessage.Replace("!Password", password);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string User_Smpt_Mail_Configuration_Template(string UserDispName, string SiteLoginUrl)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/SmptMailConfiguration.html");

                vBodyMessage = vBodyMessage.Replace("!UserDispName", UserDispName);
                vBodyMessage = vBodyMessage.Replace("!siteVerificationUrl", SiteLoginUrl);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string Candidate_ProfilesToClient_Template(string UserName, string UserRole,
          string JobName, string JobRole, string ClientName, string spocName, string MobileNo,
          List<ClientCandidateShareProfiles> clientCandidateShareProfiles, string JobCurrency, string EmailFields,
          string appURL, string apiURL, string userEmailId)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/ShareProfilesToClient.html");
                vBodyMessage = vBodyMessage.Replace("!userDispName", UserName);

                if (!string.IsNullOrEmpty(UserRole))
                {
                    vBodyMessage = vBodyMessage.Replace("!userRole", "" + UserRole + " ");
                }
                else
                {
                    vBodyMessage = vBodyMessage.Replace("!userRole", string.Empty);
                }

                if (!string.IsNullOrEmpty(MobileNo))
                {
                    vBodyMessage = vBodyMessage.Replace("!mobileNumber", MobileNo);
                }
                else
                {
                    vBodyMessage = vBodyMessage.Replace("!mobileNumber", string.Empty);
                }

                vBodyMessage = vBodyMessage.Replace("!emailId", userEmailId);
                vBodyMessage = vBodyMessage.Replace("!jobName", JobName);
                vBodyMessage = vBodyMessage.Replace("!roleName", JobRole);
                vBodyMessage = vBodyMessage.Replace("!clientName", ClientName);
                vBodyMessage = vBodyMessage.Replace("!spocName", spocName);
                vBodyMessage = vBodyMessage.Replace("!currency", JobCurrency);
                vBodyMessage = vBodyMessage.Replace("!appURL", appURL);
                vBodyMessage = vBodyMessage.Replace("!apiURL", apiURL);
                string tableHeader = string.Empty;
                string tableBody = string.Empty;

                List<int> emailFields = EmailFields.Split(',').Select(int.Parse).ToList();
                emailFields.Sort();

                tableHeader = tableHeader + " " +
                  "<tr style='height: 14pt'><td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'><div style='text - indent: 0pt; text - align: center; '>S.no</td> <td style='text-align:center;border-color: #ccc;min-width:60pt;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'> <div style='text - indent: 0pt; text - align: center; '>Resource Name</div> </td> ";
                foreach (var item in emailFields)
                {
                    if (item == 2)
                    {
                        tableHeader = tableHeader + " <td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'><div style='text - indent: 0pt; text - align: center; '> Job Name </div> </td> ";
                    }
                    if (item == 3)
                    {
                        tableHeader = tableHeader + " <td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'> <div style='text - indent: 0pt; text - align: center; '>Email</div>  </td> ";
                    }
                    if (item == 4)
                    {
                        tableHeader = tableHeader + " <td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'> <div style='text - indent: 0pt; text - align: center; '>Mobile Number</div> </td> ";
                    }
                    if (item == 5)
                    {
                        tableHeader = tableHeader + " <td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'><div style='text - indent: 0pt; text - align: center; '> Notice Period </div> </td> ";
                    }
                    if (item == 6)
                    {
                        tableHeader = tableHeader + " <td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'> <div style='text - indent: 0pt; text - align: center; '>Experience</div> </td> ";
                    }
                    if (item == 7)
                    {
                        tableHeader = tableHeader + " <td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'> <div style='text - indent: 0pt; text - align: center; '>Location</div>  </td> ";
                    }
                    if (item == 8)
                    {
                        tableHeader = tableHeader + " <td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'> <div style='text - indent: 0pt; text - align: center; '>Budget</div> </td> ";
                    }

                }
                tableHeader = tableHeader + " " + " <td style='text-align:center;border-color: #ccc;padding: 6px; background: #645df9; color: #ffffff;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'> <div style='text - indent: 0pt; text - align: center; '>Action</div>  </td> </tr>";

                for (int i = 0; i < clientCandidateShareProfiles.Count; i++)
                {
                    tableBody = tableBody + " " +
                        "<tr style='height: 14pt'><td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + (i + 1) + "</td> <td style='text-align:left;border-color: #ccc;min-width:60pt;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + clientCandidateShareProfiles[i].ResourceName + "</td>";

                    if (emailFields.Contains(2))
                    {
                        tableBody = tableBody + "<td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + JobName + " </td> ";
                    }
                    if (emailFields.Contains(3))
                    {
                        tableBody = tableBody + "<td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + clientCandidateShareProfiles[i].Email + " </td> ";
                    }
                    if (emailFields.Contains(4))
                    {
                        tableBody = tableBody + "<td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + clientCandidateShareProfiles[i].MobileNumber + " </td> ";
                    }
                    if (emailFields.Contains(5))
                    {
                        string Np = clientCandidateShareProfiles[i].NoticePeriod;
                        if (clientCandidateShareProfiles[i].NoticePeriod == "0" || string.IsNullOrEmpty(Np))
                        {
                            Np = "Immediately";
                        }
                        tableBody = tableBody + " <td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + Np + "  </td> ";
                    }
                    if (emailFields.Contains(6))
                    {
                        tableBody = tableBody + "<td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + clientCandidateShareProfiles[i].TotalExperiance + " </td> ";
                    }
                    if (emailFields.Contains(7))
                    {
                        string location = string.Empty;
                        if (!string.IsNullOrEmpty(clientCandidateShareProfiles[i].CurrLocation) && !string.IsNullOrEmpty(clientCandidateShareProfiles[i].Country))
                        {
                            location = clientCandidateShareProfiles[i].CurrLocation + "/" + clientCandidateShareProfiles[i].Country;
                        }
                        else if (!string.IsNullOrEmpty(clientCandidateShareProfiles[i].CurrLocation) && string.IsNullOrEmpty(clientCandidateShareProfiles[i].Country))
                        {
                            location = clientCandidateShareProfiles[i].CurrLocation;
                        }
                        else if (string.IsNullOrEmpty(clientCandidateShareProfiles[i].CurrLocation) && !string.IsNullOrEmpty(clientCandidateShareProfiles[i].Country))
                        {
                            location = clientCandidateShareProfiles[i].Country;
                        }
                        else
                        {
                            location = string.Empty;
                        }
                        tableBody = tableBody + " <td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + location + " </td> ";
                    }
                    if (emailFields.Contains(8))
                    {
                        string cbSalary = string.Empty;
                        string cbCurrency = clientCandidateShareProfiles[i].cbCurrency;
                        int? salary = clientCandidateShareProfiles[i].cbSalary;

                        if (salary != 0 && salary != null)
                        {
                            if (!string.IsNullOrEmpty(cbCurrency))
                            {
                                cbSalary = salary + "(" + cbCurrency + ")";
                            }
                            else
                            {
                                cbSalary = salary.ToString();
                            }
                        }

                        tableBody = tableBody + " <td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>" + cbSalary + "</td> ";
                    }
                    tableBody = tableBody + "" + "<td style='text-align:center; border-color: #ccc;padding: 6px;font-family: Helvetica;font-size: 12px;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'> <a href='!redirectURL' style='color: #645df9;padding: 2px 5px;'> View </a>" + " " +
                        "</td></tr>";
                    tableBody = tableBody.Replace("!redirectURL", clientCandidateShareProfiles[i].Review);
                }

                vBodyMessage = vBodyMessage.Replace("!TableHeader", tableHeader);
                vBodyMessage = vBodyMessage.Replace("!TableBody", tableBody);
                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string Candidate_EmailVerification_Template(string UserDispName, string VarificationUrl, string appURL)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/Candidate_EmailVerification.html");

                vBodyMessage = vBodyMessage.Replace("!userDispName", UserDispName);
                vBodyMessage = vBodyMessage.Replace("!loginUrl", VarificationUrl);
                vBodyMessage = vBodyMessage.Replace("!appURL", appURL);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string LeaveApproval(string RecipientName, string LeaveType, DateTime StartDate, DateTime EndDate, string Reasons, string Signature, string LeaveStatus)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/LeaveApproval.html");

                vBodyMessage = vBodyMessage.Replace("!RecipientName", RecipientName);
                vBodyMessage = vBodyMessage.Replace("!LeaveType", LeaveType);
                vBodyMessage = vBodyMessage.Replace("!StartDate", StartDate.ToString("dd/MM/yyyy"));
                vBodyMessage = vBodyMessage.Replace("!EndDate", EndDate.ToString("dd/MM/yyyy"));
                vBodyMessage = vBodyMessage.Replace("!Reasons", Reasons);
                vBodyMessage = vBodyMessage.Replace("!Signature", Signature);
                vBodyMessage = vBodyMessage.Replace("!LeaveStatus", LeaveStatus);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string LeaveRequest(string RecipientName, string LeaveType, DateTime StartDate, DateTime EndDate, string Reasons, string Signature, string LeaveStatus)
        {
            try
            {
                string vBodyMessage = string.Empty;
                vBodyMessage = File.ReadAllText("EmailTemplates/LeaveRequest.html");

                vBodyMessage = vBodyMessage.Replace("!RecipientName", RecipientName);
                vBodyMessage = vBodyMessage.Replace("!LeaveType", LeaveType);
                vBodyMessage = vBodyMessage.Replace("!StartDate", StartDate.ToString("dd/MM/yyyy"));
                vBodyMessage = vBodyMessage.Replace("!EndDate", EndDate.ToString("dd/MM/yyyy"));
                vBodyMessage = vBodyMessage.Replace("!Reasons", Reasons);
                vBodyMessage = vBodyMessage.Replace("!Signature", Signature);
                vBodyMessage = vBodyMessage.Replace("!LeaveStatus", LeaveStatus);

                return vBodyMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }

    public class CandidateResumeAttachments
    {
        public int CandProfId { get; set; }
        public int JoId { get; set; }
        public string CandName { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] Data { get; set; }

    }

    public class SmtpMailing
    {
        public string SmtpAddress { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }

        public SmtpMailing(string SmtpAddress, int SmtpPort, string SmtpLoginName, string SmtpLoginPassword, bool SmtpEnableSsl, string SmtpFromEmail, string SmtpFromName)
        {

            this.SmtpAddress = SmtpAddress;
            this.Port = SmtpPort;
            this.LoginName = SmtpLoginName;
            this.LoginPassword = SmtpLoginPassword;
            this.EnableSsl = SmtpEnableSsl;
            this.FromEmail = SmtpFromEmail;
            this.FromName = SmtpFromName;
        }

        public async Task SendMail(string ToMail, string Subject, string HtmlBody, string CCMail)
        {
            // void
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(FromEmail, FromName);
                string[] ToMails = ToMail.Split(",");
                foreach (var item in ToMails)
                {
                    mail.To.Add(item);
                }
                if (!string.IsNullOrEmpty(CCMail))
                {
                    string[] ccMails = CCMail.Split(",");
                    foreach (var item in ccMails)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            if (!ToMails.Contains(item))
                            {
                                mail.CC.Add(item);
                            }
                        }
                    }
                }
                mail.Subject = Subject;
                mail.IsBodyHtml = true;
                mail.Body = HtmlBody;
                using (SmtpClient SmtpServer = new SmtpClient(SmtpAddress, Port))
                {
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(LoginName, LoginPassword);
                    SmtpServer.EnableSsl = EnableSsl;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    await SmtpServer.SendMailAsync(mail);
                }
            }
        }

        public void SendMail(string ToMail, string Subject, string HtmlBody, string CCMail, List<CandidateResumeAttachments> candidateAttachments)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(FromEmail, FromName);
                string[] ToMails = ToMail.Split(",");
                foreach (var item in ToMails)
                {
                    mail.To.Add(item);
                }
                if (!string.IsNullOrEmpty(CCMail))
                {
                    string[] ccMails = CCMail.Split(",");
                    foreach (var item in ccMails)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            if (!ToMails.Contains(item))
                            {
                                mail.CC.Add(item);
                            }
                        }
                    }
                }

                if (candidateAttachments.Count > 0)
                {
                    foreach (var item in candidateAttachments)
                    {
                        MemoryStream memoryStream = new MemoryStream(item.Data);
                        if (item.FileType.ToLower() == "application/pdf") { mail.Attachments.Add(new Attachment(memoryStream, item.FileName, MediaTypeNames.Application.Pdf)); }
                        else
                        {
                            mail.Attachments.Add(new Attachment(memoryStream, item.FileName, MediaTypeNames.Application.Soap));
                        }
                    }
                }
                mail.Subject = Subject;
                mail.IsBodyHtml = true;
                mail.Body = HtmlBody;
                using (SmtpClient SmtpServer = new SmtpClient(SmtpAddress, Port))
                {
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(LoginName, LoginPassword);
                    SmtpServer.EnableSsl = EnableSsl;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    SmtpServer.Send(mail);
                }
            }

        }

        public async Task SendMail(string ToMail, string Subject, string HtmlBody, string CCMail, byte[] data, string FileName)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(FromEmail, FromName);
                string[] ToMails = ToMail.Split(",");
                foreach (var item in ToMails)
                {
                    mail.To.Add(item);
                }
                if (!string.IsNullOrEmpty(CCMail))
                {
                    string[] ccMails = CCMail.Split(",");
                    foreach (var item in ccMails)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            if (!ToMails.Contains(item))
                            {
                                mail.CC.Add(item);
                            }
                        }
                    }
                }

                MemoryStream memoryStream = new MemoryStream(data);
                mail.Attachments.Add(new Attachment(memoryStream, FileName, MediaTypeNames.Application.Pdf));


                mail.Subject = Subject;
                mail.IsBodyHtml = true;
                mail.Body = HtmlBody;
                using (SmtpClient SmtpServer = new SmtpClient(SmtpAddress, Port))
                {
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(LoginName, LoginPassword);
                    SmtpServer.EnableSsl = EnableSsl;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    ServicePointManager.SecurityProtocol =  SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    await SmtpServer.SendMailAsync(mail);
                }
            }

        }
    }

}
