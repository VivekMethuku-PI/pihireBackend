using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobOpeningActvCounter
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int? AsmtCounter { get; set; }

    public int? EmailsCounter { get; set; }

    public int? JobPostingCounter { get; set; }

    public int? ClientViewsCounter { get; set; }

    public byte Status { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
