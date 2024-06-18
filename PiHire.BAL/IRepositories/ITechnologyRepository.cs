using PiHire.BAL.ViewModel;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PiHire.DAL.Models;

namespace PiHire.BAL.IRepositories
{
    public interface ITechnologyRepository : IBaseRepository
    {

        #region Technology



        /// <summary>
        /// get Technologies
        /// </summary>       
        /// <returns></returns>
        Task<GetResponseViewModel<List<TechnologiesModel>>> GetTechnologies();

        /// <summary>
        /// get Technologies
        /// </summary>       
        /// <returns></returns>
        Task<GetResponseViewModel<TechnologyModel>> GetTechnologies(GetTechnologiesViewModel getTechnologiesViewModel);

        /// <summary>
        /// Create Technology
        /// </summary>       
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateTechnology(CreateTechnologiesViewModel createTechnologiesViewModel);

        /// <summary>
        /// Update Technology
        /// </summary>       
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateTechnology(UpdateTechnologiesViewModel updateTechnologiesViewModel);

        /// <summary>
        /// Delete Technology
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<DeleteResponseViewModel<string>> DeleteTechnology(int Id);

        #endregion


        #region Technology Group 

        /// <summary>
        /// Technology Groups
        /// </summary>       
        /// <returns></returns>
        Task<GetResponseViewModel<List<TechnologiesViewModel>>> GetTechnologyGroup(int id);

        /// <summary>
        /// Technology Group
        /// </summary>       
        /// <returns></returns>
        Task<GetResponseViewModel<List<SkillProfilesViewModel>>> TechnologyGroups();

        /// <summary>
        /// Create Technology 
        /// </summary>       
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateTechnologyGroup(CreateSkillProfileViewModel createSkillProfileViewModel);

        /// <summary>
        /// Update Technology Group
        /// </summary>       
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateTechnologyGroup(UpdateSkillProfileViewModel updateSkillProfileViewModel);

        /// <summary>
        /// Delete Technology Group 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<DeleteResponseViewModel<string>> DeleteTechnologyGroup(int Id);

        #endregion

    }
}
