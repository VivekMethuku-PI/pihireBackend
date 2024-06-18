using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface ICustomSchedulerRepository:IBaseRepository
    {
        Task<CreateResponseViewModel<List<BgJobSummaryViewModel>>> getSummariesAsync();
        Task<CreateResponseViewModel<List<BgJobSummaryViewModel>>> getSummariesAsync(int JobId);

        Task<CreateResponseViewModel<string>> setCustomSchedulerAsync(BgJobSummaryViewModel model);
        Task<CreateResponseViewModel<string>> setCustomSchedulerAsync(CreateJobBgViewModel jobBgViewModel);
        Task<CreateResponseViewModel<string>> deactiveCustomSchedulerAsync(int bgJobId);
        Task<CreateResponseViewModel<string>> activeCustomSchedulerAsync(int bgJobId);
        Task<CreateResponseViewModel<string>> deleteCustomSchedulerAsync(int bgJobId);
    }
}
