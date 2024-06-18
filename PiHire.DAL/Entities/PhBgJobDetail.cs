using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhBgJobDetail
{
    public int Id { get; set; }

    public int Bjid { get; set; }

    public DateTime ExecutedOn { get; set; }

    public int SendCount { get; set; }

    public int DeliveredCount { get; set; }

    public int ServiceProviderId { get; set; }

    public string BulkReferenceId { get; set; }

    public int? JobId { get; set; }

    public string Remarks { get; set; }

    public byte ExecutionStatus { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual PhBgJob Bj { get; set; }
}
