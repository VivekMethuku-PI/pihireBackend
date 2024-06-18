using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhNotificationsUser
{
    public int Id { get; set; }

    public int NotIid { get; set; }

    public int PushTo { get; set; }

    public byte NotiStatus { get; set; }

    public byte Status { get; set; }
}
