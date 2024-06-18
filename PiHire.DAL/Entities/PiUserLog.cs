using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiUserLog
{
    public long Id { get; set; }

    public int? ApplicationId { get; set; }

    public int? UserId { get; set; }

    public long? LastTxnId { get; set; }

    public bool? LoginStatus { get; set; }
}
