using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{

    public class CandidateInterviewRejectModel
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CanPrfId { get; set; }
        public int? CanShareId { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
        [MaxLength(500)]
        public string Reasons { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public byte ScheduledBy { get; set; }
        public int InterviewId { get; set; }
    }

    public class UpdateShareProfileTimeViewModel
    {
        [Required]
        public int CanShareId { get; set; }
        [Required]
        public byte ModeofInterview { get; set; }
        [Required]
        public DateTime InterviewDate { get; set; }
        [Required]
        [MaxLength(15)]
        public string InterviewStartTime { get; set; }
        [Required]
        [MaxLength(15)]
        public string InterviewEndTime { get; set; }
        public string InterviewTimeZone { get; set; }
    }

    public class ScheduleCandidateInterview
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CanPrfId { get; set; }
        [Required]
        public byte ModeofInterview { get; set; }
        [Required]
        public DateTime InterviewDate { get; set; }
        [Required]
        [MaxLength(15)]
        public string InterviewStartTime { get; set; }
        [Required]
        [MaxLength(15)]
        public string InterviewEndTime { get; set; }
        [Required]
        public byte InterviewDuration { get; set; }
        [Required]
        [MaxLength(1000)]
        public string HirePannel { get; set; }
        [MaxLength(1000)]
        public string ClientPannel { get; set; }

        [MaxLength(500)]
        public string Location { get; set; }
        public int? LocationId { get; set; }
        [MaxLength(1000)]
        public string Remarks { get; set; }
        [Required]
        public byte ScheduledBy { get; set; }
        [Required]
        public int UserId { get; set; }
        public string InterviewTimeZone { get; set; }
    }

    public class CancelCandidateInterviewInterview
    {
        [Required]
        public int InterviewId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CanPrfId { get; set; }
        [MaxLength(1000)]
        public string Remarks { get; set; }
    }

    public class ReScheduleScheduleCandidateInterview
    {
        [Required]
        public int InterviewId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CanPrfId { get; set; }
        [Required]
        public byte ModeofInterview { get; set; }
        [Required]
        public DateTime InterviewDate { get; set; }
        [Required]
        public byte InterviewDuration { get; set; }
        [MaxLength(500)]
        public string Location { get; set; }
        [MaxLength(1000)]
        public string Remarks { get; set; }
        [Required]
        public byte ScheduledBy { get; set; }
        public string InterviewEndTime { get; set; }
        public string InterviewStartTime { get; set; }
        public string InterviewTimeZone { get; set; }
        public string ClientPannel { get; set; }
        [Required]
        public string HirePannel { get; set; }
    }

    public class CandidateShareProfileViewModel
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string EmailSubject { get; set; }
        [Required]
        [MaxLength(100)]
        public string ToEmailID { get; set; }
        [MaxLength(1000)]
        public string CCEmailIDs { get; set; }
        [Required]
        [MaxLength(200)]
        public string EmailFields { get; set; }
        [Required]
        [MaxLength(200)]
        public string ConfView { get; set; }

        [Required]
        [MaxLength(200)]
        public string SpocName { get; set; }

        public List<SelectedProfileViewModel> SelectedProfileViewModel { get; set; }
    }

    public class SelectedProfileViewModel
    {
        [Required]
        public int CanPrfId { get; set; }
        public int Experience { get; set; }
        [Required]
        public string CandidateName { get; set; }
        public int NoticePeriod { get; set; }
        public string City { get; set; }
        public string Country { get; set; }       
        public string CbCurrency { get; set; }        
        public int? CbSalary { get; set; }
        public int? RecruiterId { get; set; }
    }


    public class UpdateCandidateInterviewModel
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CanPrfId { get; set; }
        [Required]
        public int InterviewId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Remarks { get; set; }
    }

    public class InterviewRecheduleViewModel
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CanPrfId { get; set; }
        [Required]
        public int InterviewId { get; set; }
        [Required]
        public byte ModeofInterview { get; set; }
        [Required]
        public DateTime InterviewDate { get; set; }
        [Required]
        public string InterviewTime { get; set; }
        [Required]
        public byte InterviewDuration { get; set; }
        [Required]
        [MaxLength(100)]
        public string InterviewerEmail { get; set; }
        [Required]
        [MaxLength(100)]
        public string InterviewerName { get; set; }
        [MaxLength(500)]
        public string Location { get; set; }
        [MaxLength(1000)]
        public string Remarks { get; set; }
        [Required]
        public byte ScheduledBy { get; set; }
    }


    public class UpdateCandidateRecModel
    {
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int JoId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Remarks { get; set; }
        [Required]
        public int RecruiterId { get; set; }

    }


    public class UpdateCandidateRatingModel
    {
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int JoId { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
        [Required]
        public byte Rating { get; set; }
        [Required]
        public byte ScheduledBy { get; set; }
        public int? CanShareId { get; set; }
        [Required]
        public int UserId { get; set; }

    }


    public class CandidateRecHistoryViewModel
    {        
        public string RecruiterName { get; set; }
        public int RecruiterId { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public string UpdatedName { get; set; }
    }

}
