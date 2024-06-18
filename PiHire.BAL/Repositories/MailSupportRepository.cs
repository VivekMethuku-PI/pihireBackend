using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PiHire.Utilities.ViewModels.Communications.Emails;
using PiHire.Utilities;
//using Z.EntityFramework.Plus;

namespace PiHire.BAL.Repositories
{
    public class MailSupportRepository : BaseRepository, IRepositories.IMailSupportRepository
    {
        readonly Logger logger;
        ILogger<MailSupportRepository> _logger;
        public MailSupportRepository(DAL.PiHIRE2Context dbContext, Common.Extensions.AppSettings appSettings,
            ILogger<MailSupportRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            this._logger = logger;
        }

        public async Task SendGrid_Webhook(List<SendGrid_WebhookViewModel> model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
               // logger.Log(LogLevel.Debug, LoggingEvents.Other, "Process start: model->" + Newtonsoft.Json.JsonConvert.SerializeObject(model));
                {
                    var curDt = CurrentTime;
                    var ty = typeof(MailGrid_DeliveryStatus);
                    string val;
                    foreach (SendGrid_WebhookViewModel rptRslt in model)
                    {
                        //var id = await dbContext.PiDistributionContacts.Where(da => rptRslt.sg_message_id.StartsWith(da.InfobipBulkId))
                        //    .Join(dbContext.PiContacts, da => da.ContactId, da2 => da2.Id, (da, da2) => new { da.Id, da.ContactId, da2.EmailId })
                        //    .Where(daa => daa.EmailId == rptRslt.email).Select(da => da.Id).FirstOrDefaultAsync();
                        //if (id > 0)
                        //{
                        //    val = SendGrid_WebhookViewModel.GetEventValue(rptRslt);
                        //    MailGrid_DeliveryStatus deliveryStatus = (MailGrid_DeliveryStatus)Enum.Parse(ty, val);
                        //    if (deliveryStatus == MailGrid_DeliveryStatus.bounce && rptRslt.type == "blocked")
                        //    {
                        //        deliveryStatus = MailGrid_DeliveryStatus.blocked;
                        //    }
                        //    MailDeliveryStatus? distStatus = SendGrid_WebhookViewModel.ConvertToMailDeliveryStatus(deliveryStatus);
                        //    if (distStatus.HasValue)
                        //        await dbContext.PiDistributionContacts.Where(da => da.Id == id).UpdateAsync(da => new PiDistributionContacts { InfoBipStatus = (byte)deliveryStatus, DistStatus = (byte)distStatus.Value, UpdatedDate = curDt });
                        //    else
                        //        await dbContext.PiDistributionContacts.Where(da => da.Id == id).UpdateAsync(da => new PiDistributionContacts { InfoBipStatus = (byte)deliveryStatus, UpdatedDate = curDt });
                        //}
                        //else
                        //    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Data not found for :" + JsonConvert.SerializeObject(rptRslt));
                    }
                }
                logger.Log(LogLevel.Debug, LoggingEvents.Other, "Process completed");
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, LoggingEvents.Other, "", e);
            }
            return;
        }
        public async Task MailChimp_Webhook(List<MailChimp_WebhookViewModel> model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
              //  logger.Log(LogLevel.Debug, LoggingEvents.Other, "Process start: model->" + Newtonsoft.Json.JsonConvert.SerializeObject(model));
                {
                    var curDt = CurrentTime;
                    var ty = typeof(MailGrid_DeliveryStatus);
                    string val;
                    foreach (var rptRslt in model)
                    {
                        //var id = await dbContext.PiDistributionContacts.Where(da => rptRslt.sg_message_id.StartsWith(da.InfobipBulkId))
                        //    .Join(dbContext.PiContacts, da => da.ContactId, da2 => da2.Id, (da, da2) => new { da.Id, da.ContactId, da2.EmailId })
                        //    .Where(daa => daa.EmailId == rptRslt.email).Select(da => da.Id).FirstOrDefaultAsync();
                        //if (id > 0)
                        //{
                        //    val = MailChimp_WebhookViewModel.GetEventValue(rptRslt);
                        //    MailGrid_DeliveryStatus deliveryStatus = (MailGrid_DeliveryStatus)Enum.Parse(ty, val);
                        //    if (deliveryStatus == MailGrid_DeliveryStatus.bounce && rptRslt.type == "blocked")
                        //    {
                        //        deliveryStatus = MailGrid_DeliveryStatus.blocked;
                        //    }
                        //    MailDeliveryStatus? distStatus = SendGrid_WebhookViewModel.ConvertToMailDeliveryStatus(deliveryStatus);
                        //    if (distStatus.HasValue)
                        //        await dbContext.PiDistributionContacts.Where(da => da.Id == id).UpdateAsync(da => new PiDistributionContacts { InfoBipStatus = (byte)deliveryStatus, DistStatus = (byte)distStatus.Value, UpdatedDate = curDt });
                        //    else
                        //        await dbContext.PiDistributionContacts.Where(da => da.Id == id).UpdateAsync(da => new PiDistributionContacts { InfoBipStatus = (byte)deliveryStatus, UpdatedDate = curDt });
                        //}
                        //else
                        //    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Data not found for :" + JsonConvert.SerializeObject(rptRslt));
                    }
                }
                logger.Log(LogLevel.Debug, LoggingEvents.Other, "Process completed");
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, LoggingEvents.Other, "", e);
            }
            return;
        }


        public async Task InfoBip_Webhook(Utilities.Communications.Emails.InfoBip.InfoBipNotifyReport model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.Other, "Process start: model->" + Newtonsoft.Json.JsonConvert.SerializeObject(model));
                {
                    var curDt = CurrentTime;
                    var ty = typeof(MailGrid_DeliveryStatus);
                    string val;
                    foreach (var rptRslt in model.results)
                    {
                        var obj = await dbContext.PhBgJobDetails.Where(da => rptRslt.bulkId == da.BulkReferenceId).FirstOrDefaultAsync();
                        if (obj != null)
                        {
                            if (rptRslt.status.groupName.Trim().ToLower() == "DELIVERED".Trim().ToLower())
                            {
                                obj.DeliveredCount += rptRslt.messageCount;
                            }
                        }
                        else
                            logger.Log(LogLevel.Debug, LoggingEvents.Other, "Data not found for :" + JsonConvert.SerializeObject(rptRslt));
                    }
                }
                logger.Log(LogLevel.Debug, LoggingEvents.Other, "Process completed");
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, LoggingEvents.Other, "", e);
            }
            return;
        }
    }
}
