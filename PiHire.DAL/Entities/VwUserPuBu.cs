using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwUserPuBu
{
    public int UserId { get; set; }

    public int ProcessUnit { get; set; }

    public int? BusinessUnit { get; set; }
}
