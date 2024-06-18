using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwJobStatusHistory
{
    public int? JobId { get; set; }

    public string StatusCode { get; set; }

    public string OldStatusCode { get; set; }

    public DateTime? ActivityDate { get; set; }
}
