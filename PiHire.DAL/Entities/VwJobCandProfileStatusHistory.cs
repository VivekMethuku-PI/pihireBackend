using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwJobCandProfileStatusHistory
{
    public int Id { get; set; }

    public byte ActivityType { get; set; }

    public byte ActivityMode { get; set; }

    public int? ActivityOn { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? Joid { get; set; }

    public int? CurrentStatusId { get; set; }

    public int? UpdateStatusId { get; set; }
}
