using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class SlabViewModel
    {
        public int Id { get; set; }
        public int Puid { get; set; }
        public string Title { get; set; }
        public string SlabDesc { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateSlabViewModel
    {
        [Required]
        public int Puid { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string SlabDesc { get; set; }
    }

    public class UpdateSlabViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int Puid { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string SlabDesc { get; set; }
    }

}
