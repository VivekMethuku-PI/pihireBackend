using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities
{
    public partial class TblParamEmployeeProcessunit
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public int? ProcessUnitId { get; set; }
        public bool? ActiveStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
