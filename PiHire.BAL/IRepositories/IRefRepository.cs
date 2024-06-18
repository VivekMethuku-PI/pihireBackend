using PiHire.BAL.Common.Types;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IRefRepository : IBaseRepository
    {
        Task<GetResponseViewModel<List<RefMasterViewModel>>> GetRefList();

        Task<GetResponseViewModel<RefMasterViewModel>> GetRefData(int Id);

        Task<GetResponseViewModel<List<RefDataViewModel>>> GetRefData(int[] GroupIds);
        Task<GetResponseViewModel<List<ReferenceDataViewModel>>> GetRefData(ReferenceGroupType referenceGroup);

        Task<CreateResponseViewModel<string>> CreateRefValue(CreateRefValuesViewModel createRefValuesViewModel);

        Task<UpdateResponseViewModel<string>> UpdateRefValue(UpdateRefValuesViewModel updateRefValuesViewModel);

        Task<UpdateResponseViewModel<string>> UpdateTemplateStatus(int Id);
        //Task<GetResponseViewModel<List<ReferenceDataViewModel>>> GetCandidateEducationQualifications_Graduations();
        //Task<GetResponseViewModel<List<ReferenceDataViewModel>>> GetCandidateEducationQualifications_GraduationSpecializations(int graduationId);
    }
}
