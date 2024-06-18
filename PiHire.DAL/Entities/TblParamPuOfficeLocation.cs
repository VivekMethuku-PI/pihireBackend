using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class TblParamPuOfficeLocation
{
    public int Id { get; set; }

    public int PuId { get; set; }

    public string LocationName { get; set; }

    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string Address3 { get; set; }

    public string LandMark { get; set; }

    public string ContactPerson { get; set; }

    public string LandNumber { get; set; }

    public string MobileNumber { get; set; }

    public string FaxNo { get; set; }

    public string EmailAddress { get; set; }

    public int City { get; set; }

    public string State { get; set; }

    public int Country { get; set; }

    public string Pin { get; set; }

    public string Website { get; set; }

    public string LocationMap { get; set; }

    public string Latitude { get; set; }

    public string Longitude { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string LocationMapName { get; set; }

    public string LocationMapType { get; set; }

    public int? ColumnThree { get; set; }

    public int? ColumnFour { get; set; }

    public DateTime? ColumnFive { get; set; }

    public bool? IsMainLocation { get; set; }
}
