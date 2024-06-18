using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateDoc
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int CandProfId { get; set; }

    public byte FileGroup { get; set; }

    public string DocType { get; set; }

    public string FileType { get; set; }

    public string FileName { get; set; }

    public int? UploadedBy { get; set; }

    public int Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public byte DocStatus { get; set; }

    public string Remerks { get; set; }
}
