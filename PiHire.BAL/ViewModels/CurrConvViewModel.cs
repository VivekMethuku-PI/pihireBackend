using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class CurrConvViewModel
    {
        [Required]
        [MinLength(2)]
        public string FrmCurn { get; set; }
        [Required]
        [MinLength(2)]
        public string ToCurn { get; set; }
    }
}
