using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IOffersRepository : IBaseRepository
    {
        #region SALARY SLAB

        Task<GetResponseViewModel<List<SlabViewModel>>> GetSlabs();

        Task<GetResponseViewModel<SlabViewModel>> GetSlab(int Id);

        Task<CreateResponseViewModel<string>> CreateSlab(CreateSlabViewModel createSlabViewModel);

        Task<UpdateResponseViewModel<string>> UpdateSlab(UpdateSlabViewModel updateSlabViewModel);

        Task<DeleteResponseViewModel<string>> DeleteSlab(int Id);


        #endregion

        #region SALARY COMPONENT

        Task<CreateResponseViewModel<string>> CreateSalaryComponet(CreateSalayComponentViewModel createSalayComponentViewModel);

        Task<UpdateResponseViewModel<string>> UpdateSalaryComponet(UpdateSalayComponentViewModel updateSalayComponentViewModel);

        Task<DeleteResponseViewModel<string>> DeleteSalaryComponet(int Id);

        Task<GetResponseViewModel<SalayComponentViewModel>> GetSalaryComponet(int Id);

        Task<GetResponseViewModel<List<SalayComponentViewModel>>> GetSalaryComponets();


        #endregion

        #region  SALARY SLABS WISE COMPS

        Task<CreateResponseViewModel<string>> CreateSlabComponet(CreateSlabComponentViewModel createSlabComponentViewModel);

        Task<UpdateResponseViewModel<string>> UpdateSlabComponet(UpdateSlabComponentViewModel updateSlabComponentViewModel);

        Task<DeleteResponseViewModel<string>> DeleteSlabComponet(int Id);

        Task<GetResponseViewModel<SlabComponentViewModel>> GetSlabComponet(int Id);

        Task<GetResponseViewModel<List<SlabComponentViewModel>>> GetSlabComponets();


        #endregion

        #region OFFER LETTERS 

        Task<GetResponseViewModel<List<GrpBySlabModel>>> GetSlabComponetDtls(int Id,int PuId);

        Task<CreateResponseViewModel<string>> CreateOfferLetterWithSlab(CreateOfferLetterSlabViewModel createOfferLetterSlabViewModel);

        Task<CreateResponseViewModel<string>> CreateOfferLetterWithOutSlab(CreateOfferLetterWithSlabViewModel createOfferLetterWithSlabViewModel);

        Task<GetResponseViewModel<List<JobOfferLetterViewModel>>> GetJobOfferLetter(int JobId,int CanProfId);

        Task<GetResponseViewModel<FileURLViewModel>> DownloadOfferLetter(int Id, bool Islogo);

        //Task<HtmlMessageBodyViewModel> DownloadIntentLetter(int Id, bool Islogo);

        Task<Tuple<List<NotificationPushedViewModel>, GetResponseViewModel<string>>> ReleaseIntentOffer(ReleaseIntentOfferViewModel releaseIntentOfferViewModel);

        #endregion

        #region OFFERS (PRE JOINS + ON BOARDS)
        Task<GetResponseViewModel<OfferedCandidatesModel>> GetOfferdCandidates(OfferdCandidateSearchModel offerdCandidateSearchModel);

        #endregion
    }
}
