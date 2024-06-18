using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{


    public class TechnologyModel
    {
        public List<TechnologiesModel> TechnologiesModel { get; set; }
        public int TechCount { get; set; }
    }
    public class TechnologiesModel
    {
        public short Id { get; set; }
        public string Title { get; set; }
        public byte Status { get; set; }
    }
    public class TechnologiesModelCount
    {
        public int TechCount { get; set; }
    }
      
}
