using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobOfferAllowance
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int CandProfId { get; set; }

    public int JobOfferId { get; set; }

    public string AllowanceTitle { get; set; }

    public decimal Amount { get; set; }

    public byte Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }
}
