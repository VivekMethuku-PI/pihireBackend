using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class FileMigratationLog
{
    public string TblName { get; set; }

    public int? JobId { get; set; }

    public int? CandProfId { get; set; }

    public string FileName { get; set; }

    public bool? IsSuccess { get; set; }

    public string Msg { get; set; }
}
