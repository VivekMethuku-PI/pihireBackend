using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Repositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Entities;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.IRepositories
{
    public interface ICommonRepository : IBaseRepository
    {
        #region  - PU & BU
        /// <summary>
        /// returning process unit list
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<GetPUViewModel>>> GetPUAsync();

        /// <summary>
        /// returning bussiness unit list
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<GetResponseViewModel<List<GetBUViewModel>>> GetBUAsync(GetBURequestVM model);


        /// <summary>
        /// returning process unit list
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<GetUserPUViewModel>>> UserPuListAsync();

        /// <summary>
        /// returning bussiness unit list
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<GetResponseViewModel<List<GetBUViewModel>>> UserBuListAsync(GetBURequestVM model);

        #endregion
        Task<GetResponseViewModel<List<SelectWithCodeViewModel>>> GetJobStatusAsync();

        #region - User Remarks 

        Task<GetResponseViewModel<List<UserRemarksViewModel>>> GetUserRemarksList(UserRemarksRequestModel userRemarksRequestModel);
        Task<CreateResponseViewModel<string>> CreateUserRemark(CreateUserRemarksModel createUserRemarksModel);
        Task<UpdateResponseViewModel<string>> UpdateUserRemark(UpdateUserRemarksModel updateUserRemarksModel);
        Task<UpdateResponseViewModel<string>> DeleteUserRemark(int Id);

        #endregion

        #region Timezones
        Task<GetResponseViewModel<List<Common.Meeting.TeamCalendarTimeZoneViewModel_value>>> getTeamTimeZones();
        Task<GetResponseViewModel<List<Common.Meeting.TeamCalendarTimeZoneViewModel_value>>> getGoogleTimeZones();
        #endregion

        /// <summary>
        /// returning enum values 
        /// </summary>
        /// <returns></returns>
        GetResponseViewModel<CommonEnumViewModel> GetTypes();

        /// <summary>
        /// returning ref data list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<RefDataViewModel>>> GetRefData(int[] GroupId);


        /// <summary>
        /// returning ref data list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<DocTypesViewModel>>> DocTypes(int[] GroupId);


        /// <summary>
        /// returning countries data list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<CountryModel>>> GetCountries();

        /// <summary>
        /// returning cities data list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<CityModel>>> GetCities(int CountryId);

        /// <summary>
        /// multi countries ids 
        /// </summary>
        /// <param name="CountryIds"></param>
        /// <returns></returns>
        Task<GetResponseViewModel<List<CityModel>>> GetCities(int?[] CountryIds);

        Task<GetResponseViewModel<List<SalaryCalculatorCountryViewModel>>> SalaryCalculatorCountriesAsync(bool IsFrom = false, bool IsTo = false, int? FromCountryId = null);
        Task<GetResponseViewModel<List<string>>> GetCityWiseBenefitsAsync(int CityId);
        Task<GetResponseViewModel<List<CityWiseBenefitViewModel>>> GetCityWiseBenefitListAsync(int CityId);
        Task<GetResponseViewModel<string>> SetCityWiseBenefitsAsync(CityWiseBenefitViewModel model);

        Task<GetResponseViewModel<List<string>>> GetCountryWiseBenefitsAsync(int CountryId, bool? isSalaryWise);
        Task<GetResponseViewModel<List<CountryWiseBenefitViewModel>>> GetCountryWiseBenefitListAsync(int CountryId, bool? isSalaryWise);
        Task<GetResponseViewModel<string>> SetCountryWiseBenefitsAsync(CountryWiseBenefitViewModel model);

        Task<GetResponseViewModel<List<CountryWiseAllowanceDetailViewModel>>> GetCountryWiseAllowancesAsync(int CountryId, bool? IsCitizenWise);
        Task<GetResponseViewModel<List<CountryWiseAllowanceViewModel>>> GetCountryWiseAllowanceListAsync(int CountryId, bool? IsCitizenWise);
        Task<GetResponseViewModel<string>> SetCountryWiseAllowancesAsync(CountryWiseAllowanceViewModel model);


        /// <summary>
        /// returning client data list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<ClientViewModel>>> GetClients();

        /// <summary>
        /// returning spocs data list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<ClientSpocsModel>>> GetSpocs(int ClientId);

        /// <summary>
        /// returning Assessment list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<AssessmentViewModel>>> GetAssessments();

        /// <summary>
        /// returning media list 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<MediaFilesViewModel>>> GetMediaFiles();

        /// <summary>
        /// Upload file 
        /// </summary>
        /// <param name="createMediaViewModel"></param>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> UploadMediaFile(CreateMediaViewModel createMediaViewModel);


        #region SMTP Config
        Task<GetResponseViewModel<ConfiguredSmptViewModel>> GetConfiguredUserMailDetails(int UserId);
        Task<CreateResponseViewModel<string>> ConfigureUserMail(ConfigureSmptMailViewModel configureSmptMailViewModel);
        //Task<CreateResponseViewModel<string>> UserMailConfigureSuccess(UserMailConfigureSuccessViewModel UserMailConfigureSuccessViewModel);
        //Task<GetResponseViewModel<string>> GetOutlookTokenAsync();
        #endregion


        #region Skill Search 

        GetResponseViewModel<List<SearchKeyModel>> GetSearchListModel();

        GetResponseViewModel<SearchKeyModel> GetSkillDtls(int Id);


        #endregion


        #region Audit dtls 

        Task<GetResponseViewModel<AuditModel>> GetAuditList(AuditListSearchViewModel audit);

        Task<GetResponseViewModel<ActivitesModel>> GetActivitiesList(ActivityListSearchViewModel activity);

        #endregion


        #region Integrations 
        Task<GetResponseViewModel<List<IntegrationsViewModel>>> GetIntegrationList(byte Category);
        Task<UpdateResponseViewModel<IntegrationStatusRespViewModel>> UpdateIntegrationStatus(int[] Id, RecordStatus Status);
        Task<UpdateResponseViewModel<IntegrationReqStatusViewModel>> GetUpdateIntegrationStatus(IntegrationCategory category, int subCategory);

        #region Google meet
        Task<string> SetGoogleMeetToken(string access_token, string code, string state);
        Task<CreateResponseViewModel<string>> CheckGoogleMeetCalendarTokenExist();
        #endregion

        #region Team Calendar
        Task<string> SetTeamCalendarToken(string code, string state, string session_state, string error);
        Task<CreateResponseViewModel<string>> CheckTeamCalendarTokenExist();
        #endregion

        #endregion



    }
}
