using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PiHire.BAL.ViewModels
{
    public class DisplayRuleViewmodel
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public byte StatusIdIsEnable { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public List<DisplayRuleNextOrderViewmodel> DisplayRuleNextOrderViewmodels { get; set; }
    }


    public class NextCandidateStatusViewmodel
    {       
        public int StatusId { get; set; }
        public string StatusName { get; set; }
    }

        public class DisplayRuleNextOrderViewmodel
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public int NextStatusId { get; set; }
        public byte NextStatusIdIsEnable { get; set; }
        public int DisplayOrder { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
    }

    public class CreateDisplayRuleViewmodel
    {
        [Required]
        public int StatusId { get; set; }
        [Required]
        public List<int> NextStatusId { get; set; }
    }

    public class UpdateDisplayRuleViewmodel
    {
        [Required]
        public int StatusId { get; set; }
        [Required]
        [Range(0, 1)]
        //[EnumDataType(typeof(RecordStatus))]
        public byte Status { get; set; }
    }

    public class DeleteDisplayRuleViewmodel
    {
        [Required]
        public int StatusId { get; set; }
    }

    public class EditDisplayRuleViewmodel
    {
        [Required]
        public int StatusId { get; set; }
        [Required]
        public List<int> NextStatusId { get; set; }
    }

}
