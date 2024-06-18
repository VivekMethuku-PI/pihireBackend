using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class CandidateStatusListViewModel
    {
        public short Id { get; set; }
        public string Title { get; set; }
        public string StatusDesc { get; set; }
        public byte Status { get; set; }
    }


    public class CreateCandidateStatusViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [Required]
        [MaxLength(100)]
        public string StatusDesc { get; set; }
    }

    public class EditCandidateStatusViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [Required]
        [MaxLength(100)]
        public string StatusDesc { get; set; }
    }


    public class UpdateCandidateStatusViewModel
    {
        [Required]
        public int ID { get; set; }
        [Required]
        [Range(0, 1)]
        public byte Status { get; set; }
    }


}

