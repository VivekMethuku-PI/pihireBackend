using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IWorkflowRepository : IBaseRepository
    {

        /// <summary>
        /// Get work flow rule list
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<WorkflowRuleViewmodel>>> GetWorkflowRules(byte sort);

        /// <summary>
        /// Get work flow rule
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<EditWorkflowRuleViewmodel>> GetWorkflowRule(int Id);

        /// <summary>
        /// Create work flow rule
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateWorkflowRule(CreateWorkflowRuleViewmodel CreateWorkflowRuleViewmodel);

        /// <summary>
        /// Create work flow rule
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateWorkflowRule(EditWorkflowRuleViewmodel editWorkflowRuleViewmodel);

        /// <summary>
        /// Delete work flow rule
        /// </summary>
        /// <returns></returns>
        Task<DeleteResponseViewModel<string>> DeleteWorkflowRule(int Id);


        /// <summary>
        /// Delete work flow details rule
        /// </summary>
        /// <returns></returns>
        Task<DeleteResponseViewModel<string>> DeleteWorkflowDetails(int Id);
    }
}
