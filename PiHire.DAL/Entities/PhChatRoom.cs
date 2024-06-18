using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhChatRoom
{
    public int Id { get; set; }

    public int? Joid { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; }

    public string Crdesc { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
