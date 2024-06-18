using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class RecsViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int JobsCount { get; set; }
        public int? LocationId { get; set; }
        public int? ShiftId { get; set; }
        public int puId { get; set; }
        public byte UserStatus { get; set; }
    }


    public class UsersViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public byte Status { get; set; }
        public byte UserType { get; set; }
        public string MobileNo { get; set; }
        public string RoleName { get; set; }
        public string Location { get; set; }
        public int? LocationId { get; set; }
        public int? ShiftId { get; set; }
        public int PuId { get; set; }
        public string Nationality { get; set; }
        public string ProfilePhoto { get; set; }
        public string EmailSignature { get; set; }
    }

   


    public class GetModuleModel
    {
        public short Id { get; set; }
        public string Module { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
    }


}
