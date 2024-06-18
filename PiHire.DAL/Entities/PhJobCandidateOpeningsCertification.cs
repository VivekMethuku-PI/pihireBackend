using System;

namespace PiHire.DAL.Entities
{
    public partial class PhJobCandidateOpeningsCertification
    {
        public int Id { get; set; }
        public int JoCandId { get; set; }
        public int Joid { get; set; }
        public int CandProfId { get; set; }

        public byte GroupType { get; set; }
        public int CertificationId { get; set; }
        public bool? PrefCertification { get; set; }
        public int? PrefCertificationId { get; set; }

        public byte Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
