using PiHire.BAL.Common.Types;
using System;
using System.Collections.Generic;
using System.Text;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class ScheduleServiceViewModel
    {
        public int Id { get; set; }
        public CustomSchedulerEventTypes Event { get; set; }
        public DateTime ScheduleDate { get; set; }
                
        public CustomSchedulerFrequency Frequency { get; set; }


        public bool AnySubPending { get; set; }
        public bool IsMissed { get; set; }
    }
}
