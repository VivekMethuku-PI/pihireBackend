using PiHire.BAL.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface INotificationRepository: IBaseRepository
    {
        Task<List<UserNotificationsViewModel>> GetUserNotifications();
        Task<(string Message, bool Status)> SaveNotification(List<NotificationPushedViewModel> model);
        Task<List<string>> GetConnectionsIds(int[] userIds);
        Task<(string Message, bool Status)> UpdateReadStatus();

    }
}
