using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhMessageTemplate
{
    public int Id { get; set; }

    public string Code { get; set; }

    public byte MessageType { get; set; }

    public byte ProfileType { get; set; }

    public string TplTitle { get; set; }

    public string TplDesc { get; set; }

    public string TplSubject { get; set; }

    public byte SentBy { get; set; }

    public string TplBody { get; set; }

    public string DynamicLabels { get; set; }

    public bool PublishStatus { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string TplFullBody { get; set; }

    public int? IndustryId { get; set; }
}
