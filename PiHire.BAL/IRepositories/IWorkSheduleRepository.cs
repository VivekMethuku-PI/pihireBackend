using PiHire.BAL.Repositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IWorkSheduleRepository : IBaseRepository
    {

        #region  Leaves

        /// <summary>
        /// returns  leaves
        /// </summary>       
        /// <returns></returns>
        Task<GetResponseViewModel<List<LeavesViewModel>>> GetLeaves();

        /// <summary>
        /// returns leave
        /// </summary>       
        /// <returns></returns>
        Task<GetResponseViewModel<LeavesViewModel>> GetLeave(int id);


        /// <summary>
        /// create leave 
        /// </summary>       
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateLeave(CreateLeaveViewModel createLeaveViewModel);

        Task<CreateResponseViewModel<string>> CreateLeaveInstead(CreateLeaveInsteadViewModel createLeaveViewModel);

        /// <summary>
        /// update leave
        /// </summary>       
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateLeave(UpdateLeaveViewModel updateLeaveViewModel);

        #endregion

        #region WorkShifts


        /// <summary>
        /// returns Work shifts
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<WorkShiftViewModel>>> GetWorkShifts();

        /// <summary>
        /// returns Work shift
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<WorkShiftViewModel>> GetWorkShift(int Id);


        /// <summary>
        /// Create work shift
        /// </summary>
        /// <param name="createWorkShiftDtlsViewModel"></param>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateWorkShifts(CreateWorkShiftDtlsViewModel createWorkShiftDtlsViewModel);


        /// <summary>
        /// Update work shift
        /// </summary>
        /// <param name="createWorkShiftDtlsViewModel"></param>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateWorkShifts(UpdateWorkShiftDtlsViewModel updateWorkShiftDtlsViewModel);

        Task<UpdateResponseViewModel<string>> UpdateWorkShiftStatus(int Id, byte status);

        #endregion

        #region WorkSchedule 

        Task<GetResponseViewModel<List<WorkScheduleDtlsViewModel>>> GetWorkScheduleDtls(WorkScheduleSearchViewModel workScheduleSearchViewModel);

        #endregion

    }
}
