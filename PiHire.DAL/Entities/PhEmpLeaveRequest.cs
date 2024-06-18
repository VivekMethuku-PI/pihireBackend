using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhEmpLeaveRequest
{
    public int Id { get; set; }

    public int EmpId { get; set; }

    public int LeaveType { get; set; }

    public DateTime LeaveStartDate { get; set; }

    public DateTime LeaveEndDate { get; set; }

    public string LeaveReason { get; set; }

    public byte Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string ApproveRemarks { get; set; }

    public string RejectRemarks { get; set; }

    public DateTime? RejectedDate { get; set; }

    public DateTime? CancelDate { get; set; }

    public string CancelRemarks { get; set; }

    public bool LeaveCategory { get; set; }
}
