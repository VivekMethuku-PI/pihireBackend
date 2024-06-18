using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class TblParamRoleMaster
{
    public int Id { get; set; }

    public string Role { get; set; }

    public string Description { get; set; }

    public int? DepartmentId { get; set; }
}
