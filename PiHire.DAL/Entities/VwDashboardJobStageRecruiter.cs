using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwDashboardJobStageRecruiter
{
    public int JobId { get; set; }

    public int? BdmId { get; set; }

    public int? RecruiterId { get; set; }

    public DateTime PostedDate { get; set; }

    public string RecrName { get; set; }
}
