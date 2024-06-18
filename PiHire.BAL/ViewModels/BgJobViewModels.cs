using PiHire.BAL.Common.Types;
using System;
using System.Collections.Generic;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class CreateJobBgViewModel
    {
        public int EmailTemplateId { get; set; }
        public int JobId { get; set; }
    }

    public class BgJobSummaryViewModel
    {
        public BgJobSummaryViewModel()
        {
            Details = new List<BgJobDetailsViewModel>();
        }

        public int Id { get; set; }
        public CustomSchedulerEventTypes EventType { get; set; }
        public string EventTypeCode { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public CustomSchedulerFrequency Frequency { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string ScheduleTime { get; set; }
        public int EmailTemplateId { get; set; }
        public CustomSchedulerSendTo SendTo { get; set; }
        public string Pus { get; set; }
        public string Bus { get; set; }
        public string CandidateStatus { get; set; }
        public string Gender { get; set; }
        public string CountryIds { get; set; }
        public RecordStatus Status { get; set; }

        public List<BgJobDetailsViewModel> Details { get; set; }

        public DateTime? LastExecutedOn { get; set; }
        public int SendCount { get; set; }
        public int DeliveredCount { get; set; }
    }
    public class BgJobDetailsViewModel
    {
        public string JobName { get; set; }


        public DateTime ExecutedOn { get; set; }
        public int SendCount { get; set; }
        public int DeliveredCount { get; set; }
        public string ServiceProvider { get; set; }
        public string Remarks { get; set; }
        public byte ExecutionStatusId { get; set; }
        public string ExecutionStatus { get; set; }
    }
}
