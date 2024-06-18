using PiHire.BAL.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IWsServiceRepository : IDisposable
    {
        Task<List<ScheduleServiceViewModel>> GetScheduleListSync();
        Task BirthdayAsync(int scheduleId);
        Task EventHappendAsync(int scheduleId);
        Task NewJobPublishedAsync(int scheduleId);
        Task SpecialDayAsync(int scheduleId);

        Task<string> UpdateInfoBipStatus();
        Task<int> TmpAllCandidatesUpdate();
        Task<int> RecruiterJobAssignmentsCarryForwardAsync();
    }
}
