using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhTestimonial
{
    public int Id { get; set; }

    public int? CandidateId { get; set; }

    public string Title { get; set; }

    public string Tdesc { get; set; }

    public string ProfilePic { get; set; }

    public byte Rating { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string Designation { get; set; }
}
