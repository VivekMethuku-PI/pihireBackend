using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class SlabComponentViewModel
    {
        public int Id { get; set; }
        public int Puid { get; set; }
        public int SlabId { get; set; }
        public int CompId { get; set; }
        public string CompName { get; set; }
        public int ComType { get; set; }
        public string ComTypeNam { get; set; }
        public string SlabName { get; set; }
        public decimal Amount { get; set; }
        public bool? PercentageFlag { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateSlabComponentViewModel
    {
        [Required]
        public int Puid { get; set; }
        [Required]
        public int SlabId { get; set; }
        [Required]
        public int CompId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public bool? PercentageFlag { get; set; }
    }

    public class UpdateSlabComponentViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int Puid { get; set; }
        [Required]
        public int SlabId { get; set; }
        [Required]
        public int CompId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public bool? PercentageFlag { get; set; }
    }


    public class SlabComponentDtlsViewModel
    {
        public int Id { get; set; }
        public int Puid { get; set; }
        public int SlabId { get; set; }
        public int CompId { get; set; }
        public string CompName { get; set; }
        public int CompType { get; set; }
        public string CompTypeName { get; set; }
        public string SlabName { get; set; }
        public decimal Amount { get; set; }
        public bool? PercentageFlag { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DownloadSlabComponentDtlsViewModel
    {
        public int Id { get; set; }
        public string SlabName { get; set; }
        public string CompName { get; set; }
        public string CompTypeName { get; set; }      
        public decimal Amount { get; set; }
    }

    public class GrpBySlabModel
    {
        public string CompTypeName { get; set; }
        public List<SlabComponentDtlsViewModel> slabComponentDtlsViewModels { get; set; }
    }

    public class DownloadGrpBySlabModel
    {
        public string CompTypeName { get; set; }
        public List<DownloadSlabComponentDtlsViewModel> slabComponentDtlsViewModels { get; set; }
    }

}
