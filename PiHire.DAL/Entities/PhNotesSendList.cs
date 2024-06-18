using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhNotesSendList
{
    public int Id { get; set; }

    public int NotesId { get; set; }

    public int SendTo { get; set; }

    public byte Status { get; set; }
}
