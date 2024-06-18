using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiHireUser
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public byte UserType { get; set; }

    public int? EmployId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MobileNumber { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public string PasswordHash { get; set; }

    public string UserName { get; set; }

    public int? UserRoleId { get; set; }

    public string UserRoleName { get; set; }

    public int? LocationId { get; set; }

    public string Location { get; set; }

    public string EmailSignature { get; set; }

    public bool VerifiedFlag { get; set; }

    public string VerifyToken { get; set; }

    public DateTime? TokenExpiryDate { get; set; }

    public string EmailId { get; set; }

    public string Nationality { get; set; }

    public DateTime? Dob { get; set; }

    public int? ShiftId { get; set; }

    public string ProfilePhoto { get; set; }
}
