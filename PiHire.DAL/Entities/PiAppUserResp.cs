using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiAppUserResp
{
    public int Id { get; set; }

    public int AppUserId { get; set; }

    public int ApplicationId { get; set; }

    public short ModuleId { get; set; }

    public short TaskId { get; set; }

    public string Permissions { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }
}
