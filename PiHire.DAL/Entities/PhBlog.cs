using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhBlog
{
    public int Id { get; set; }

    public string AuthorName { get; set; }

    public string Title { get; set; }

    public string BlogDesc { get; set; }

    public string BlogPic { get; set; }

    public string Tags { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string BlogShortDesc { get; set; }
}
