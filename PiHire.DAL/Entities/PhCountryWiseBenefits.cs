using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.DAL.Entities
{
    public class PhCountryWiseBenefit
    {
        public int Id { get; set; }

        public int CountryId { get; set; }

        public bool IsSalaryWise { get; set; }

        public string BenefitTitle { get; set; }

        public string BenefitDesc { get; set; }

        public byte Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

    }
}
