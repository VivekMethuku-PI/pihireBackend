using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{

    public class CreateStageViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [Required]
        [MaxLength(100)]
        public string StageDesc { get; set; }
        [Required]
        [MaxLength(10)]
        public string ColorCode { get; set; }
    }

    public class EditStageViewModel
    {
        [Required]
        public short Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [Required]
        [MaxLength(100)]
        public string StageDesc { get; set; }
        [Required]
        [MaxLength(10)]
        public string ColorCode { get; set; }
    }


    public class UpdateStageViewModel
    {
        [Required]
        public short Id { get; set; }
    }




    public class MapCandidateStatusViewModel
    {
        [Required]
        public int StageId { get; set; }
        [Required]
        public List<int> CandStatusId { get; set; }
    }

    public class UpdateStageCandidateStatusViewModel
    {
        [Required]
        public int CandStatusMapId { get; set; }
    }

    public class StageViewModel
    {
        public short Id { get; set; }
        public string Title { get; set; }
        public string StageDesc { get; set; }
        public string ColorCode { get; set; }
        public byte Status { get; set; }
    }

    public class StageStatusListViewModel
    {
        public short Id { get; set; }
        public string Title { get; set; }
        public string StageDesc { get; set; }
        public string ColorCode { get; set; }
        public byte Status { get; set; }

        public List<StageCandidateStatusListViewModel> StageCandidateStatusListViewModel { get; set; }
    }

    public class StageCandidateStatusListViewModel
    {
        public int CandStageMapId { get; set; }
        public short CandidateStatusId { get; set; }
        public byte CandidateStatusIdIsEnable { get; set; }
        public string Title { get; set; }
        public string StatusDesc { get; set; }
        public byte Status { get; set; }
    }

}
