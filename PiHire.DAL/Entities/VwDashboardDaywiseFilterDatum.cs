using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwDashboardDaywiseFilterDatum
{
    public int JobId { get; set; }

    public int? BdmId { get; set; }

    public int RecruiterId { get; set; }

    public short? NoCvsrequired { get; set; }

    public short? NoCvsuploadded { get; set; }

    public DateTime? JobAssignmentDate { get; set; }

    public short? NoOfFinalCvsFilled { get; set; }

    public int? Puid { get; set; }

    public int? Buid { get; set; }

    public DateTime ClosedDate { get; set; }
}
