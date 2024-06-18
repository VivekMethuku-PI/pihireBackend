using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandReofferDetailsTemp
{
    public short Id { get; set; }

    public string Opcurrency { get; set; }

    public int OpgrossPayPerMonth { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int JoId { get; set; }

    public int CandProfId { get; set; }
}
