using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class ResendUserCredViewModel
    {
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int JoId { get; set; }
    }
}
