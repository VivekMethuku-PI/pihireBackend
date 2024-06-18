using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IAutomationRepository : IBaseRepository
    {
        #region - Candidate 
        /// <summary>
        /// returns candidate status list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<CandidateStatusListViewModel>>> GetCandidateStatus();


        /// <summary>
        /// create candidate status 
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateCandidateStatus(CreateCandidateStatusViewModel createCandidateStatusViewModel);

        /// <summary>
        /// edit candidate status 
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> EditCandidateStatus(EditCandidateStatusViewModel editCandidateStatusViewModel);


        /// <summary>
        /// update candidate status 
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateCandidateStatus(UpdateCandidateStatusViewModel updateCandidateStatusViewModel);

        #endregion

        #region - Job 

        /// <summary>
        /// returns opening status list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<OpeningStatusListViewModel>>> GetOpeningStatus();


        /// <summary>
        /// create candidate status 
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateOpeningStatus(CreateOpeningStatusViewModel createOpeningStatusViewModel);


        /// <summary>
        /// edit candidate status 
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> EditOpeningStatus(EditOpeningStatusViewModel editOpeningStatusViewModel);



        /// <summary>
        /// update candidate status 
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateOpeningStatus(UpdateOpeningStatusViewModel updateOpeningStatusViewModel);
        #endregion

        #region - Pipeline 


        /// <summary>
        /// create stage 
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateStage(CreateStageViewModel createStageViewModel);


        /// <summary>
        /// edit stage
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> EditStage(EditStageViewModel editStageViewModel);



        /// <summary>
        /// update stage
        /// </summary>
        /// <returns></returns>
        Task<DeleteResponseViewModel<string>> DeleteStage(int Id);


        /// <summary>
        /// returns stage status list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<StageViewModel>>> Stages();


        /// <summary>
        /// returns stage  candidate status list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<StageStatusListViewModel>>> StageCandidateStatus();


        /// <summary>
        /// update stage candidate status
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> DeleteStageCandidateStatus(int Id);


        /// <summary>
        /// map candidate status to stage 
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> MapCandidateStatus(MapCandidateStatusViewModel mapCandidateStatusViewModel);

        #endregion

        #region - DisplayOrder 

        /// <summary>
        /// delete display rule
        /// </summary>
        /// <returns></returns>
        Task<DeleteResponseViewModel<string>> DeleteDisplayRule(int StatusId);

        /// <summary>
        /// update display rule
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateDisplayRule(UpdateDisplayRuleViewmodel updateDisplayRuleViewmodel);

        /// <summary>
        /// edit display rule
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> EditDisplayRule(EditDisplayRuleViewmodel editDisplayRuleViewmodel);

        /// <summary>
        /// create display rule
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateDisplayRule(CreateDisplayRuleViewmodel createDisplayRuleViewmodel);

        /// <summary>
        /// returns display rules list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<DisplayRuleViewmodel>>> DisplayRules();

        Task<GetResponseViewModel<List<NextCandidateStatusViewmodel>>> GetNextCandidateStatus(int StatusId);

        #endregion
    }
}
