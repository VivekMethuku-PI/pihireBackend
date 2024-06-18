using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.Utilities
{
    //public enum ProviderCode
    //{
    //    IB, // InfoBip,
    //    SG, // SendGrid,
    //    MC // MailChimp
    //}
    public enum EmailProviders
    {
        InfoBip = 1,
        SendGrid = 2,
        MailChimp = 3
    }

    public enum MailDeliveryStatus
    {
        NotSent = 0,
        Sent = 1,
        Blocked = 2,
        Bounced = 3,
        Optedout = 4,
        Opened = 5,
        Clicked = 6
    }

    public enum InfoBip_DeliveryStatus
    {
        NotStarted = 0,
        DELIVERED = 1,
        NotAvailable = 2,
        PENDING = 3,
        UNDELIVERABLE = 4,
        EXPIRED = 5,
        REJECTED = 6,
        Dropped = 7,
        Bounced = 8,
        SystemError = 9,
        OK = 10,
        HANDSET_ERRORS = 11,
        USER_ERRORS = 12,
        OPERATOR_ERRORS = 13,
        EC_HARD_BOUNCE = 200,
        Others = (byte.MaxValue - 1),
    }
    public enum MailGrid_DeliveryStatus
    {

        ///Delivery events
        //Processed
        //Message has been received and is ready to be delivered.
        processed,
        //Dropped
        //You may see the following drop reasons: Invalid SMTPAPI header, Spam Content (if Spam Checker app is enabled), Unsubscribed Address, Bounced Address, Spam Reporting Address, Invalid, Recipient List over Package Quota
        dropped,
        //Delivered
        //Message has been successfully delivered to the receiving server.
        delivered,
        //Deferred
        //Receiving server temporarily rejected the message.
        deferred,
        //Bounce
        //Receiving server could not or would not accept mail to this recipient permanently. If a recipient has previously unsubscribed from your emails, the message is dropped.
        bounce,
        //Blocked
        //Receiving server could not or would not accept the message temporarily. If a recipient has previously unsubscribed from your emails, the message is dropped.
        //bounce - type->blocked
        blocked,

        ///Engagement events
        //Open
        //Recipient has opened the HTML message. Open Tracking needs to be enabled for this type of event.
        open,
        //Click
        //Recipient clicked on a link within the message. Click Tracking needs to be enabled for this type of event.
        click,
        //Spam Report
        //Recipient marked message as spam.
        spamreport,
        //Unsubscribe
        //Recipient clicked on the 'Opt Out of All Emails' link (available after clicking the message's subscription management link). Subscription Tracking needs to be enabled for this type of event.
        unsubscribe,
        //Group Unsubscribe
        //Recipient unsubscribed from a specific group either by clicking the link directly or updating their preferences. Subscription Tracking needs to be enabled for this type of event.
        group_unsubscribe,
        //Group Resubscribe
        //Recipient resubscribed to a specific group by updating their preferences. Subscription Tracking needs to be enabled for this type of event.
        group_resubscribe
    }
}
