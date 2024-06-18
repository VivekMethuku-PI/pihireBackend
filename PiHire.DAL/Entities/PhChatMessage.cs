using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhChatMessage
{
    public int Id { get; set; }

    public int ChatRoomId { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string Message { get; set; }

    public string FileName { get; set; }

    public byte ReadStatus { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }
}
