using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhEducationQualificationMaster
{
    public int Id { get; set; }
    public byte GroupType { get; set; }
    public int? GroupId { get; set; }

    public string Title { get; set; }
    public string Desc { get; set; }

    public byte Status { get; set; }
    public DateTime CreatedDate { get; set; }
}
