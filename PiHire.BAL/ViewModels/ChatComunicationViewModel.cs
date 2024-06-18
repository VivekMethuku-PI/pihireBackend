using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{

    public class SendMessageModel
    {
        public IFormFile File { get; set; }
        public string Model { get; set; }
    }

    public class SendMessageViewModel
    {
        public int ChatRoomId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
        public string ReceiverName { get; set; }
    }
    public class GetChatMessagesResponseVM
    {
        public int RoomId { get; set; }
        public List<GetChatComunicationViewModel> Messages { get; set; }
    }

    public class UpdateReadStatusViewModel
    {
        public int RoomId { get; set; }
    }
    public class ChatPaginationViewModel
    {
        public int TotalCount { get; set; }
        public List<_ChatPaginationViewModel> Rooms { get; set; }
    }
    public class _ChatPaginationViewModel
    {
        public int RoomId { get; set; }
        public DateTime? RoomUpdatedDate { get; set; }
        public int Count { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? LatestMessageDt { get; set; }

        public int? JobId { get; set; }
        public string JobTitle { get; set; }


        public int? ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoto { get; set; }
        public int? CandidateId { get; set; }
    }

    public class GetUnreadCountViewModel
    {
        public int RoomId { get; set; }
        public int Count { get; set; }
        public DateTime? LatestMessageDt { get; set; }
        public int? JobId { get; set; }
    }

    public class RoomMediaViewModel
    {
        public string FileUrl { get; set; }
        public int MessageId { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
    }

    public class GetRoomsViewModel
    {
        public int RoomId { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public string Role { get; set; }
        public string ProfileImage { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string JobTitle { get; set; }
        public DateTime? LatestMessageDt { get; set; }
    }

    public class GetChatComunicationViewModel
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public byte ReadStatus { get; set; }
        public string ReceiverName { get; set; }
        public string SenderName { get; set; }
        public int RoomId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? chatMsgId { get; set; }
    }

    public class GetChatComunicationTempViewModel
    {
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public string message { get; set; }
        public string fileName { get; set; }
        public byte readStatus { get; set; }
        public string receiverName { get; set; }
        public string senderName { get; set; }
        public int roomId { get; set; }
        public DateTime? createdDate { get; set; }
    }
    public class GetChatMessagesViewModel
    {
        public int ReceiverId { get; set; }
        public int JobId { get; set; }
    }

    public class GetRoomReuestViewModel
    {
        public int JobId { get; set; }
    }
}
