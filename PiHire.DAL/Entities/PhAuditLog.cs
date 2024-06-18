using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhAuditLog
{
    public int Id { get; set; }

    public byte ActivityType { get; set; }

    public int? TaskId { get; set; }

    public string ActivitySubject { get; set; }

    public string ActivityDesc { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }
}
