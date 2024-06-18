using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PiEmailServiceProvider
{
    public int Id { get; set; }

    public string RequestUrl { get; set; }

    public string AuthKey { get; set; }

    public string FromName { get; set; }

    public string FromEmailId { get; set; }

    public bool DefaultFlag { get; set; }

    public byte DistType { get; set; }

    public byte Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string ProviderCode { get; set; }
}
