using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiAppUserRoleMap
{
    public int Id { get; set; }

    public int AppUserId { get; set; }

    public int ApplicationId { get; set; }

    public int AppRoleId { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }
}
