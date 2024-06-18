using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class RefMasterViewModel
    {
        public short Id { get; set; }
        public string Rmtype { get; set; }
        public string Rmvalue { get; set; }
        public string Rmdesc { get; set; }
        public int GroupId { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RefDataViewModel
    {
        public short Id { get; set; }
        public string Rmtype { get; set; }
        public string Rmvalue { get; set; }
        public string Rmdesc { get; set; }
        public int GroupId { get; set; }
        public byte Status { get; set; }
    }
    public class ReferenceDataViewModel
    {
        public int Id { get; set; }
        public string Rmvalue { get; set; }
        public string Rmdesc { get; set; }
    }

    public class DocTypesViewModel
    {
        public short Id { get; set; }
        public string Rmtype { get; set; }
        public string Rmvalue { get; set; }
        public string Rmdesc { get; set; }
        public int FileGroup { get; set; }
        public int GroupId { get; set; }
    }



    public class CreateRefValuesViewModel
    {
        public short Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string Rmtype { get; set; }
        [MaxLength(100)]
        [Required]
        public string Rmvalue { get; set; }
        [MaxLength(100)]
        public string Rmdesc { get; set; }
        [Required]
        public int GroupId { get; set; }
    }
    public class CreateReferenceViewModel
    {
        [MaxLength(100)]
        [Required]
        public string Rmvalue { get; set; }
        [MaxLength(100)]
        public string Rmdesc { get; set; }
    }

    public class CreateRefValueViewModel
    {
        public int id { get; set; }       
        public string type { get; set; }    
        public string value { get; set; }
        public string oldvalue { get; set; }
        public int groupid { get; set; }        
        public string description { get; set; }
    }



    public class UpdateRefValuesViewModel
    {
        [Required]
        public short Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string Rmtype { get; set; }
        [MaxLength(100)]
        [Required]
        public string Rmvalue { get; set; }
        [MaxLength(100)]
        public string Rmdesc { get; set; }
        [Required]
        public int GroupId { get; set; }
    }

}
