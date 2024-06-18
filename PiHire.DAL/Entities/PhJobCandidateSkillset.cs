using System;

namespace PiHire.DAL.Entities
{
    public partial class PhJobCandidateSkillset
    {
        public int Id { get; set; }

        public byte GroupType { get; set; }
        public int JoCandId { get; set; }
        public int Joid { get; set; }
        public int CandProfId { get; set; }

        public int TechnologyId { get; set; }
        public int ExpInYears { get; set; }
        public int ExpInMonths { get; set; }

        public int Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
