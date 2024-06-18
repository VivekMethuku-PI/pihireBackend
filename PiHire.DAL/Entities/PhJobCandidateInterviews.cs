using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities
{
    public partial class PhJobCandidateInterviews
    {
        public int Id { get; set; }
        public int Joid { get; set; }
        public int CandProfId { get; set; }
        public DateTime InterviewDate { get; set; }
        public string InterviewerName { get; set; }
        public string InterviewerEmail { get; set; }
        public byte InterviewStatus { get; set; }
        public string Location { get; set; }
        public string Remarks { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public byte ModeOfInterview { get; set; }
        public byte InterviewDuration { get; set; }
        public byte ScheduledBy { get; set; }
        public string InterviewStartTime { get; set; }
        public string InterviewEndTime { get; set; }
        public string PiTeamEmailIds { get; set; }
        public string InterviewTimeZone { get; set; }
        public string CalendarEventId { get; set; }
    }
}
