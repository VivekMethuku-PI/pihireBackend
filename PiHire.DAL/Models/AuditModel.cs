using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{

    // Audits
    public class AuditModel
    {
        public List<AuditList> Audits { get; set; }
        public int AuditCount { get; set; }
    }
    public class AuditCounts
    {
        public int AuditCount { get; set; }
    }
    public class AuditList
    {
        public int ActivityType { get; set; }
        public int TaskId { get; set; }
        public string ActivitySubject { get; set; }
        public string ActivityDesc { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }

    // Activities
    public class ActivitesModel
    {
        public List<ActivitesList> Activites { get; set; }
        public int ActiviteCount { get; set; }
    }
    public class ActiviteCounts
    {
        public int ActiviteCount { get; set; }
    }
    public class ActivitesList
    {
        public int Id { get; set; }
        public int? Joid { get; set; }
        public byte ActivityType { get; set; }
        public string ActivityTypeName { get; set; }
        public byte ActivityMode { get; set; }
        public string ActivityModeName { get; set; }      
        public string ActivityDesc { get; set; }
        public int? ActivityOn { get; set; }
        public byte Status { get; set; }
        public string ProfilePhoto { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    // Notes

    public class NotesList
    {
        public int Id { get; set; }
        public int? CandId { get; set; }
        public string Title { get; set; }
        public string NotesDesc { get; set; }
        public int? NoteId { get; set; }
        public string Role { get; set; }
        public int? Joid { get; set; }     
        public byte Status { get; set; }
        public string ProfilePhoto { get; set; }
        public DateTime CreatedDate { get; set; }    
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
