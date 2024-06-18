using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhSalarySlabsWiseCompsS
{
    public int Id { get; set; }

    public int Puid { get; set; }

    public int SlabId { get; set; }

    public int CompId { get; set; }

    public decimal Amount { get; set; }

    public bool? PercentageFlag { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
