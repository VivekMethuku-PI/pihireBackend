using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiAppUserRole
{
    public int Id { get; set; }

    public short ApplicationId { get; set; }

    public string RoleName { get; set; }

    public string RoleDesc { get; set; }

    public byte UserType { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }
}
