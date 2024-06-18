using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PiHire.BAL.Common.Http;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Common.Types;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        readonly Logger logger;
        public EmployeeRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<EmployeeRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        public async Task<EmployeeContactViewModel> GetEmployeeContactInfo(int empId)
        {
            int UserId = Usr.Id;
            try
            {
                EmployeeContactViewModel contact = null;
                using var client = new HttpClientService();
                var response = client.Get(appSettings.AppSettingsProperties.GatewayUrl, "/api/GWService/employee/GetOfficeContact/" + empId);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    contact = JsonConvert.DeserializeObject<EmployeeContactViewModel>(responseContent);
                }

                return contact;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<EmployeeViewModel>> GetEmployees()
        {
            int UserId = Usr.Id;
            try
            {
                List<EmployeeViewModel> employees = null;
                using var client = new HttpClientService();
                var response = client.Get(appSettings.AppSettingsProperties.GatewayUrl, "/api/GWService/employee/GetEmployees");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    employees = JsonConvert.DeserializeObject<List<EmployeeViewModel>>(responseContent);
                    var piHireEmp = dbContext.PiHireUsers.Where(s => s.Status != (byte)RecordStatus.Delete && s.UserType != (byte)UserType.Candidate && s.EmployId.HasValue).Select(s => s.EmployId.Value).ToList();
                    employees = employees.Where(s => !piHireEmp.Contains(s.Id)).OrderBy(o => o.FirstName).ToList();
                }

                return employees;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
