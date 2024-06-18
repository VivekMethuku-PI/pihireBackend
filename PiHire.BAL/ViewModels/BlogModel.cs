using Microsoft.AspNetCore.Http;
using PiHire.BAL.Common.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class BlogModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }
        public string Title { get; set; }
        public string BlogDesc { get; set; }
        public string BlogShortDesc { get; set; }
        public string BlogPic { get; set; }
        public string Tags { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class BlogsModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }
        public string Title { get; set; }
        public string BlogDesc { get; set; }
        public string BlogShortDesc { get; set; }
        public string BlogPic { get; set; }
        public string Tags { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateBlogModel
    {
        [Required]
        [MaxLength(100)]
        public string AuthorName { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string BlogShortDesc { get; set; }
        [Required]
        public string BlogDesc { get; set; }
        [Required]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        public IFormFile File { get; set; }
        [MaxLength(1000)]
        public string Tags { get; set; }
    }

    public class UpdateBlogModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string AuthorName { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string BlogShortDesc { get; set; }
        [Required]
        public string BlogDesc { get; set; }
        [Required]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        public IFormFile File { get; set; }
        [MaxLength(1000)]
        public string Tags { get; set; }
    }


}
