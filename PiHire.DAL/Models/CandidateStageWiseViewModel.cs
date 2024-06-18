using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.DAL.Models
{
    public class CandidateStageWiseViewModel
    {
        public int ID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CandProfStatusId { get; set; }
        public string CandProfStatus { get; set; }
        public string Stage { get; set; }
        public string Age { get; set; }
    }
}
