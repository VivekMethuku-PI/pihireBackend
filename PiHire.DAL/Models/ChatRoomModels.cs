using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class GetChatRoomModel
    {
        public int RoomId { get; set; }
        public DateTime? RoomUpdatedDate { get; set; }
        public int? JobId { get; set; }
        public int Count { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? LatestMessageDt { get; set; }
        public int? ReceiverId { get; set; }
    }


    public class GetChatRoomCountModel
    {
        public int RoomsCount { get; set; }
    }
}
