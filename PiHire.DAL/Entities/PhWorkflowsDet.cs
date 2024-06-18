using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhWorkflowsDet
{
    public short Id { get; set; }

    public int WorkflowId { get; set; }

    public byte ActionType { get; set; }

    public int? CurrentStatusId { get; set; }

    public int? UpdateStatusId { get; set; }

    public int? AsmtOrTplId { get; set; }

    public byte? SendMode { get; set; }

    public byte? SendTo { get; set; }

    public byte Status { get; set; }

    public string DocsReqstdIds { get; set; }
}
