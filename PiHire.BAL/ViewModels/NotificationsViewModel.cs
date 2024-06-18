using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class UserNotificationsViewModel
    {
        public int Id { get; set; }
        public int? JobId { get; set; }
        public string Title { get; set; }
        public string NoteDesc { get; set; }
        public byte UserType { get; set; }
        public string EmailId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public byte NotiStatus { get; set; }
        public string ProfilePhoto { get; set; }
        public string Photo { get; set; }
        public int? CandProfId { get; set; }
        public NotiPushedUser NotiPushedUser { get; set; }
    }

    public class NotiPushedUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }

    public class NotificationPushedViewModel
    {
        public int? JobId { get; set; }
        public string Title { get; set; }
        public string NoteDesc { get; set; }
        public int CreatedBy { get; set; }
        public int[] PushedTo { get; set; }
        public bool IsAudioNotify { get; set; } = false;
    }
}
