using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiAppModulesS
{
    public short Id { get; set; }

    public short ApplicationId { get; set; }

    public string ModuleName { get; set; }

    public string ModuleDesc { get; set; }

    public byte Status { get; set; }

    public string ModuleCode { get; set; }
}
