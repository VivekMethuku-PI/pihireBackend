using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiAppTasksS
{
    public short Id { get; set; }

    public short ApplicationId { get; set; }

    public short ModuleId { get; set; }

    public string TaskCode { get; set; }

    public string TaskName { get; set; }

    public string TaskDesc { get; set; }

    public bool ActivityFlag { get; set; }

    public string Activities { get; set; }

    public DateTime CreatedDate { get; set; }

    public byte Status { get; set; }
}
