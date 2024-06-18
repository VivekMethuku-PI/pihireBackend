using PiHire.BAL.Repositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PiHire.BAL.IRepositories
{
    public interface ITemplateRepository : IBaseRepository
    {
        /// <summary>
        /// returning templates
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<TemplatesViewModel>>> GetTemplateList(TemplateSerachViewModel templateSerachViewModel);


        /// <summary>
        /// returning template
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<TemplatesViewModel>> GetTemplate(int tempId);


        /// <summary>
        /// creating template
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateTemplate(CreateJobTemplateViewModel createJobTemplateViewModel);



        /// <summary>
        /// updating template
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateTemplate(UpdateJobTemplateViewModel updateJobTemplateViewModel);


        /// <summary>
        /// unpublishing template
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UnPublishTemplate(UpdateJobTemplateStatus updateJobTemplateStatus);


        /// <summary>
        /// publishing template 
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> PublishTemplate(UpdateJobTemplateStatus updateJobTemplateStatus);


        /// <summary>
        /// updating template status
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateTemplateStatus(UpdateJobTemplateStatus updateJobTemplateStatus);
    }
}
