using Microsoft.AspNetCore.Http;
using PiHire.BAL.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IChatComunicationRepository: IBaseRepository
    {
        /// <summary>
        /// Getting all chat messages based on roomId
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<(GetChatMessagesResponseVM data, bool status)> GetChatMessages(GetChatMessagesViewModel model);
        Task<bool> SaveChatMessage(SendMessageViewModel model, IFormFile file, DateTime createdTime);
        Task<List<GetRoomsViewModel>> GetRoomsByJob(GetRoomReuestViewModel model);
        Task<(GetChatMessagesResponseVM data, bool status)> GetAllChatMessages();

        Task<bool> UpdateReadStatus(UpdateReadStatusViewModel model);

        Task<List<GetUnreadCountViewModel>> GetUnreadCount();

        Task<List<RoomMediaViewModel>> GetRoomMedia(int RoomId);


        Task<ChatPaginationViewModel> GetRoomsAsync(int PageCount, int SkipCount);
        Task<List<GetChatComunicationViewModel>> GetRoomChatAsync(int ChatRoomId);
    }
}
