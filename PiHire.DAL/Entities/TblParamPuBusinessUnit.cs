using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class TblParamPuBusinessUnit
{
    public int Id { get; set; }

    public int PuId { get; set; }

    public string BusUnitFullName { get; set; }

    public string BusUnitCode { get; set; }

    public string Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string ColumnOne { get; set; }

    public string ColumnTwo { get; set; }

    public int? ColumnThree { get; set; }

    public int? ColumnFour { get; set; }

    public DateTime? ColumnFive { get; set; }
}
