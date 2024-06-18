using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhSalaryComp
{
    public int Id { get; set; }

    public int CompType { get; set; }

    public string Title { get; set; }

    public string CompDesc { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
