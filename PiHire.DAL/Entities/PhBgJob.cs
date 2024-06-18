using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhBgJob
{
    public int Id { get; set; }

    public string EventType { get; set; }

    public string Title { get; set; }

    public string JobDesc { get; set; }

    public byte Frequency { get; set; }

    public DateTime? ScheduleDate { get; set; }

    public string ScheduleTime { get; set; }

    public int EmailTemplateId { get; set; }

    public byte SendTo { get; set; }

    public string Pus { get; set; }

    public string Bus { get; set; }

    public string CandidateStatus { get; set; }

    public string Gender { get; set; }

    public string CountryIds { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<PhBgJobDetail> PhBgJobDetails { get; } = new List<PhBgJobDetail>();
}
