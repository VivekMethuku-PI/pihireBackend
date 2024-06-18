using PiHire.BAL.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IEmployeeRepository : IBaseRepository
    {
        Task<List<EmployeeViewModel>> GetEmployees();
        Task<EmployeeContactViewModel> GetEmployeeContactInfo(int empId);
    }
}
