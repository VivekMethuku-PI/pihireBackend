using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhRefMaster
{
    public short Id { get; set; }

    public string Rmtype { get; set; }

    public string Rmvalue { get; set; }

    public string Rmdesc { get; set; }

    public int GroupId { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
