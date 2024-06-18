using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class SalayComponentViewModel
    {
        public int Id { get; set; }
        public int CompType { get; set; }
        public string Title { get; set; }
        public string CompDesc { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CompTypeName { get; set; }
    }

    public class CreateSalayComponentViewModel
    {
        [Required]
        public int CompType { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string CompDesc { get; set; }
    }

    public class UpdateSalayComponentViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int CompType { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string CompDesc { get; set; }
    }
}
