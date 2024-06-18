using System;
using System.Text;
using System.Threading.Tasks;
using PiHire.BAL.ViewModel;
using System.Collections.Generic;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Models;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.IRepositories
{
    public interface ICompanyRepository : IBaseRepository
    {
        Task<GetResponseViewModel<List<CompanyViewModel>>> GetCompanies();

        Task<CreateResponseViewModel<int>> CreateUpdateCompany(CreateUpdateProcessUnitViewModel model);

        Task<CreateResponseViewModel<int>> CreateUpdateCompanyBusinessUnit(CreateUpdateProcessUnitBusinessUnitViewModel model);

        Task<CreateResponseViewModel<int>> CreateUpdateCompanyLocation(CreateUpdateProcessUnitLocationViewModel model);

        Task<GetResponseViewModel<CompanyViewModel>> GetCompany(int Id);

        Task<GetResponseViewModel<List<Sp_UserType_LocationsModel>>> GetUserTypeLocationsAsync(UserType? userType);
        Task<GetResponseViewModel<List<PuLocationViewModel>>> GetCompanyLocations(int cId);

        Task<GetResponseViewModel<List<PuLocationsModel>>> GetCompanyLocations(int[] cId);

        Task<GetResponseViewModel<PuLocationViewModel>> GetCompanyLocation(int cId, int locId);

    }
}
