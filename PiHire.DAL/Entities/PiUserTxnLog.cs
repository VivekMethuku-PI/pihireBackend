using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiUserTxnLog
{
    public long Id { get; set; }

    public int? ApplicationId { get; set; }

    public int? UserId { get; set; }

    public string SessionId { get; set; }

    public string Ipaddress { get; set; }

    public string Lat { get; set; }

    public string Long { get; set; }

    public string DeviceUid { get; set; }

    public byte? DeviceType { get; set; }

    public DateTime TxnStartDate { get; set; }

    public DateTime? TxnOutDate { get; set; }

    public string TxnDesc { get; set; }

    public int Status { get; set; }
}
