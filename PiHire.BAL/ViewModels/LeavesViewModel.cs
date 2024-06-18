using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class LeavesViewModel
    {
        public int Id { get; set; }
        public int EmpId { get; set; }
        public string EmpName { get; set; }
        public string EmpNameRole { get; set; }
        public string EmpNameProfilePic { get; set; }
        public int LeaveType { get; set; }
        public string LeaveTypeName { get; set; }
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public string LeaveReason { get; set; }
        public byte Status { get; set; }
        public string StatusName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public string ApprovedByRole { get; set; }
        public string ApprovedByName { get; set; }
        public string ApprovedByProfilePic { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApproveRemarks { get; set; }
        public string RejectRemarks { get; set; }
        public DateTime? RejectedDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public string CancelRemarks { get; set; }
    }

    public class UpdateLeaveViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public byte Status { get; set; }
        [MaxLength(500)]
        public string ApproveRemarks { get; set; }
        [MaxLength(500)]
        public string RejectRemarks { get; set; }
        [MaxLength(500)]
        public string CancelRemarks { get; set; }
    }


    public class CreateLeaveViewModel
    {
        [Required]
        public int EmpId { get; set; }
        [Required]
        public int LeaveType { get; set; }
        [Required]
        public DateTime LeaveStartDate { get; set; }
        [Required]
        public DateTime LeaveEndDate { get; set; }
        [Required]
        [MaxLength(5000)]
        public string LeaveReason { get; set; }
        [Required]
        public int ApprovedBy { get; set; }
        [Required]
        public bool LeaveCategory { get; set; }

    }

    public class CreateLeaveInsteadViewModel
    {
        [Required]
        public int EmpId { get; set; }
        [Required]
        public int LeaveType { get; set; }
        [Required]
        public DateTime LeaveStartDate { get; set; }
        [Required]
        public DateTime LeaveEndDate { get; set; }
        [Required]
        [MaxLength(5000)]
        public string LeaveReason { get; set; }      
        public bool LeaveCategory { get; set; }

    }
}
