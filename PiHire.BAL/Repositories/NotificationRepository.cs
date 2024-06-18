using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        readonly Logger logger;
        public NotificationRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<NotificationRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        
        public Task<List<string>> GetConnectionsIds(int[] userIds)
        {
            try
            {
                var connectioId = dbContext.GetDeviceConnectionsId(userIds);
                return connectioId;
            }
            catch (Exception)
            {
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<List<UserNotificationsViewModel>> GetUserNotifications()
        {
            try
            {
                var userNotification = await (from ff in dbContext.PhNotifications
                                              join cc in dbContext.PhNotificationsUsers on ff.Id equals cc.NotIid
                                              join uu in dbContext.PiHireUsers on ff.CreatedBy equals uu.Id
                                              where cc.PushTo == Usr.Id && cc.Status == (byte)RecordStatus.Active && ff.Status == (byte)RecordStatus.Active
                                              select new UserNotificationsViewModel
                                              {
                                                  Id = ff.Id,
                                                  JobId = ff.Joid,
                                                  NoteDesc = ff.NotesDesc,
                                                  Title = ff.Title,
                                                  NotiStatus = cc.NotiStatus,
                                                  Photo = uu.ProfilePhoto,
                                                  UserType = uu.UserType,
                                                  EmailId = uu.EmailId,
                                                  CreatedBy = ff.CreatedBy,
                                                  CreatedDate = ff.CreatedDate,
                                                  NotiPushedUser = new NotiPushedUser
                                                  {
                                                      FirstName = uu.FirstName,
                                                      LastName = uu.LastName
                                                  }
                                              }).OrderByDescending(x => x.CreatedDate).ToListAsync();
                foreach (var item in userNotification)
                {
                    if (!string.IsNullOrEmpty(item.Photo))
                    {
                        if (item.UserType != (byte)UserType.Candidate)
                        {
                            item.ProfilePhoto = this.appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + item.CreatedBy + "/ProfilePhoto/" + item.Photo;
                        }
                        else
                        {
                            item.ProfilePhoto = item.Photo;
                        }
                    }
                }
                return userNotification;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<(string Message, bool Status)> SaveNotification(List<NotificationPushedViewModel> notificationPushedViewModels)
        {
            try
            {
                using (var trans = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var model in notificationPushedViewModels)
                        {
                            if (!string.IsNullOrEmpty(model.NoteDesc))
                            {
                                var notification = new PhNotification
                                {
                                    Joid = model.JobId,
                                    Title = model.Title,
                                    NotesDesc = model.NoteDesc,
                                    Status = (byte)RecordStatus.Active,
                                    CreatedBy = model.CreatedBy,
                                    CreatedDate = CurrentTime,
                                    UpdatedBy = model.CreatedBy,
                                    UpdatedDate = CurrentTime
                                };
                                await dbContext.PhNotifications.AddAsync(notification);
                                await dbContext.SaveChangesAsync();
                                foreach (var item in model.PushedTo)
                                {
                                    await dbContext.PhNotificationsUsers.AddAsync(new PhNotificationsUser
                                    {
                                        NotIid = notification.Id,
                                        NotiStatus = 0,
                                        PushTo = item,
                                        Status = (byte)RecordStatus.Active
                                    });
                                }
                                await dbContext.SaveChangesAsync();
                            }
                        }
                        trans.Commit();
                        return ("Notification save successfully", true);
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        return (e.Message, false);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(string Message, bool Status)> UpdateReadStatus()
        {
            using (var trans = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var unReadNoti = dbContext.PhNotificationsUsers.Where(s => s.PushTo == Usr.Id && s.NotiStatus == (byte)NotificationStatus.unRead && s.Status == (byte)RecordStatus.Active).ToList();
                    foreach (var item in unReadNoti)
                    {
                        item.NotiStatus = (byte)NotificationStatus.read;
                    }
                    await dbContext.SaveChangesAsync();
                    trans.Commit();
                    return ("Updated successully", true);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return (ex.Message, false);
                }
            }
        }
    }
}
