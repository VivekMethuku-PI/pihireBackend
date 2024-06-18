using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateEduDetail
{
    public int Id { get; set; }

    public int CandProfId { get; set; }

    public int QualificationId { get; set; }

    public string Qualification { get; set; }

    public string Course { get; set; }

    public string UnivOrInstitution { get; set; }

    public int? YearofPassing { get; set; }

    public DateTime? DurationFrom { get; set; }

    public DateTime? DurationTo { get; set; }

    public string Grade { get; set; }

    public double? Percentage { get; set; }

    public byte Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? CourseId { get; set; }
}
