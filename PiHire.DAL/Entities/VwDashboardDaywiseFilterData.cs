using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.DAL.Entities
{
    public class VwDashboardDaywiseFilterData
    {
        public int JobId { get; set; }
        public int? BdmId { get; set; }
        public int RecruiterId { get; set; }
        public short? NoCvsrequired { get; set; }
        public short? NoCvsuploadded { get; set; }
        public DateTime? JobAssignmentDate { get; set; }
        public int? Puid { get; set; }
        public int? Buid { get; set; }
    }
}
