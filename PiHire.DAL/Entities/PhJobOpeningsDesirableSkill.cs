using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobOpeningsDesirableSkill
{
    public int Id { get; set; }
    public int Joid { get; set; }
    public byte GroupType { get; set; }
    public int TechnologyId { get; set; }
    public int ExpYears { get; set; }
    public byte? PreferenceType { get; set; }
    public byte Status { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
