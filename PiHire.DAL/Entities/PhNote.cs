using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhNote
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string NotesDesc { get; set; }

    public byte Status { get; set; }

    public int? NoteId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int Joid { get; set; }

    public int? CandId { get; set; }
}
