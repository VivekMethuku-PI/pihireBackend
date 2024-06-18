using Microsoft.AspNetCore.Http;
using PiHire.BAL.Common.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class TestimonialsModel
    {
        public int Id { get; set; }
        public int? CandidateId { get; set; }
        public string Title { get; set; }
        public string Tdesc { get; set; }
        public string ProfilePic { get; set; }
        public string Designation { get; set; }
        public byte Rating { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TestimonialModel
    {
        public int Id { get; set; }
        public int? CandidateId { get; set; }
        public string Title { get; set; }
        public string Tdesc { get; set; }
        public string ProfilePic { get; set; }
        public byte Rating { get; set; }
        public string Designation { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }


    public class CreateTestimonialModel
    {
        public int? CandidateId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Tdesc { get; set; }
        [Required]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        public IFormFile File { get; set; }
        [Required]
        public byte Rating { get; set; }
        [MaxLength(100)]
        public string Designation { get; set; }
    }

    public class UpdateTestimonialModel
    {
        [Required]
        public int Id { get; set; }
        public int? CandidateId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Tdesc { get; set; }
        [Required]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        public IFormFile File { get; set; }
        [Required]
        public byte Rating { get; set; }
        [MaxLength(100)]
        public string Designation { get; set; }
    }

}
