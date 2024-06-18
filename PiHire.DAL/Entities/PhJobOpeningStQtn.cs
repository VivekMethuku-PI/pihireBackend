using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobOpeningStQtn
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public byte QuestionType { get; set; }

    public string QuestionText { get; set; }

    public byte QuestionSlno { get; set; }

    public bool? IsMandatory { get; set; }

    public byte Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
