using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateSkillset
{
    public int Id { get; set; }

    public int CandProfId { get; set; }

    public short SkillLevelId { get; set; }

    public string SkillLevel { get; set; }

    public int TechnologyId { get; set; }

    public int ExpInYears { get; set; }

    public int ExpInMonths { get; set; }

    public byte SelfRating { get; set; }

    public int Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
