using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class TemplatesViewModel
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string Code { get; set; }
        public byte MessageType { get; set; }
        public string MessageTypeName { get; set; }
        public byte ProfileType { get; set; }
        public string ProfileTypeName { get; set; }
        public string TplDesc { get; set; }
        public string TplSubject { get; set; }
        public byte SentBy { get; set; }
        public string SentByName { get; set; }
        public string TplBody { get; set; }
        public string TplFullBody { get; set; }
        public string DynamicLabels { get; set; }
        public bool PublishStatus { get; set; }
        public int? IndustryId { get; set; }
        public string IndustryName { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class JobTemplatesViewModel
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string Template { get; set; }
    }

    public class TemplateSerachViewModel
    {
        [Required]
        public byte MessageType { get; set; }
        public int? IndustryId { get; set; }
    }


    public class UpdateJobTemplateViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public byte MessageType { get; set; }
        [Required]
        public byte ProfileType { get; set; }
        [Required]
        [MaxLength(50)]
        public string TplTitle { get; set; }
        [MaxLength(100)]
        public string TplDesc { get; set; }
        [MaxLength(100)]
        public string TplSubject { get; set; }
        [Required]
        public byte SentBy { get; set; }
        [Required]
        public string TplBody { get; set; }
        [MaxLength(1000)]
        public string DynamicLabels { get; set; }
        [Required]
        public string TplFullBody { get; set; }
        public int? IndustryId { get; set; }
    }

    public class CreateJobTemplateViewModel
    {
        [Required]
        public byte MessageType { get; set; }
        [Required]
        public byte ProfileType { get; set; }
        [Required]
        [MaxLength(50)]
        public string TplTitle { get; set; }
        [MaxLength(100)]
        public string TplDesc { get; set; }
        [MaxLength(100)]
        public string TplSubject { get; set; }
        [Required]
        public byte SentBy { get; set; }
        [Required]
        public string TplBody { get; set; }
        [MaxLength(1000)]
        public string DynamicLabels { get; set; }
        [Required]        
        public string TplFullBody { get; set; }
        public int? IndustryId { get; set; }

    }

    public class UpdateJobTemplateStatus
    {
        public int Id { get; set; }
        public byte Status { get; set; }
    }



}
