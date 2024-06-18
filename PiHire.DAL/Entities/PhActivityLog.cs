using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhActivityLog
{
    public int Id { get; set; }

    public byte ActivityType { get; set; }

    public byte ActivityMode { get; set; }

    public int? ActivityOn { get; set; }

    public string ActivityDesc { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? Joid { get; set; }

    public int? CurrentStatusId { get; set; }

    public int? UpdateStatusId { get; set; }
}
