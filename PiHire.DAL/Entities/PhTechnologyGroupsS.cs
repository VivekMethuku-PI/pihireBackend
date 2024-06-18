using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhTechnologyGroupsS
{
    public short Id { get; set; }

    public int TechnologyId { get; set; }

    public int TechnologyGroupId { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
