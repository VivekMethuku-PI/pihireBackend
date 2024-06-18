using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class TodayJobAssignment
    {
        public int RecId { get; set; }
        public string RecName { get; set; }
        public short? NoCVSRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }
        public short? ProfilesUploaded { get; set; }
        public int? CvTarget { get; set; }
        public byte? AssignBy { get; set; }
        public int JoId { get; set; }
        public string JobTitle { get; set; }
        public int BdmId { get; set; }
        public string BdmName { get; set; }
        public DateTime? CvTargetDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public DateTime PostedDate { get; set; }
        public int? CvTargetFilled { get; set; }
    }
}
