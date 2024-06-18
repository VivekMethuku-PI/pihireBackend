using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class OpeningStatusListViewModel
    {
        public short Id { get; set; }
        public string Title { get; set; }
        public string StatusDesc { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }

    public class CreateOpeningStatusViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [Required]
        [MaxLength(100)]
        public string StatusDesc { get; set; }
    }


    public class EditOpeningStatusViewModel
    {
        [Required]
        public int ID { get; set; }
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [Required]
        [MaxLength(100)]
        public string StatusDesc { get; set; }
    }


    public class UpdateOpeningStatusViewModel
    {
        [Required]
        public int ID { get; set; }
        [Required]
        [Range(0, 1)]
        //[EnumDataType(typeof(RecordStatus))]
        public byte Status { get; set; }
    }
}
