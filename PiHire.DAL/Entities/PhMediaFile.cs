using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhMediaFile
{
    public int Id { get; set; }

    public byte FileGroup { get; set; }

    public string FileType { get; set; }

    public string FileName { get; set; }

    public byte Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
