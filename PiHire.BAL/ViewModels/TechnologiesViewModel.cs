using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class TechnologiesViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public byte Status { get; set; }
    }  

    public class GetTechnologiesViewModel
    {
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        public string SearchKey { get; set; }
    }

    public class CreateTechnologiesViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
    }

    public class UpdateTechnologiesViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
    }

    public class CreateSkillProfileViewModel
    {
        [Required]
        [MaxLength(200)]
        public string SkillProfileName { get; set; }
        [Required]
        public List<int> TechnologyId { get; set; }
    }


    public class UpdateSkillProfileViewModel
    {
        [Required]
        public int Id { get; set; }
        [MaxLength(200)]
        public string SkillProfileName { get; set; }
        [Required]
        public List<int> TechnologyId { get; set; }
    }


    public class SkillProfilesViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TechnologiesViewModel> TechnologiesViewModel { get; set; }
    }

}
