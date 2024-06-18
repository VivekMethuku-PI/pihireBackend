using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.DAL.Entities
{
    public class PhCountryWiseAllowance
    {
        public int Id { get; set; }

        public int CountryId { get; set; }

        public bool? IsCitizenWise { get; set; }

        public string AllowanceCode { get; set; }

        public string AllowanceTitle { get; set; }

        public string AllowanceDesc { get; set; }

        public decimal? AllowancePrice { get; set; }

        public decimal? AllowancePercentage { get; set; }

        public byte Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

    }
}
