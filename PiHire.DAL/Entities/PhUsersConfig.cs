using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhUsersConfig
{
    public int Id { get; set; }

    public string UserName { get; set; }

    public string PasswordHash { get; set; }

    public bool VerifyFlag { get; set; }

    public string VerifyToken { get; set; }

    public byte Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public int? UserId { get; set; }
}
