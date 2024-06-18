using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class TblParamProcessUnitMaster
{
    public int Id { get; set; }

    public string PuName { get; set; }

    public string ShortName { get; set; }

    public DateTime DateOfEstablisment { get; set; }

    public int City { get; set; }

    public string State { get; set; }

    public int Country { get; set; }

    public string TimeZone { get; set; }

    public string IsoCode { get; set; }

    public string MobileNumber { get; set; }

    public string Website { get; set; }

    public string Logo { get; set; }

    public string Latitude { get; set; }

    public string Longitude { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string VatNumber { get; set; }

    public string ColumnTwo { get; set; }

    public int? ColumnThree { get; set; }

    public int? ColumnFour { get; set; }

    public DateTime? ColumnFive { get; set; }

    public string PanNo { get; set; }

    public string GstNo { get; set; }

    public string ServiceTaxNo { get; set; }

    public string TinNo { get; set; }

    public string TanNo { get; set; }

    public string PayslipEmail { get; set; }
}
