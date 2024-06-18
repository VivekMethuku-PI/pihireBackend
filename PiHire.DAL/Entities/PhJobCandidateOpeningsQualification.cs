using System;

namespace PiHire.DAL.Entities
{
    public partial class PhJobCandidateOpeningsQualification
    {
        public int Id { get; set; }
        public int JoCandId { get; set; }
        public int Joid { get; set; }
        public int CandProfId { get; set; }

        public byte GroupType { get; set; }
        public int QualificationId { get; set; }
        public bool? PrefQualification { get; set; }
        public int? PrefQualificationId { get; set; }
        public int? CourseId { get; set; }
        public bool? PrefCourse { get; set; }
        public int? PrefCourseId { get; set; }

        public byte Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
