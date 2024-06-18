using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhIntegrationsS
{
    public int Id { get; set; }

    public byte Category { get; set; }

    public string Title { get; set; }

    public string Logo { get; set; }

    public string InteDesc { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string Account { get; set; }

    public string Token { get; set; }

    public string RefreshToken { get; set; }

    public string ReDirectUrl { get; set; }

    public byte? SubCategory { get; set; }

    public byte? QtyOrPeriodFlag { get; set; }

    public int? Quantity { get; set; }

    public short? ValiPeriod { get; set; }

    public int? Price { get; set; }
}
