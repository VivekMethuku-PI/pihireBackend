using Microsoft.AspNetCore.Http;
using PiHire.BAL.Common.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class CreateMediaViewModel
    {
        [Required]
        public byte FileGroup { get; set; }

        [Required]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg", ".doc", ".docx", ".pdf" })]
        public IFormFile File { get; set; }
    }
}
