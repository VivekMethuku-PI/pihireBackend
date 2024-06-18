using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.ViewModels
{
    public class RecruiterJobAssignmentSearchViewModel
    {
        public int JoId { get; set; }
        public int RecruiterId { get; set; }
        public DateTime AssignmentDate { get; set; }
    }
    public class RecruiterJobAssignmentSearchResponseViewModel
    {
        public int JoId { get; set; }
        public int RecruiterId { get; set; }
        public DateTime AssignmentDate { get; set; }
        public short? NoOfCvs { get; set; }
        public short? CumulativeNoOfCvs { get; set; }
        public DateTime? DeassignDate { get; internal set; }
    }
    public class RecruiterJobAssignmentViewModel
    {
        public int JoId { get; set; }
        public int RecruiterId { get; set; }
        public DateTime AssignmentDate { get; set; }
        public short NoOfCvs { get; set; }
        public bool IsReplacement { get; set; }
    }
}
