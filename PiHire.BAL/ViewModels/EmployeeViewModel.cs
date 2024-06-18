using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class EmployeeContactViewModel
    {
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
    }

    public class CtoEResponseModel
    {
        public bool Success { get; set; }
        public int EmployeeId { get; set; }
        public string Message { get; set; }
    }


    public class OddoLoginResponseViewModel
    {
        public int count { get; set; }
        public OddoLoginResponseDataViewModel data { get; set; }
    }


    public class OddoLoginResponseDataViewModel
    {
        public int contract_id { get; set; }
        public int emp_id { get; set; }
        public string email { get; set; }
        public string message { get; set; }
        public bool status { get; set; }
    }
}
