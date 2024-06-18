using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class AssignedJobsReqViewModel
    {
        public int RecUserId { get; set; }
    }

    public class AssignedJobsViewModel
    {
        public int JoId { get; set; }
        public string JobName { get; set; }
        public string AccountName { get; set; }
        public string BDMName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime EndDate { get; set; }
        public short? RequiredCv { get; set; }
        public short? SubmittedCV { get; set; }
    }
}
