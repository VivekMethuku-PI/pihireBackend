using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class JobMailCountViewModel
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
    }
    public class JobMailGroupViewModel
    {
        public List<JobMailViewModel> jobMails { get; set; }
        public string conversationId { get; set; }
    }
    public class JobMailViewModel
    {
        //public string OdataEtag { get; set; }
        public string id { get; set; }
        public DateTime? createdDateTime { get; set; }
        public DateTime? lastModifiedDateTime { get; set; }
        //public string changeKey { get; set; }
        public List<object> categories { get; set; }
        public DateTime? receivedDateTime { get; set; }
        public DateTime? sentDateTime { get; set; }
        public bool? hasAttachments { get; set; }
        //public string internetMessageId { get; set; }
        public string subject { get; set; }
        public string bodyPreview { get; set; }
        public string importance { get; set; }
        public string parentFolderId { get; set; }
        //public string conversationId { get; set; }
        public string conversationIndex { get; set; }
        public bool? isDeliveryReceiptRequested { get; set; }
        public bool? isReadReceiptRequested { get; set; }
        public bool? isRead { get; set; }
        public bool? isDraft { get; set; }
        //public string webLink { get; set; }
        public string inferenceClassification { get; set; }
        public JobMailViewModels.Body body { get; set; }
        public JobMailViewModels.Sender sender { get; set; }
        public JobMailViewModels.From from { get; set; }
        public List<JobMailViewModels.ToRecipient> toRecipients { get; set; }
        public List<JobMailViewModels.CcRecipient> ccRecipients { get; set; }
        public List<JobMailViewModels.BccRecipient> bccRecipients { get; set; }
        public List<JobMailViewModels.ReplyTos> replyTo { get; set; }
        public JobMailViewModels.Flag flag { get; set; }
        public bool isSender { get; set; }

        public List<JobMailViewModels.Attachment> attachments { get; set; }

        public static JobMailViewModel ToViewModel(PiHire.BAL.Common._3rdParty.Microsoft.GetMailsByConversation.GetMailsByConversation_Value da, string credUserName)
        {
            return new JobMailViewModel
            {
                id = da.id,
                subject = da.subject,
                body = JobMailViewModels.Body.ToViewModel(da.body),
                bodyPreview = da.bodyPreview,
                categories = da.categories,
                conversationIndex = da.conversationIndex,
                flag = JobMailViewModels.Flag.ToViewModel(da.flag),
                createdDateTime = da.createdDateTime,
                sentDateTime = da.sentDateTime,
                lastModifiedDateTime = da.lastModifiedDateTime,
                receivedDateTime = da.receivedDateTime,

                isDeliveryReceiptRequested = da.isDeliveryReceiptRequested,
                isDraft = da.isDraft,
                isRead = da.isRead,
                isReadReceiptRequested = da.isReadReceiptRequested,
                hasAttachments = da.hasAttachments,
                importance = da.importance,
                parentFolderId = da.parentFolderId,
                inferenceClassification = da.inferenceClassification,

                toRecipients = da.toRecipients.Select(da => JobMailViewModels.ToRecipient.ToViewModel(da)).ToList(),
                ccRecipients = da.ccRecipients.Select(da => JobMailViewModels.CcRecipient.ToViewModel(da)).ToList(),
                bccRecipients = da.bccRecipients.Select(da => JobMailViewModels.BccRecipient.ToViewModel(da)).ToList(),
                replyTo = da.replyTo.Select(da => JobMailViewModels.ReplyTos.ToViewModel(da)).ToList(),
                from = JobMailViewModels.From.ToViewModel(da.from),
                sender = JobMailViewModels.Sender.ToViewModel(da.sender),
                isSender = da.sender.emailAddress.address == credUserName
            };
        }
    }
}
namespace PiHire.BAL.ViewModels.JobMailViewModels
{
    public class Body
    {
        public string contentType { get; set; }
        public string content { get; set; }

        internal static Body ToViewModel(Common._3rdParty.Microsoft.Microsoft_Body body)
        {
            return new Body { content = body.content, contentType = body.contentType };
        }
    }
    public class EmailAddress
    {
        public string name { get; set; }
        public string address { get; set; }
        internal static EmailAddress ToViewModel(Common._3rdParty.Microsoft.Microsoft_EmailAddress email)
        {
            return new EmailAddress { name = email?.name ?? "", address = email?.address ?? "" };
        }
    }
    public class ToRecipient
    {
        public EmailAddress emailAddress { get; set; }

        internal static ToRecipient ToViewModel(Common._3rdParty.Microsoft.Microsoft_ToRecipient da)
        {
            return new ToRecipient { emailAddress = EmailAddress.ToViewModel(da?.emailAddress??new Common._3rdParty.Microsoft.Microsoft_EmailAddress()) };
        }
    }
    public class Sender
    {
        public EmailAddress emailAddress { get; set; }

        internal static Sender ToViewModel(Common._3rdParty.Microsoft.Microsoft_Sender da)
        {
            return new Sender { emailAddress = EmailAddress.ToViewModel(da?.emailAddress??new Common._3rdParty.Microsoft.Microsoft_EmailAddress()) };
        }


    }

    public class From
    {
        public EmailAddress emailAddress { get; set; }
        internal static From ToViewModel(Common._3rdParty.Microsoft.Microsoft_From da)
        {
            return new From { emailAddress = EmailAddress.ToViewModel(da?.emailAddress ?? new Common._3rdParty.Microsoft.Microsoft_EmailAddress()) };
        }
    }

    public class CcRecipient
    {
        public EmailAddress emailAddress { get; set; }

        internal static CcRecipient ToViewModel(Common._3rdParty.Microsoft.Microsoft_CcRecipient da)
        {
            return new CcRecipient { emailAddress = EmailAddress.ToViewModel(da?.emailAddress ?? new Common._3rdParty.Microsoft.Microsoft_EmailAddress()) };
        }
    }
    public class BccRecipient
    {
        public EmailAddress emailAddress { get; set; }

        internal static BccRecipient ToViewModel(Common._3rdParty.Microsoft.Microsoft_BccRecipient da)
        {
            return new BccRecipient { emailAddress = EmailAddress.ToViewModel(da?.emailAddress ?? new Common._3rdParty.Microsoft.Microsoft_EmailAddress()) };
        }
    }
    public class ReplyTos
    {
        public EmailAddress emailAddress { get; set; }

        internal static ReplyTos ToViewModel(Common._3rdParty.Microsoft.Microsoft_ReplyTos da)
        {
            return new ReplyTos { emailAddress = EmailAddress.ToViewModel(da?.emailAddress ?? new Common._3rdParty.Microsoft.Microsoft_EmailAddress()) };
        }
    }

    public class Flag
    {
        public string flagStatus { get; set; }

        internal static Flag ToViewModel(Common._3rdParty.Microsoft.GetMailsByConversation.GetMailsByConversation_Flag flag)
        {
            return new Flag { flagStatus = flag.flagStatus };
        }
    }


    public class Attachment
    {
        public string FileName { get; set; }
        public string content { get; set; }
        public string contentType { get; set; }
    }
}
