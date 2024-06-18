using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.DAL.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class ChatComunicationRepository : BaseRepository, IChatComunicationRepository
    {
        readonly Logger logger;
        private readonly IWebHostEnvironment _environment;
        public ChatComunicationRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<ChatComunicationRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<(GetChatMessagesResponseVM data, bool status)> GetAllChatMessages()
        {
            try
            {
                var data = new GetChatMessagesResponseVM();
                data.Messages = (from chat in dbContext.PhChatMessages
                                 join room in dbContext.PhChatRooms on chat.ChatRoomId equals room.Id
                                 join sender in dbContext.PiHireUsers on chat.SenderId equals sender.Id
                                 join receiver in dbContext.PiHireUsers on chat.ReceiverId equals receiver.Id
                                 where (room.UserId == Usr.Id || room.CreatedBy == Usr.Id)
                                 orderby chat.CreatedDate ascending
                                 select new GetChatComunicationViewModel
                                 {
                                     FileName = chat.FileName,
                                     Message = chat.Message,
                                     ReadStatus = chat.ReadStatus,
                                     ReceiverId = chat.ReceiverId,
                                     SenderId = chat.SenderId,
                                     ReceiverName = receiver.FirstName + " " + receiver.LastName,
                                     SenderName = sender.FirstName + " " + sender.LastName,
                                     RoomId = chat.ChatRoomId,
                                     chatMsgId = chat.Id
                                 }).ToList();
                return (data, true);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        /// <summary>
        /// sender Id will login user Id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<(GetChatMessagesResponseVM data, bool status)> GetChatMessages(GetChatMessagesViewModel model)
        {
            try
            {
                var data = new GetChatMessagesResponseVM();
                var chatRoom = await dbContext.PhChatRooms.FirstOrDefaultAsync(f => (f.UserId == model.ReceiverId || f.CreatedBy == model.ReceiverId) && (f.UserId == Usr.Id || f.CreatedBy == Usr.Id) && f.Joid == model.JobId);
                if (chatRoom == null)
                {
                    var jobDesc = await (from op in dbContext.PhJobOpenings
                                         join city in dbContext.PhCities on op.JobLocationId equals city.Id
                                         where op.Id == model.JobId
                                         select new
                                         {
                                             op.JobTitle,
                                             city = city.Name
                                         }).FirstOrDefaultAsync();

                    if (jobDesc == null)
                    {
                        return (null, false);
                    }
                    chatRoom = new DAL.Entities.PhChatRoom
                    {
                        Joid = model.JobId,
                        Title = model.JobId + " - " + jobDesc.JobTitle + " (" + jobDesc.city + ")",
                        Crdesc = "",
                        Status = (byte)RecordStatus.Active,
                        UserId = model.ReceiverId,
                        CreatedBy = Usr.Id,
                        CreatedDate = CurrentTime,
                        UpdatedBy = Usr.Id,
                        UpdatedDate = CurrentTime
                    };
                    await dbContext.PhChatRooms.AddAsync(chatRoom);
                    await dbContext.SaveChangesAsync();
                    data.RoomId = chatRoom.Id;
                    return (data, true);
                }


                data.RoomId = chatRoom.Id;

                data.Messages = (from chat in dbContext.PhChatMessages
                                 join sender in dbContext.PiHireUsers on chat.SenderId equals sender.Id
                                 join receiver in dbContext.PiHireUsers on chat.ReceiverId equals receiver.Id
                                 where chat.ChatRoomId == chatRoom.Id
                                 orderby chat.CreatedDate ascending
                                 select new GetChatComunicationViewModel
                                 {
                                     FileName = chat.FileName,
                                     Message = chat.Message,
                                     ReadStatus = chat.ReadStatus,
                                     ReceiverId = chat.ReceiverId,
                                     SenderId = chat.SenderId,
                                     ReceiverName = receiver.FirstName + " " + (receiver.LastName ?? ""),
                                     SenderName = sender.FirstName + " " + (sender.LastName ?? ""),
                                     RoomId = chat.ChatRoomId,
                                     chatMsgId = chat.Id
                                 }).ToList();
                foreach (var item in data.Messages)
                {
                    item.CreatedDate = ConvertUTCToDateByTimezone(item.CreatedDate, "GMT+04:00", false);
                }
                return (data, true);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<RoomMediaViewModel>> GetRoomMedia(int RoomId)
        {
            try
            {
                string webRootPath = _environment.ContentRootPath + "\\ChatMedia" + "\\" + RoomId + "";
                var roomMedia = await dbContext.PhChatMessages.Where(s => s.ChatRoomId == RoomId && s.Status == (byte)RecordStatus.Active && s.FileName != null && s.FileName != "").OrderByDescending(s => s.Id).Take(20).Select(s => new RoomMediaViewModel
                {
                    MessageId = s.Id,
                    FileUrl = appSettings.AppSettingsProperties.HireApiUrl + "/ChatMedia/" + RoomId + "/" + s.FileName,
                    FileName = s.FileName,

                }).ToListAsync();

                foreach (var item in roomMedia)
                {
                    var fileInfo = new FileInfo(Path.Combine(webRootPath, string.Empty, item.FileName));
                    if (fileInfo.Exists)
                    {
                        item.FileSize = (fileInfo.Length / 1024).ToString();
                    }
                }
                return roomMedia;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<GetRoomsViewModel>> GetRoomsByJob(GetRoomReuestViewModel model)
        {
            try
            {

                var rooms = await (from room in dbContext.PhChatRooms
                                   join us in dbContext.PiHireUsers on room.UserId equals us.Id
                                   join cr in dbContext.PiHireUsers on room.CreatedBy equals cr.Id
                                   join jd in dbContext.PhJobOpenings on room.Joid equals jd.Id
                                   where room.Joid == model.JobId && room.Status == (byte)RecordStatus.Active && (room.UserId == Usr.Id || room.CreatedBy == Usr.Id)
                                   select new GetRoomsViewModel
                                   {
                                       ReceiverId = room.UserId == Usr.Id ? room.CreatedBy : room.UserId,
                                       ReceiverName = room.UserId == Usr.Id ? cr.FirstName + " " + cr.LastName : us.FirstName + " " + us.LastName,
                                       RoomId = room.Id,
                                       JobTitle = jd.JobTitle,
                                       Role = room.UserId == Usr.Id ? cr.UserRoleName : us.UserRoleName,
                                       UpdatedDate = room.UpdatedDate,
                                       ProfileImage = us.ProfilePhoto != null ? (room.UserId == Usr.Id ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + cr.Id + "/ProfilePhoto/" + cr.ProfilePhoto : appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + us.Id + "/ProfilePhoto/" + us.ProfilePhoto) : string.Empty
                                   }).ToListAsync();
                var roomIds = rooms.Select(da => da.RoomId).ToArray();
                var msgCounts = await dbContext.PhChatMessages.AsNoTracking().Where(da => roomIds.Contains(da.ChatRoomId)).GroupBy(da => da.ChatRoomId)
                    .Select(da => new
                    {
                        RoomId = da.Key,
                        LatestMessageDt = da.Max(dai => dai.CreatedDate)
                    }).ToListAsync();

                foreach (var msgCount in msgCounts)
                {
                    var room = rooms.FirstOrDefault(da => da.RoomId == msgCount.RoomId);
                    if (room != null)
                    {
                        room.LatestMessageDt = msgCount.LatestMessageDt;
                    }
                }

                return rooms.OrderByDescending(da => da.LatestMessageDt).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<GetUnreadCountViewModel>> GetUnreadCount()
        {
            try
            {
                var msgQry = dbContext.PhChatMessages.AsNoTracking().Where(da => da.ReceiverId == Usr.Id && da.ReadStatus == (byte)ReadStatus.Unread);
                var roomQry = dbContext.PhChatRooms.AsNoTracking().Where(da => da.Status == (byte)RecordStatus.Active);
                var messages = await msgQry
                    .Join(roomQry, message => message.ChatRoomId, room => room.Id, (message, room) => new { message.Id, message.CreatedDate, message.ChatRoomId, room.Joid })
                    .GroupBy(da => new { da.ChatRoomId, da.Joid })
                    .Select(da => new GetUnreadCountViewModel
                    {
                        JobId = da.Key.Joid,
                        RoomId = da.Key.ChatRoomId,
                        Count = da.Count(),
                        LatestMessageDt = da.Max(dai => dai.CreatedDate)
                    }).ToListAsync();

                return messages.OrderByDescending(da => da.LatestMessageDt).ToList();

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<bool> SaveChatMessage(SendMessageViewModel model, IFormFile file, DateTime createdTime)
        {
            try
            {
                var message = new PhChatMessage
                {
                    ChatRoomId = model.ChatRoomId,
                    Message = model.Message,
                    ReadStatus = (byte)ReadStatus.Unread,
                    FileName = string.Empty,
                    ReceiverId = model.ReceiverId,
                    SenderId = Usr.Id,
                    Status = (byte)RecordStatus.Active,
                    CreatedDate = createdTime
                };

                if (file != null)
                {
                    string webRootPath = _environment.ContentRootPath + "\\ChatMedia" + "\\" + model.ChatRoomId + "";
                    // Checking for folder is available or not 
                    if (!Directory.Exists(webRootPath))
                    {
                        Directory.CreateDirectory(webRootPath);
                    }
                    var fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + file.FileName);
                    fileName = fileName.Replace(" ", "_");
                    if (fileName.Length > 200)
                    {
                        fileName = fileName.Substring(0, 199);
                    }

                    var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    message.FileName = fileName;
                }
                await dbContext.PhChatMessages.AddAsync(message);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateReadStatus(UpdateReadStatusViewModel model)
        {
            try
            {
                var messages = await dbContext.PhChatMessages.Where(s => s.ChatRoomId == model.RoomId && s.ReceiverId == Usr.Id && s.ReadStatus == (byte)ReadStatus.Unread)
                    .OrderByDescending(o => o.Id).ToListAsync();
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        message.ReadStatus = (byte)ReadStatus.Read;
                    }
                    await dbContext.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        
        public async Task<ChatPaginationViewModel> GetRoomsAsync(int PageCount, int SkipCount)
        {
            logger.SetMethodName(System.Reflection.MethodBase.GetCurrentMethod());
            try
            {
               // logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "PageCount:" + PageCount + ", SkipCount:" + SkipCount);

                var data = await dbContext.GetChatRoomsAsync(PageCount, SkipCount, Usr.Id);
              

                var jobIds = data.Select(da => da.JobId).ToArray();
                var CandIds = data.Select(da => da.ReceiverId).ToArray();

                var jobs = await dbContext.PhJobOpenings.Where(da => jobIds.Contains(da.Id)).Select(da => new { da.Id, da.JobTitle }).ToListAsync();
                var cands = await dbContext.PiHireUsers.Where(da => CandIds.Contains(da.Id)).Select(da => new { da.Id, /*da.FirstName, da.LastName,*/ da.ProfilePhoto, da.EmailId }).ToListAsync();
                var emails = cands.Select(da => da.EmailId).ToList();
                var candProfIds = await dbContext.PhCandidateProfiles.Where(da => emails.Contains(da.EmailId))
                    .GroupBy(da => da.EmailId).Select(da => new { EmailId = da.Key, Id = da.Max(dai => dai.Id), name = da.Max(dai => dai.CandName) }).ToListAsync();
                var totCnt = data.Count;
                if (PageCount > 0)
                {
                    var cntData = await dbContext.GetChatRoomsCountAsync(Usr.Id);
                    totCnt = cntData?.RoomsCount ?? 0;
                }

                return new ChatPaginationViewModel
                {
                    Rooms = data.Select(da => new _ChatPaginationViewModel
                    {
                        JobId = da.JobId,
                        JobTitle = jobs.Where(dai => dai.Id == da.JobId).Select(dai => dai.JobTitle).FirstOrDefault(),

                        RoomId = da.RoomId,
                        RoomUpdatedDate = da.RoomUpdatedDate,
                        Count = da.Count,
                        UnreadCount = da.UnreadCount,
                        LatestMessageDt = ConvertUTCToDateByTimezone(da.LatestMessageDt, "GMT+04:00", false),                       


                        ReceiverId = da.ReceiverId,
                        CandidateId = cands.Where(dai => dai.Id == da.ReceiverId).Join(candProfIds, da => da.EmailId, da2 => da2.EmailId, (da, da2) => da2.Id).FirstOrDefault(),
                        ReceiverName = cands.Where(dai => dai.Id == da.ReceiverId).Join(candProfIds, da => da.EmailId, da2 => da2.EmailId, (da, da2) => da2.name).FirstOrDefault(),
                        ReceiverPhoto = cands.Where(dai => dai.Id == da.ReceiverId).Select(dai => getCandidatePhotoUrl(dai.ProfilePhoto, dai.Id)).FirstOrDefault()
                    }).OrderByDescending(da => da.LatestMessageDt).ToList(),
                    TotalCount = totCnt
                };

            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "PageCount:" + PageCount + ", SkipCount:" + SkipCount + ", User:" + Usr?.Id, e);
                throw;
            }
        }

        public async Task<List<GetChatComunicationViewModel>> GetRoomChatAsync(int ChatRoomId)
        {
            logger.SetMethodName(System.Reflection.MethodBase.GetCurrentMethod());
            try
            {
               // logger.Log(LogLevel.Debug, LoggingEvents.GetItem, "ChatRoomId:" + ChatRoomId);

                var Messages = (from chat in dbContext.PhChatMessages
                                orderby chat.CreatedDate ascending
                                where chat.ChatRoomId == ChatRoomId
                                select new GetChatComunicationViewModel
                                {
                                    FileName = chat.FileName,
                                    Message = chat.Message,
                                    ReadStatus = chat.ReadStatus,
                                    ReceiverId = chat.ReceiverId,
                                    SenderId = chat.SenderId,
                                    RoomId = chat.ChatRoomId,
                                    CreatedDate = chat.CreatedDate,
                                    chatMsgId = chat.Id
                                }).ToList();
                foreach (var item in Messages)
                {
                    item.CreatedDate = ConvertUTCToDateByTimezone(item.CreatedDate, "GMT+04:00", false);
                }
                return Messages;
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "ChatRoomId:" + ChatRoomId + ", User:" + Usr?.Id, e);
                throw;
            }
        }
    }
}
